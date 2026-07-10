using FluentValidation;

namespace CoparentHub.Application.Features.Family
{
    public class CreateFamilyValidator : AbstractValidator<CreateFamilyCommand>
    {
        public CreateFamilyValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);
        }
    }

    public class AddChildValidator : AbstractValidator<AddChildCommand>
    {
        public AddChildValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.DateOfBirth)
                .Must(dob => !dob.HasValue || dob.Value <= DateOnly.FromDateTime(DateTime.UtcNow))
                    .WithMessage("Date of birth cannot be in the future.")
                .Must(dob => !dob.HasValue || dob.Value >= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-100))
                    .WithMessage("Date of birth must be within the last 100 years.");
        }
    }

    public class JoinFamilyByCodeValidator : AbstractValidator<JoinFamilyByCodeCommand>
    {
        public JoinFamilyByCodeValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .Length(8);
        }
    }

    public class SendFamilyInviteEmailValidator : AbstractValidator<SendFamilyInviteEmailCommand>
    {
        public SendFamilyInviteEmailValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }

    public class UpdateChildInfoValidator : AbstractValidator<UpdateChildInfoCommand>
    {
        public UpdateChildInfoValidator()
        {
            RuleFor(x => x.Allergies).MaximumLength(500);
            RuleFor(x => x.Medications).MaximumLength(500);
            RuleFor(x => x.MedicalNotes).MaximumLength(500);

            RuleFor(x => x.DoctorName).MaximumLength(150);
            RuleFor(x => x.SchoolName).MaximumLength(150);
            RuleFor(x => x.SchoolContact).MaximumLength(150);
            RuleFor(x => x.EmergencyContactName).MaximumLength(150);

            // Length cap only, no format regex — avoids rejecting valid international numbers.
            RuleFor(x => x.DoctorPhone).MaximumLength(30);
            RuleFor(x => x.EmergencyContactPhone).MaximumLength(30);
            RuleFor(x => x.ClothingSize).MaximumLength(30);
            RuleFor(x => x.ShoeSize).MaximumLength(30);
        }
    }
}
