using FluentValidation;
using NotesPro.Api.Domain;

namespace NotesPro.Api.Contracts.Notes;
    public record CreateNoteRequest(string Title, string Content, List<string>? Tags);

public sealed class CreateNoteRequestValidator : AbstractValidator<CreateNoteRequest>
{
    public CreateNoteRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(NoteConstraints.TitleMin)
            .MaximumLength(NoteConstraints.TitleMax);

        RuleFor(x => x.Content)
            .NotEmpty()
            .MinimumLength(NoteConstraints.ContentMin)
            .MaximumLength(NoteConstraints.ContentMax);

      

        RuleFor(x => x.Tags)
            .NotNull()
            .Must(t => t.Distinct(StringComparer.Ordinal).Count() == t.Count)
                .WithMessage("Tags must be unique.")
            .Must(t => t.Count <= NoteConstraints.TagsMaxItems)
                .WithMessage($"Maximum {NoteConstraints.TagsMaxItems} tags allowed.")
            .ForEach(tag =>
            {
                tag.NotEmpty();
                tag.MaximumLength(NoteConstraints.TagMaxLen);
            });
    }
}


