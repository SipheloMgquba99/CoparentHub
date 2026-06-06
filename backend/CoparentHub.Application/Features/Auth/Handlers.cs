using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using MediatR;

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

    public class LoginHandler(IUnitOfWork uow, IPasswordHasher hasher, ITokenService tokens)
        : IRequestHandler<LoginCommand, Result<AuthDto>>
    {
        public async Task<Result<AuthDto>> Handle(LoginCommand cmd, CancellationToken ct)
        {
            var user = await uow.Users.GetByEmailAsync(cmd.Email, ct);
            if (user is null || !hasher.Verify(cmd.Password, user.PasswordHash))
                return Result<AuthDto>.Fail("Invalid email or password.");

            return Result<AuthDto>.Ok(new AuthDto(user.Id, tokens.Generate(user), user.FullName, user.Email));
        }
    }
}