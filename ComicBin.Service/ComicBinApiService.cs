using Microsoft.AspNetCore.Http;

namespace ComicBin.Service
{
    internal class ComicBinApiService : IComicBinApiService
    {
        public async Task<string> RefreshComicDatabaseAsync(IComicBinRepo repo, CancellationToken ct)
        {
            await repo.RefreshAsync(ct).ConfigureAwait(false);
            return "Ok, cool";
        }

        public Task<IEnumerable<Book>> GetAllBooksAsync(IComicBinRepo repo, CancellationToken ct) => repo.GetAllBooksAsync(ct);

        public async Task GetCoverAsync(HttpContext context, string bookId, IComicBinRepo repo, CancellationToken ct)
        {
            context.Response.ContentType = "image/jpg";
            var image = await repo.GetCoverAsync(bookId).ConfigureAwait(false);
            await image.CopyToAsync(context.Response.Body, ct).ConfigureAwait(false);
        }

        public async Task MarkReadAsync(MarkReadRequest req, IComicBinRepo repo, CancellationToken ct)
        {
            await repo.MarkReadAsync(req.Read, req.BookIds, ct).ConfigureAwait(false);
        }
    }
}
