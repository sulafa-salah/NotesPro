using MongoDB.Bson;
using MongoDB.Driver;
using NotesPro.Api.Domain;
using System.Xml.Linq;

namespace NotesPro.Api.Infrastructure.Persistence.Configurations;
   public sealed class NoteCollectionInitializer(IMongoContext ctx, ILogger<NoteCollectionInitializer> logger)
    : IHostedService
{
    private const string CollName = "notes";

    public async Task StartAsync(CancellationToken ct)
    {
        var db = ctx.Database;

        var names = (await db.ListCollectionNamesAsync(cancellationToken: ct)).ToList(ct);
        if (!names.Contains(CollName))
        {
            logger.LogInformation("Creating '{CollName}' with validator…", CollName);

            var schema = new BsonDocument
            {
                {
                    "$jsonSchema", new BsonDocument
                    {
                        { "bsonType", "object" },
                        { "required", new BsonArray { "title", "content", "slug", "tags" } },
                        
                        { "properties", new BsonDocument
                {
                    { "title", new BsonDocument
                        {
                            { "bsonType", "string" },
                            { "minLength", 1 },
                            { "maxLength", 200 }
                        }
                    },
                    { "content", new BsonDocument
                        {
                            { "bsonType", "string" },
                            { "minLength", 1 }
                        }
                    },
                    { "slug", new BsonDocument
                        {
                            { "bsonType", "string" },
                            { "minLength", 3 },
                            // lowercase letters, digits, hyphens 
                            { "pattern", "^[a-z0-9-]+$" }
                        }
                    },
                    { "tags", new BsonDocument
                        {
                            { "bsonType", "array" },
                            { "items", new BsonDocument { { "bsonType", "string" } } },
                            { "uniqueItems", true },
                            { "maxItems", 50 }
                        }
                    },
                    { "version", new BsonDocument
                        {
                            { "bsonType", "int" },
                            { "minimum", 0 }
                        }
                    },
                    { "createdAtUtc", new BsonDocument { { "bsonType", "date" } } },
                    { "updatedAtUtc", new BsonDocument { { "bsonType", "date" } } },
                    { "deletedAtUtc", new BsonDocument { { "bsonType", new BsonArray { "null", "date" } } } },
                    { "purgeAtUtc",   new BsonDocument { { "bsonType", new BsonArray { "null", "date" } } } }
                }
            }
        }
    }
};

            // Validator must be a FilterDefinition<TDocument>
            var validator = new BsonDocumentFilterDefinition<BsonDocument>(schema);

            var options = new CreateCollectionOptions<BsonDocument>
            {
                Validator = validator,                                 
                ValidationAction = DocumentValidationAction.Error, //Rejects invalid documents 
                ValidationLevel = DocumentValidationLevel.Strict //Tells MongoDB to validate all documents (new and existing) against the schema
            };

          
            await db.CreateCollectionAsync(CollName, options, ct);
            

        }

        // Indexes
        var coll = ctx.Database.GetCollection<Note>(CollName);

        // unique(slug)
        await coll.Indexes.CreateOneAsync(
            new CreateIndexModel<Note>(
                Builders<Note>.IndexKeys.Ascending(n => n.Slug),
                new CreateIndexOptions { Unique = true, Name = "UX_slug" }),
            cancellationToken: ct);

        // tags + createdAt
        await coll.Indexes.CreateOneAsync(
            new CreateIndexModel<Note>(
                Builders<Note>.IndexKeys.Ascending(n => n.Tags).Descending(n => n.CreatedAtUtc),
                new CreateIndexOptions { Name = "IX_tags_createdAt" }),
            cancellationToken: ct);

        // TTL on purgeAt (expire at the time stored in the document)
        await coll.Indexes.CreateOneAsync(
            new CreateIndexModel<Note>(
                Builders<Note>.IndexKeys.Ascending(n => n.PurgeAtUtc),
                new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = "TTL_purgeAt" }),
            cancellationToken: ct);

        // text(title, content)
        await coll.Indexes.CreateOneAsync(
            new CreateIndexModel<Note>(
                Builders<Note>.IndexKeys.Text(n => n.Title).Text(n => n.Content),
                new CreateIndexOptions { Name = "TXT_title_content" }),
            cancellationToken: ct);

        logger.LogInformation("Collection '{CollName}' ready.", CollName);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}