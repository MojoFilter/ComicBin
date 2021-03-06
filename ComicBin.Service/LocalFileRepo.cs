namespace ComicBin.Service
{
    internal class LocalFileRepo : IComicFileRepo
    {
        public LocalFileRepo(IComicBinServiceConfiguration config)
        {
            _rootPath = config.LibraryRootPath;
        }

        public Task<IEnumerable<string>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Directory.GetFiles(_rootPath, "*.cbr", SearchOption.AllDirectories).AsEnumerable());
        }

        private readonly string _rootPath;
    }
}
