namespace NotesPro.Api.Domain;
  public sealed class Note { 
    public string Id { get; set; } = default!; 
    public string Title { get; set; } = default!; 
    public string Content { get; set; } = default!;
    public List<string> Tags { get; set; } = new(); 
    // concurrency token
     public int Version { get; set; }

     public string Slug { get; set; } = default!; 
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow; 
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow; 
    // soft delete + TTL
   public DateTime? DeletedAtUtc { get; set; }
    public DateTime? PurgeAtUtc { get; set; }
}