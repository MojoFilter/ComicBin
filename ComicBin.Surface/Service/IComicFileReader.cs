namespace ComicBin.Service
{
    public interface IComicFileReader
    {
        Task<Stream> GetCoverAsync(string bookPath, CancellationToken cancellationToken = default);
        IAsyncEnumerable<BookEntry> ReadAll(IEnumerable<string> newPaths);
    }
}
