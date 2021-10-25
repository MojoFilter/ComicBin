namespace ComicBin.Service
{
    public interface IComicFileReader
    {
        IAsyncEnumerable<BookEntry> ReadAll(IEnumerable<string> newPaths);
    }
}
