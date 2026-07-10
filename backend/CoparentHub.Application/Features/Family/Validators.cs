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

            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class DeleteFamilyValidator : AbstractValidator<DeleteFamilyCommand>
    {
        public DeleteFamilyValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class AddChildValidator : AbstractValidator<AddChildCommand>
    {
        public AddChildValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);

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

    public class RemoveChildValidator : AbstractValidator<RemoveChildCommand>
    {
        public RemoveChildValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.ChildId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class JoinFamilyByCodeValidator : AbstractValidator<JoinFamilyByCodeCommand>
    {
        public JoinFamilyByCodeValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .Length(8);

            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class CreateFamilyInviteValidator : AbstractValidator<CreateFamilyInviteCommand>
    {
        public CreateFamilyInviteValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class SendFamilyInviteEmailValidator : AbstractValidator<SendFamilyInviteEmailCommand>
    {
        public SendFamilyInviteEmailValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }

    public class UpdateChildInfoValidator : AbstractValidator<UpdateChildInfoCommand>
    {
        public UpdateChildInfoValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.ChildId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);

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

    public class GetFamilyValidator : AbstractValidator<GetFamilyQuery>
    {
        public GetFamilyValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class GetMyFamiliesValidator : AbstractValidator<GetMyFamiliesQuery>
    {
        public GetMyFamiliesValidator()
        {
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class GetActiveFamilyInviteValidator : AbstractValidator<GetActiveFamilyInviteQuery>
    {
        public GetActiveFamilyInviteValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class GetFamilyInviteStatusValidator : AbstractValidator<GetFamilyInviteStatusQuery>
    {
        public GetFamilyInviteStatusValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class GetPendingInviteValidator : AbstractValidator<GetPendingInviteQuery>
    {
        public GetPendingInviteValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
