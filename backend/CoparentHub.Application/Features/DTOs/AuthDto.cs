namespace CoparentHub.Application.Features.DTOs
{
    public record AuthDto(Guid UserId, string Token, string FullName, string Email);
}
