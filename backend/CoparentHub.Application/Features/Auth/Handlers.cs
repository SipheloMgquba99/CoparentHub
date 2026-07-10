using CoparentHub.Application.Interfaces;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace CoparentHub.Application.Features.Auth
{
    public interface IPasswordHasher { string Hash(string pw); bool Verify(string pw, string hash); }
    public interface ITokenService { string Generate(User user); }

    public class RegisterHandler(IUnitOfWork uow, IPasswordHasher hasher, ITokenService tokens)
        : IRequestHandler<RegisterCommand, Result<AuthDto>>
    {
        public async Task<Result<AuthDto>> Handle(RegisterCommand cmd, CancellationToken ct)
        {
            if (cmd.Password != cmd.ConfirmPassword)
                return Result<AuthDto>.Fail("Passwords do not match.");

            if (await uow.Users.ExistsAsync(cmd.Email, ct))
                return Result<AuthDto>.Fail("An account with this email already exists.");

            var user = User.Create(cmd.FirstName, cmd.LastName, cmd.Email, hasher.Hash(cmd.Password));
            uow.Users.Add(user);
            await uow.SaveAsync(ct);

            return Result<AuthDto>.Ok(new AuthDto(user.Id, tokens.Generate(user), user.FullName, user.Email));
        }
    }

    public class LoginHandler(IUnitOfWork uow, IPasswordHasher hasher, ITokenService tokens, IConfiguration config)
        : IRequestHandler<LoginCommand, Result<AuthDto>>
    {
        private const string InvalidCredentialsMessage = "Invalid email or password.";

        public async Task<Result<AuthDto>> Handle(LoginCommand cmd, CancellationToken ct)
        {
            var user = await uow.Users.GetByEmailAsync(cmd.Email, ct);
            if (user is null)
                return Result<AuthDto>.Fail(InvalidCredentialsMessage);

            // Same generic message as a wrong password, and skip verifying it — avoids both
            // revealing the lockout and letting an attacker keep re-extending it.
            if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
                return Result<AuthDto>.Fail(InvalidCredentialsMessage);

            if (!hasher.Verify(cmd.Password, user.PasswordHash))
            {
                var lockoutThreshold = config.GetValue("Auth:LockoutThreshold", 5);
                var lockoutMinutes = config.GetValue("Auth:LockoutMinutes", 15);
                await uow.Users.RecordFailedLoginAsync(user.Id, lockoutThreshold, TimeSpan.FromMinutes(lockoutMinutes), ct);
                return Result<AuthDto>.Fail(InvalidCredentialsMessage);
            }

            await uow.Users.ResetFailedLoginAsync(user.Id, ct);

            return Result<AuthDto>.Ok(new AuthDto(user.Id, tokens.Generate(user), user.FullName, user.Email));
        }
    }

    public class ForgotPasswordHandler(IUnitOfWork uow, IEmailSender emailSender)
        : IRequestHandler<ForgotPasswordCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ForgotPasswordCommand cmd, CancellationToken ct)
        {
            var user = await uow.Users.GetByEmailAsync(cmd.Email, ct);

            if (user is not null && emailSender.IsConfigured)
            {
                var token = PasswordResetToken.Create(user.Id);
                uow.PasswordResetTokens.Add(token);
                await uow.SaveAsync(ct);

                await emailSender.SendPasswordResetAsync(user.Email, user.FullName, token.Token, token.ExpiresAt, ct);
            }

            // Always report success — never reveal whether an account exists for this email.
            return Result<bool>.Ok(true);
        }
    }

    public class ResetPasswordHandler(IUnitOfWork uow, IPasswordHasher hasher)
        : IRequestHandler<ResetPasswordCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ResetPasswordCommand cmd, CancellationToken ct)
        {
            var token = await uow.PasswordResetTokens.GetByTokenAsync(cmd.Token, ct);

            if (token is null || !token.IsValid)
                return Result<bool>.Fail("This reset link is invalid or has expired.");

            await uow.Users.SetPasswordHashAsync(token.UserId, hasher.Hash(cmd.NewPassword), ct);

            token.MarkUsed();
            await uow.SaveAsync(ct);

            return Result<bool>.Ok(true);
        }
    }

    public class DeleteAccountHandler(IUnitOfWork uow, IPasswordHasher hasher, IEventCacheVersion cacheVersion)
        : IRequestHandler<DeleteAccountCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteAccountCommand cmd, CancellationToken ct)
        {
            var user = await uow.Users.GetByIdAsync(cmd.UserId, ct);

            if (user is null)
                return Result<bool>.Fail("Account not found.");

            if (!hasher.Verify(cmd.Password, user.PasswordHash))
                return Result<bool>.Fail("Incorrect password.");

            var families = await uow.Families.GetByUserIdAsync(cmd.UserId, ct);

            await uow.ExecuteInTransactionAsync(async innerCt =>
            {
                // Sole-member families are deleted outright; families with a co-parent keep
                // their shared history (the departed user's rows have no FK to User, so they're
                // safely orphaned and already display as "Unknown").
                foreach (var family in families.Where(f => f.Members.Count == 1))
                {
                    uow.Families.Remove(family);
                }

                uow.Users.Remove(user);

                await uow.SaveAsync(innerCt);
            }, ct);

            foreach (var family in families)
                cacheVersion.Bump(family.Id);

            return Result<bool>.Ok(true);
        }
    }
}