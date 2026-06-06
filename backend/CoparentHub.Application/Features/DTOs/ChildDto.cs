namespace CoparentHub.Application.Features.DTOs
{
    public record ChildDto(
      Guid Id,
      string Name,
      DateOnly? DateOfBirth
  );
}
