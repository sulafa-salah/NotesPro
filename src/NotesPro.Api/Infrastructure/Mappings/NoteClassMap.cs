using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using NotesPro.Api.Domain;

namespace NotesPro.Api.Infrastructure.Mappings;
    public static class NoteClassMap
    {
        private static bool _done;

        public static void Register()
        {
            if (_done) return;
            _done = true;

            BsonClassMap.RegisterClassMap<Note>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);

                // Id as string in C#, stored as ObjectId in Mongo
                cm.MapIdMember(n => n.Id)
                  .SetIdGenerator(StringObjectIdGenerator.Instance)
                  .SetSerializer(new StringSerializer(BsonType.ObjectId));

                // Field names 
                cm.MapMember(n => n.Title).SetElementName("title");
                cm.MapMember(n => n.Content).SetElementName("content");
                cm.MapMember(n => n.Tags).SetElementName("tags");
                cm.MapMember(n => n.Version).SetElementName("version");
                cm.MapMember(n => n.Slug).SetElementName("slug");
                cm.MapMember(n => n.CreatedAtUtc).SetElementName("createdAt");
                cm.MapMember(n => n.UpdatedAtUtc).SetElementName("updatedAt");
                cm.MapMember(n => n.DeletedAtUtc).SetElementName("deletedAt");
                cm.MapMember(n => n.PurgeAtUtc).SetElementName("purgeAt");

                
            });
        }
    }