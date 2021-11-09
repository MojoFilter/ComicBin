using Microsoft.AspNetCore.Http;

namespace ComicBin.Service
{
    public interface IComicBinApiService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync(IComicBinRepo repo, CancellationToken ct);
        Task GetCoverAsync(HttpContext context, string bookId, IComicBinRepo repo, CancellationToken ct);
        Task MarkReadAsync(MarkReadRequest req, IComicBinRepo repo, CancellationToken ct);
        Task<string> RefreshComicDatabaseAsync(IComicBinRepo repo, CancellationToken ct);
    }
}
