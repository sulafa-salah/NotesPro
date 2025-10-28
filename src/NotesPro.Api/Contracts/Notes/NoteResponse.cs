namespace NotesPro.Api.Contracts.Notes;

public record NoteResponse(
        string Id,
        string Title,
        string Content,
        List<string> Tags,
        string Slug,
        int Version,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);

