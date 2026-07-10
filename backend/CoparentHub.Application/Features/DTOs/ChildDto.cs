namespace CoparentHub.Application.Features.DTOs
{
    public record ChildDto(
      Guid Id,
      string Name,
      DateOnly? DateOfBirth,
      string? Allergies,
      string? Medications,
      string? MedicalNotes,
      string? DoctorName,
      string? DoctorPhone,
      string? SchoolName,
      string? SchoolContact,
      string? ClothingSize,
      string? ShoeSize,
      string? EmergencyContactName,
      string? EmergencyContactPhone
  );
}
