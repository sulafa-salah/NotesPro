using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NotesPro.Api.Contracts.ContractMappings;
using NotesPro.Api.Contracts.Notes;
using NotesPro.Api.Domain;
using NotesPro.Api.Infrastructure.Persistence.Repositories.Notes;
using NotesPro.Api.Services;

namespace NotesPro.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class NotesController(INotesRepository _notesRepository, ISlugService _slugService ) : ControllerBase
{

    // POST /api/notes
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateNoteRequest req, CancellationToken ct)
    {
        try
        {
            // Generate unique slug from title
            var uniqueSlug = await _slugService.GenerateUniqueSlugAsync(req.Title, ct);
            var note = new Note
            {
                Title = req.Title,
                Content = req.Content,
                Tags = req.Tags ?? new(),
                Slug = uniqueSlug,
                Version = 0,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            var id = await _notesRepository.CreateAsync(note, ct);
            var resp = note.MapToNote();
            return CreatedAtAction(nameof(GetById), new { id }, resp);
        }
        catch (MongoBulkWriteException<Note> ex) when (ex.WriteErrors.Any(e => e.Code == 11000))
        {
            return Conflict(new { error = "Slug conflict. Please try again." });
        }
        catch (Exception ex)
        {
           
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    // GET /api/notes/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<NoteResponse>> GetById(string id, CancellationToken ct)
    {
        var note = await _notesRepository.GetAsync(id, ct);
        return note is null ? NotFound() : Ok(note.MapToNote());
    }

    // GET /api/notes (search)
    // /api/notes?q=foo&tags=tag1&tags=tag2&page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<object>> Search(
        [FromQuery] string? q,
        [FromQuery] List<string>? tags,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var (items, total) = await _notesRepository.SearchAsync(q, tags, page, pageSize, ct);
        return Ok(new
        {
            total,
            page,
            pageSize,
            items = items.Select(n => n.MapToNote())
        });
    }

    // PUT /api/notes/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateNoteRequest req,
        CancellationToken ct)
    {
        var ok = await _notesRepository.UpdateAsync(id, n =>
        {
            n.Title = req.Title;
            n.Content = req.Content;
            n.Tags = req.Tags ?? new();
        }, req.ExpectedVersion, ct);

        return ok ? NoContent() : Conflict("Version mismatch or note not found.");
    }

    // DELETE /api/notes/{id}?purgeDays=7
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(
        string id,
        [FromQuery] int purgeDays = 7,
        CancellationToken ct = default)
    {
        var ok = await _notesRepository.SoftDeleteAsync(id, TimeSpan.FromDays(purgeDays), ct);
        return ok ? NoContent() : NotFound();
    }

    // POST /api/notes/{id}/restore
    [HttpPost("{id}/restore")]
    public async Task<IActionResult> Restore(string id, CancellationToken ct)
    {
        var ok = await _notesRepository.RestoreAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    // GET /api/notes/deleted?page=1&pageSize=10
    [HttpGet("deleted")]
    public async Task<ActionResult<object>> GetDeleted(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var (items, total) = await _notesRepository.GetDeletedAsync(page, pageSize, ct);
        return Ok(new
        {
            total,
            page,
            pageSize,
            items = items.Select(n => n.MapToNote())
        });
    }
}

  

  
