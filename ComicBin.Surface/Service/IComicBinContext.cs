namespace ComicBin.Service
{
    public interface IComicBinContext : IDisposable
    {
        Task<IEnumerable<string>> PruneAndFindNewAsync(IEnumerable<string> allPaths, CancellationToken cancellationToken = default);
        Task AddBooksAsync(IAsyncEnumerable<BookEntry> newBooks, CancellationToken cancellationToken);
        Task<IEnumerable<BookEntry>> GetAllBooksAsync(CancellationToken cancellationToken);
        Task<string> GetBookPathAsync(string bookId, CancellationToken cancellationToken);
    }
}
