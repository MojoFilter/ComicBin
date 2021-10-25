namespace ComicBin.Service
{
    public interface IComicBinRepo
    {
        Task RefreshAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<Book>> GetAllBooksAsync(CancellationToken cancellationToken = default);
    }
}
