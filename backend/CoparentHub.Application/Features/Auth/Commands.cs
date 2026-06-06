using CoparentHub.Application.Validations;
using CoparentHub.Domain.Common;
using MediatR;

namespace CoparentHub.Application.Features.Auth
{
    public record RegisterCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string ConfirmPassword) : IRequest<Result<AuthDto>>;
    public record LoginCommand(string Email, string Password) : ICommand<AuthDto>, IRequest<Result<AuthDto>>;
    public record AuthDto(Guid UserId, string Token, string FullName, string Email);
}
