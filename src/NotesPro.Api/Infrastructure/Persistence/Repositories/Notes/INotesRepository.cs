using NotesPro.Api.Domain;

namespace NotesPro.Api.Infrastructure.Persistence.Repositories.Notes;
    public interface INotesRepository
    {
        Task<string> CreateAsync(Note note, CancellationToken ct);
    Task<Note?> GetBySlugAsync(string slug, CancellationToken ct);
        Task<Note?> GetAsync(string id, CancellationToken ct);

        Task<(IReadOnlyList<Note> Items, long Total)> SearchAsync(string? q, List<string>? tags, int page, int pageSize, CancellationToken ct);
        Task<bool> UpdateAsync(string id, Action<Note> mutate, int expectedVersion, CancellationToken ct);
        Task<bool> SoftDeleteAsync(string id, TimeSpan purgeAfter, CancellationToken ct);
        Task<bool> RestoreAsync(string id, CancellationToken ct);

        Task<(IReadOnlyList<Note> Items, long Total)> GetDeletedAsync(int page, int pageSize, CancellationToken ct);
    }