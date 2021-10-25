using SharpCompress.Archives.Rar;
using System.IO.Compression;

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

        public async Task<Stream> GetCoverAsync(string bookPath, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                using var archive = RarArchive.Open(bookPath);
                return archive.Entries.OrderBy(e=>e.Key)
                                      .Select(e => e.OpenEntryStream())
                                      .First();
            });
        }
    }
}
