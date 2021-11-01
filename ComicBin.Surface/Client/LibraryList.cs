namespace ComicBin.Client
{
    public class LibraryList : IComicContainer
    {
        public string Name { get; } = "Library";
        public bool Filter(Book book) => true;
    }
}
