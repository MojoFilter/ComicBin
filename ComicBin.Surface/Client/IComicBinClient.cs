namespace ComicBin.Client
{
    public interface IComicBinClient
    {
        Task<IEnumerable<Book>> GetAllBooksAsync(CancellationToken cancellationToken);
        Task<Stream> GetCoverAsync(string bookId, CancellationToken cancellationToken);
        Task MarkReadAsync(IEnumerable<Book> books, bool read, CancellationToken cancellationToken);
    }
}
