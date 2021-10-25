using Microsoft.EntityFrameworkCore;

namespace ComicBin.Service
{
    internal class ComicBinContextFactory : IComicBinContextFactory
    {
        public IComicBinContext NewComicBinContext() =>
            new ComicBinContext();
    }

    internal class ComicBinContext : DbContext, IComicBinContext
    {
        public ComicBinContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            _dbPath = $"{path}{Path.DirectorySeparatorChar}comicbin.db";
        }

        public DbSet<BookEntry> Books => this.Set<BookEntry>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={_dbPath}");

        public async Task<IEnumerable<string>> PruneAndFindNewAsync(IEnumerable<string> allPaths, CancellationToken cancellationToken = default)
        {
            var currentPaths = this.Books.Select(book => book.Path).ToList();
            var orphanedPaths = currentPaths.Except(allPaths);
            this.RemoveRange(this.Books.Where(b => orphanedPaths.Contains(b.Path)));
            await this.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return allPaths.Except(currentPaths).Select(p => p!);
        }

        public async Task AddBooksAsync(IAsyncEnumerable<BookEntry> newBooks, CancellationToken cancellationToken)
        {
            await foreach (var book in newBooks.WithCancellation(cancellationToken))
            {
                this.Add(book);
            }
            await this.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<BookEntry>> GetAllBooksAsync(CancellationToken cancellationToken)
        {
            return await this.Books.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        private readonly string _dbPath;
    }
}
