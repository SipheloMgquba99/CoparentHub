namespace CoparentHub.Application.Features.DTOs
{
    public record MemberDto(
     Guid UserId,
     string FullName,
     string Email
 );
}
