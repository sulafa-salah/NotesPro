using NotesPro.Api.Contracts.Notes;
using NotesPro.Api.Domain;

namespace NotesPro.Api.Contracts.ContractMappings;
    public static class ContractMapping
    {
      
    public static NoteResponse MapToNote(this Note n) =>
      new(
          n.Id!,
          n.Title,
          n.Content,
          n.Tags,
          n.Slug,
          n.Version,
          n.CreatedAtUtc,
          n.UpdatedAtUtc
      );
}