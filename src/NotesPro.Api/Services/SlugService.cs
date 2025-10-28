using NotesPro.Api.Contracts.Notes;
using NotesPro.Api.Infrastructure.Persistence.Repositories.Notes;
using System.Text.RegularExpressions;

namespace NotesPro.Api.Services;
    public interface ISlugService
    {
        string GenerateSlug(string input);
        Task<string> GenerateUniqueSlugAsync(string input, CancellationToken ct);
    }

    public class SlugService : ISlugService
    {
        private readonly INotesRepository _notesRepository;
        private readonly ILogger<SlugService> _logger;

        public SlugService(INotesRepository notesRepository, ILogger<SlugService> logger)
        {
            _notesRepository = notesRepository;
            _logger = logger;
        }

        public string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Convert to lowercase
            var slug = input.ToLowerInvariant();

            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");

            // Remove invalid characters (keep only letters, numbers, hyphens)
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

            // Replace multiple hyphens with single hyphen
            slug = Regex.Replace(slug, @"-+", "-");

            // Trim hyphens from start and end
            slug = slug.Trim('-');

            // If slug becomes empty after processing, generate a random one
            if (string.IsNullOrEmpty(slug))
            {
                slug = $"note-{Guid.NewGuid():N}";
            }

            // Ensure minimum length
            if (slug.Length < NoteConstraints.SlugMin)
            {
                slug = slug.PadRight(NoteConstraints.SlugMin, '-');
            }

            // Truncate if too long (MongoDB index key limit is 1024 bytes)
            if (slug.Length > NoteConstraints.SlugMax)
            {
                slug = slug.Substring(0, NoteConstraints.SlugMax);
            }

            return slug;
        }

        public async Task<string> GenerateUniqueSlugAsync(string input, CancellationToken ct)
        {
            var baseSlug = GenerateSlug(input);
            var slug = baseSlug;
            var counter = 1;
            const int maxAttempts = 100;

            // Check if slug already exists
            while (counter <= maxAttempts)
            {
                var existingNote = await _notesRepository.GetBySlugAsync(slug, ct);

                if (existingNote == null)
                {
                    return slug; // Slug is available
                }

                // Slug exists, try with counter suffix
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            // If all attempts fail, use GUID as fallback
            var fallbackSlug = $"{baseSlug}-{Guid.NewGuid():N}";
            _logger.LogWarning("Failed to generate unique slug after {MaxAttempts} attempts, using fallback: {FallbackSlug}",
                maxAttempts, fallbackSlug);

            return fallbackSlug;
        }
    }