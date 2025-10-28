using NotesPro.Api.Infrastructure.Persistence.Configurations;
using NotesPro.Api.Infrastructure.Persistence.Repositories.Notes;
using NotesPro.Api.Services;
using NotesPro.Api.Settings;

namespace NotesPro.Api.Infrastructure.Mappings;
    public static class MongoExtensions
    {
        public static void AddMongoInfrastructure(this WebApplicationBuilder builder)
        {
            MongoClassMapRegistration.RegisterAll();
            //Add configuration and Mongo services
            builder.Services.Configure<MongoSettings>(
                builder.Configuration.GetSection("Mongo"));

            builder.Services.AddSingleton<IMongoContext, MongoContext>();
        // repositories
        builder.Services.AddSingleton<INotesRepository, NotesRepository>();
        builder.Services.AddScoped<ISlugService, SlugService>();
        // collection/index initializer
        builder.Services.AddHostedService<NoteCollectionInitializer>();
        }
    }