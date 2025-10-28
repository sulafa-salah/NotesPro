using FluentValidation;

namespace NotesPro.Api.Contracts.Notes;

public record UpdateNoteRequest(string Title, string Content, List<string>? Tags, int ExpectedVersion);

public sealed class UpdateNoteRequestValidator : AbstractValidator<UpdateNoteRequest>
{
    public UpdateNoteRequestValidator()
    {
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(0);
        When(x => x.Title is not null, () =>
        {
            RuleFor(x => x.Title!)
                .NotEmpty()
                .MinimumLength(NoteConstraints.TitleMin)
                .MaximumLength(NoteConstraints.TitleMax);
        });
        When(x => x.Content is not null, () =>
        {
            RuleFor(x => x.Content!)
                .NotEmpty()
                .MinimumLength(NoteConstraints.ContentMin)
                .MaximumLength(NoteConstraints.ContentMax);
        });
        When(x => x.Tags is not null, () =>
        {
            RuleFor(x => x.Tags!)
                .Must(t => t.Distinct(StringComparer.Ordinal).Count() == t.Count)
                .WithMessage("Tags must be unique.")
                .Must(t => t.Count <= NoteConstraints.TagsMaxItems);
            RuleForEach(x => x.Tags!).NotEmpty().MaximumLength(NoteConstraints.TagMaxLen);
        });
    }
}

