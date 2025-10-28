using MongoDB.Driver;
using NotesPro.Api.Domain;

namespace NotesPro.Api.Infrastructure.Persistence.Repositories.Notes;
    public sealed class NotesRepository(IMongoContext ctx) : INotesRepository
    {
    private IMongoCollection<Note> Collection => ctx.Notes;

    public async Task<string> CreateAsync(Note note, CancellationToken ct)
    {
        await Collection.InsertOneAsync(note, cancellationToken: ct);
        return note.Id;
    }

    public async Task<Note?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        return await Collection.Find(n => n.Slug == slug).FirstOrDefaultAsync(ct);
    }

    public async Task<Note?> GetAsync(string id, CancellationToken ct)
    => await Collection.Find(n => n.Id ==id && n.DeletedAtUtc == null).FirstOrDefaultAsync(ct);

    public async Task<(IReadOnlyList<Note> Items,long Total)>  SearchAsync(string? q, List<string>? tags, int page, int pageSize, CancellationToken ct)
    {
        var filter =Builders<Note>.Filter.Where(n => n.DeletedAtUtc == null);
       
        if (!string.IsNullOrWhiteSpace(q))
            filter &=Builders<Note>.Filter.Text(q);
        // tag filter
        if (tags is { Count: > 0 })
            filter &=Builders<Note>.Filter.All(n => n.Tags, tags);
        var find = Collection.Find(filter);
        // total count
        var total = await find.CountDocumentsAsync(ct);
        var items = await find.SortByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
        return(items, total);
    }

    public async Task<bool> UpdateAsync(string id, Action<Note> mutate, int expectedVersion, CancellationToken ct)
    {
        var filter = Builders<Note>.Filter.Where(n => n.Id == id && n.Version == expectedVersion && n.DeletedAtUtc == null);
    
        var note = await Collection.Find(filter).FirstOrDefaultAsync(ct);
        if (note == null) return false;
        // apply changes
        mutate(note);
        note.Version += 1;
        note.UpdatedAtUtc = DateTime.UtcNow;
        // write guarded by the SAME filter to avoid races (e.g., soft-delete)
        var result = await Collection.ReplaceOneAsync(filter, note, cancellationToken: ct);
        return result.ModifiedCount == 1;
    }
    public async Task<bool> SoftDeleteAsync(string id, TimeSpan purgeAfter, CancellationToken ct)
    {
        var filter = Builders<Note>.Filter.Where(n => n.Id == id && n.DeletedAtUtc == null);
        var update = Builders<Note>.Update
            .Set(n => n.DeletedAtUtc, DateTime.UtcNow)
            .Set(n => n.PurgeAtUtc, DateTime.UtcNow.Add(purgeAfter))
            .Set(n => n.UpdatedAtUtc, DateTime.UtcNow);
        var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: ct);
        return result.ModifiedCount == 1;
    }
    public async Task<bool> RestoreAsync(string id, CancellationToken ct)
    {
        var filter = Builders<Note>.Filter.Where(n => n.Id == id && n.DeletedAtUtc != null);
        var update = Builders<Note>.Update
            .Set(n => n.DeletedAtUtc, null)
            .Set(n => n.PurgeAtUtc, null)
            .Set(n => n.UpdatedAtUtc, DateTime.UtcNow);
        var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: ct);
        return result.ModifiedCount == 1;
    }

    //  List items in the “Recycle Bin”
    public async Task<(IReadOnlyList<Note> Items, long Total)> GetDeletedAsync(int page, int pageSize, CancellationToken ct)
    {
        var filter = Builders<Note>.Filter.Ne(n => n.DeletedAtUtc, null);
        var total = await Collection.CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await Collection.Find(filter)
            .SortByDescending(n => n.DeletedAtUtc)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

}