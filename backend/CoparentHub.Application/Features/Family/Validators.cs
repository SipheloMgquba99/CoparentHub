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
}
