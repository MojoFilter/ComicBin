namespace ComicBin.Service
{
    internal class DirectRepo : IComicBinRepo
    {
        public DirectRepo(IComicFileRepo fileRepo, IComicBinContextFactory contextFactory, IComicFileReader reader)
        {
            _fileRepo = fileRepo;
            _contextFactory = contextFactory;
            _reader = reader;
        }

        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            var paths = await _fileRepo.ListAllAsync(cancellationToken).ConfigureAwait(false);
            using var context = _contextFactory.NewComicBinContext();
            var newPaths = await context.PruneAndFindNewAsync(paths, cancellationToken).ConfigureAwait(false);
            var newBooks = _reader.ReadAll(newPaths);
            await context.AddBooksAsync(newBooks, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync(CancellationToken cancellationToken = default)
        {
            using var context = _contextFactory.NewComicBinContext();
            return await context.GetAllBooksAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Stream> GetCoverAsync(string bookId, CancellationToken cancellationToken = default)
        {
            using var context = _contextFactory.NewComicBinContext();
            var path = await context.GetBookPathAsync(bookId, cancellationToken).ConfigureAwait(false);
            return await _reader.GetCoverAsync(path, cancellationToken).ConfigureAwait(false);
        }

        private readonly IComicFileRepo _fileRepo;
        private readonly IComicBinContextFactory _contextFactory;
        private readonly IComicFileReader _reader;
    }
}
