namespace NotesPro.Api.Infrastructure.Mappings;
    public static class MongoClassMapRegistration
    {
        private static bool _registered;

        public static void RegisterAll()
        {
            if (_registered) return;
            _registered = true;

            NoteClassMap.Register();

        // add more create new entities for ex :  UserClassMap.Register();
    }
}