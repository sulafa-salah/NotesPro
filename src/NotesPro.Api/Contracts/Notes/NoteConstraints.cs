namespace NotesPro.Api.Contracts.Notes;
    public static class NoteConstraints
    {
        public const int TitleMin = 1;
        public const int TitleMax = 200;

        public const int ContentMin = 1;
        public const int ContentMax = 20000;


        public const int SlugMin = 3;
        public const int SlugMax = 50 ;
        public const int TagsMaxItems = 50;
        public const int TagMaxLen = 20; 
    }