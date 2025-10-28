using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotesPro.Api.Domain;
using NotesPro.Api.Settings;

namespace NotesPro.Api.Infrastructure;
    public interface IMongoContext
    {
        IMongoDatabase Database { get; }
        IMongoCollection<Note> Notes { get; }
       
    }

    public sealed class MongoContext : IMongoContext
    {
        private readonly MongoClient _client;
        public IMongoDatabase Database { get; }

        public MongoContext(IOptions<MongoSettings> options)
        {
            _client = new MongoClient(options.Value.ConnectionString);
            Database = _client.GetDatabase(options.Value.Database);
        }

        public IMongoCollection<Note> Notes => Database.GetCollection<Note>("notes");
      
    }
