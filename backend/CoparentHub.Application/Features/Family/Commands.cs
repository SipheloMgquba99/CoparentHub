using CoparentHub.Application.Features.DTOs;
using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Family
{
    public record CreateFamilyCommand(
      string Name,
      Guid UserId
  ) : IRequest<Result<Guid>>;

    public record JoinFamilyByCodeCommand(
        string Code,
        Guid UserId
    ) : IRequest<Result<Guid>>;

    public record CreateFamilyInviteCommand(
        Guid FamilyId,
        Guid UserId
    ) : IRequest<Result<FamilyInviteDto>>;

    public record SendFamilyInviteEmailCommand(
        Guid FamilyId,
        Guid UserId,
        string Email
    ) : IRequest<Result<FamilyInviteDto>>;

    public record AddChildCommand(
        Guid FamilyId,
        Guid UserId,
        string Name,
        DateOnly? DateOfBirth
    ) : IRequest<Result<Guid>>;

    public record RemoveChildCommand(
        Guid FamilyId,
        Guid ChildId,
        Guid UserId
    ) : IRequest<Result<Guid>>;
}
