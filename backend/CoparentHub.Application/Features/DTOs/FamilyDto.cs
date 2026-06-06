namespace CoparentHub.Application.Features.DTOs
{
    public record FamilyDto(
     Guid Id,
     string Name,
     List<MemberDto> Members,
     List<ChildDto> Children
 );
}
