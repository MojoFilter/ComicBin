namespace ComicBin.Service
{
    internal class LocalFileReader : IComicFileReader
    {
        public async IAsyncEnumerable<BookEntry> ReadAll(IEnumerable<string> newPaths)
        {
            foreach (var path in newPaths)
            {
                yield return new BookEntry()
                {
                    Path = path,
                    Title = Path.GetFileNameWithoutExtension(path)
                };
            }
        }
    }
}
