using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Family
{
    public record CreateFamilyCommand(
      string Name,
      Guid UserId
  ) : IRequest<Result<Guid>>;

    public record JoinFamilyCommand(
        Guid FamilyId,
        Guid UserId
    ) : IRequest<Result<Guid>>;

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
