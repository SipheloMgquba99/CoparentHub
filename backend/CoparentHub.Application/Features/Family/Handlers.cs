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

            if (user.FamilyId.HasValue)
                return Result<Guid>.Fail("User already belongs to a family.");

            var family = CoparentHub.Domain.Entities.Family.Create(cmd.Name, cmd.UserId);

            uow.Families.Add(family);

            user.JoinFamily(family.Id);

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(family.Id);
        }
    }

    public class JoinFamilyHandler(IUnitOfWork uow)
        : IRequestHandler<JoinFamilyCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(JoinFamilyCommand cmd, CancellationToken ct)
        {
            var user = await uow.Users.GetByIdAsync(cmd.UserId, ct);

            if (user is null)
                return Result<Guid>.Fail("User not found.");

            if (user.FamilyId.HasValue)
                return Result<Guid>.Fail("User already belongs to a family.");

            var family = await uow.Families.GetByIdAsync(cmd.FamilyId, ct);

            if (family is null)
                return Result<Guid>.Fail("Family not found.");

            var result = family.AddMember(cmd.UserId);

            if (!result.IsSuccess)
                return Result<Guid>.Fail(result.Error!);

            user.JoinFamily(family.Id);

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(family.Id);
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

            var dto = new FamilyDto(
                family.Id,
                family.Name,
                members,
                children
            );

            return Result<FamilyDto>.Ok(dto);
        }
    }
}
