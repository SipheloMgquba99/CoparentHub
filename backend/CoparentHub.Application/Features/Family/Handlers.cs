using CoparentHub.Application.Features.DTOs;
using CoparentHub.Application.Interfaces;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoparentHub.Application.Features.Family
{
    public class CreateFamilyHandler(IUnitOfWork uow)
    : IRequestHandler<CreateFamilyCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateFamilyCommand cmd, CancellationToken ct)
        {
            var user = await uow.Users.GetByIdAsync(cmd.UserId, ct);

            if (user is null)
                return Result<Guid>.Fail("User not found.");

            var family = CoparentHub.Domain.Entities.Family.Create(cmd.Name, cmd.UserId);

            uow.Families.Add(family);

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(family.Id);
        }
    }

    public class JoinFamilyByCodeHandler(IUnitOfWork uow)
        : IRequestHandler<JoinFamilyByCodeCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(JoinFamilyByCodeCommand cmd, CancellationToken ct)
        {
            var user = await uow.Users.GetByIdAsync(cmd.UserId, ct);

            if (user is null)
                return Result<Guid>.Fail("User not found.");

            var invite = await uow.Invites.GetByCodeAsync(cmd.Code.Trim().ToUpperInvariant(), ct);

            if (invite is null || !invite.IsValid)
                return Result<Guid>.Fail("This invite code is invalid or has expired.");

            var family = await uow.Families.GetByIdAsync(invite.FamilyId, ct);

            if (family is null)
                return Result<Guid>.Fail("Family not found.");

            var result = family.AddMember(cmd.UserId);

            if (!result.IsSuccess)
                return Result<Guid>.Fail(result.Error!);

            invite.MarkUsed();

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(family.Id);
        }
    }

    public class CreateFamilyInviteHandler(IUnitOfWork uow)
        : IRequestHandler<CreateFamilyInviteCommand, Result<FamilyInviteDto>>
    {
        public async Task<Result<FamilyInviteDto>> Handle(CreateFamilyInviteCommand cmd, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(cmd.FamilyId, ct);

            if (family is null)
                return Result<FamilyInviteDto>.Fail("Family not found.");

            if (!family.IsMember(cmd.UserId))
                return Result<FamilyInviteDto>.Fail("Access denied.");

            if (family.Members.Count >= 2)
                return Result<FamilyInviteDto>.Fail("Family already has 2 co-parents.");

            var invite = CoparentHub.Domain.Entities.FamilyInvite.Create(cmd.FamilyId);

            uow.Invites.Add(invite);

            await uow.SaveAsync(ct);

            return Result<FamilyInviteDto>.Ok(new FamilyInviteDto(invite.Code, invite.ExpiresAt));
        }
    }

    public class SendFamilyInviteEmailHandler(IUnitOfWork uow, IEmailSender emailSender)
        : IRequestHandler<SendFamilyInviteEmailCommand, Result<FamilyInviteDto>>
    {
        public async Task<Result<FamilyInviteDto>> Handle(SendFamilyInviteEmailCommand cmd, CancellationToken ct)
        {
            if (!emailSender.IsConfigured)
                return Result<FamilyInviteDto>.Fail(
                    "Email invites aren't available in this environment yet — please share the invite code directly.");

            var resolved = await FamilyInviteResolver.GetOrCreateActiveInvite(uow, cmd.FamilyId, cmd.UserId, ct);

            if (!resolved.IsSuccess)
                return Result<FamilyInviteDto>.Fail(resolved.Error!);

            var (family, invite) = resolved.Value!;

            await uow.SaveAsync(ct);

            var inviter = await uow.Users.GetByIdAsync(cmd.UserId, ct);

            await emailSender.SendFamilyInviteAsync(
                cmd.Email, family.Name, inviter!.FullName, invite.Code, invite.ExpiresAt, ct);

            return Result<FamilyInviteDto>.Ok(new FamilyInviteDto(invite.Code, invite.ExpiresAt));
        }
    }

    public class GetActiveFamilyInviteHandler(IUnitOfWork uow)
        : IRequestHandler<GetActiveFamilyInviteQuery, Result<FamilyInviteDto?>>
    {
        public async Task<Result<FamilyInviteDto?>> Handle(GetActiveFamilyInviteQuery q, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(q.FamilyId, ct);

            if (family is null)
                return Result<FamilyInviteDto?>.Fail("Family not found.");

            if (!family.IsMember(q.UserId))
                return Result<FamilyInviteDto?>.Fail("Access denied.");

            var invite = await uow.Invites.GetActiveByFamilyIdAsync(q.FamilyId, ct);

            return Result<FamilyInviteDto?>.Ok(
                invite is null ? null : new FamilyInviteDto(invite.Code, invite.ExpiresAt));
        }
    }

    public class GetFamilyInviteStatusHandler(IUnitOfWork uow)
        : IRequestHandler<GetFamilyInviteStatusQuery, Result<FamilyInviteStatusDto?>>
    {
        public async Task<Result<FamilyInviteStatusDto?>> Handle(GetFamilyInviteStatusQuery q, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(q.FamilyId, ct);

            if (family is null)
                return Result<FamilyInviteStatusDto?>.Fail("Family not found.");

            if (!family.IsMember(q.UserId))
                return Result<FamilyInviteStatusDto?>.Fail("Access denied.");

            var invite = await uow.Invites.GetLatestUnusedByFamilyIdAsync(q.FamilyId, ct);

            return Result<FamilyInviteStatusDto?>.Ok(
                invite is null ? null : new FamilyInviteStatusDto(invite.ExpiresAt, !invite.IsValid));
        }
    }

    public class AddChildHandler(IUnitOfWork uow)
     : IRequestHandler<AddChildCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(AddChildCommand cmd, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(cmd.FamilyId, ct);

            if (family is null)
                return Result<Guid>.Fail("Family not found.");

            if (!family.IsMember(cmd.UserId))
                return Result<Guid>.Fail("Access denied.");

            var result = family.AddChild(cmd.Name, cmd.DateOfBirth);

            if (!result.IsSuccess)
                return Result<Guid>.Fail(result.Error!);


                await uow.SaveAsync(ct);
          

                return Result<Guid>.Ok(result.Value!.Id);
        }
    }

    public class RemoveChildHandler(IUnitOfWork uow)
        : IRequestHandler<RemoveChildCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RemoveChildCommand cmd, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(cmd.FamilyId, ct);

            if (family is null)
                return Result<Guid>.Fail("Family not found.");

            if (!family.IsMember(cmd.UserId))
                return Result<Guid>.Fail("Access denied.");

            var result = family.RemoveChild(cmd.ChildId);

            if (!result.IsSuccess)
                return Result<Guid>.Fail(result.Error!);

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(cmd.ChildId);
        }
    }

    public class GetFamilyHandler(IUnitOfWork uow)
        : IRequestHandler<GetFamilyQuery, Result<FamilyDto>>
    {
        public async Task<Result<FamilyDto>> Handle(GetFamilyQuery q, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(q.FamilyId, ct);

            if (family is null)
                return Result<FamilyDto>.Fail("Family not found.");

            if (!family.IsMember(q.UserId))
                return Result<FamilyDto>.Fail("Access denied.");

            return Result<FamilyDto>.Ok(await FamilyMapper.ToDto(family, uow, ct));
        }
    }

    public class GetMyFamiliesHandler(IUnitOfWork uow)
        : IRequestHandler<GetMyFamiliesQuery, Result<List<FamilyDto>>>
    {
        public async Task<Result<List<FamilyDto>>> Handle(GetMyFamiliesQuery q, CancellationToken ct)
        {
            var families = await uow.Families.GetByUserIdAsync(q.UserId, ct);

            var dtos = new List<FamilyDto>();
            foreach (var family in families)
                dtos.Add(await FamilyMapper.ToDto(family, uow, ct));

            return Result<List<FamilyDto>>.Ok(dtos);
        }
    }

    internal static class FamilyInviteResolver
    {
        public static async Task<Result<(CoparentHub.Domain.Entities.Family Family, CoparentHub.Domain.Entities.FamilyInvite Invite)>> GetOrCreateActiveInvite(
            IUnitOfWork uow, Guid familyId, Guid userId, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(familyId, ct);

            if (family is null)
                return Result<(CoparentHub.Domain.Entities.Family, CoparentHub.Domain.Entities.FamilyInvite)>.Fail("Family not found.");

            if (!family.IsMember(userId))
                return Result<(CoparentHub.Domain.Entities.Family, CoparentHub.Domain.Entities.FamilyInvite)>.Fail("Access denied.");

            if (family.Members.Count >= 2)
                return Result<(CoparentHub.Domain.Entities.Family, CoparentHub.Domain.Entities.FamilyInvite)>.Fail("Family already has 2 co-parents.");

            var invite = await uow.Invites.GetActiveByFamilyIdAsync(familyId, ct);

            if (invite is null)
            {
                invite = CoparentHub.Domain.Entities.FamilyInvite.Create(familyId);
                uow.Invites.Add(invite);
            }

            return Result<(CoparentHub.Domain.Entities.Family, CoparentHub.Domain.Entities.FamilyInvite)>.Ok((family, invite));
        }
    }

    internal static class FamilyMapper
    {
        public static async Task<FamilyDto> ToDto(CoparentHub.Domain.Entities.Family family, IUnitOfWork uow, CancellationToken ct)
        {
            var members = new List<MemberDto>();

            foreach (var m in family.Members)
            {
                var user = await uow.Users.GetByIdAsync(m.UserId, ct);

                if (user is not null)
                {
                    members.Add(new MemberDto(
                        user.Id,
                        user.FullName,
                        user.Email
                    ));
                }
            }

            var children = family.Children
                .Select(c => new ChildDto(c.Id, c.Name, c.DateOfBirth))
                .ToList();

            return new FamilyDto(family.Id, family.Name, members, children);
        }
    }
}
