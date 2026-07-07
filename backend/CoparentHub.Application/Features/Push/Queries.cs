using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Push
{
    public record GetVapidPublicKeyQuery() : IRequest<Result<string>>;
}
