using CoparentHub.Application.Features.DTOs;
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
