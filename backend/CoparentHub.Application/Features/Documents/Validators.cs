using FluentValidation;

namespace CoparentHub.Application.Features.Documents
{
    public class UploadDocumentValidator : AbstractValidator<UploadDocumentCommand>
    {
        private static readonly string[] AllowedContentTypes =
        [
            "application/pdf",
            "image/jpeg",
            "image/png",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ];

        public UploadDocumentValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
            RuleFor(x => x.ChildId).NotEqual(Guid.Empty).When(x => x.ChildId.HasValue);

            RuleFor(x => x.FileName)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.ContentType)
                .Must(ct => AllowedContentTypes.Contains(ct))
                .WithMessage("Only PDF, JPEG, PNG, and Word documents can be uploaded.");

            RuleFor(x => x.Content)
                .NotEmpty()
                .Must(c => c.Length <= 10_000_000)
                .WithMessage("Files must be 10MB or smaller.");

            RuleFor(x => x.Category).IsInEnum();

            RuleFor(x => x.Description).MaximumLength(500);
        }
    }

    public class RemoveDocumentValidator : AbstractValidator<RemoveDocumentCommand>
    {
        public RemoveDocumentValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.DocumentId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class GetDocumentsValidator : AbstractValidator<GetDocumentsQuery>
    {
        public GetDocumentsValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }

    public class GetDocumentContentValidator : AbstractValidator<GetDocumentContentQuery>
    {
        public GetDocumentContentValidator()
        {
            RuleFor(x => x.FamilyId).NotEqual(Guid.Empty);
            RuleFor(x => x.DocumentId).NotEqual(Guid.Empty);
            RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        }
    }
}
