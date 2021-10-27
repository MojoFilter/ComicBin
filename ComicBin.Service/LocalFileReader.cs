using SharpCompress.Archives.Rar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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
            using var archive = RarArchive.Open(bookPath);
            var entries = archive.Entries.OrderBy(e => e.Key).ToList();
            using var source = archive.Entries.OrderBy(e=>e.Key)
                                        .Where(e=>!e.IsDirectory && e.Key.Contains(".jpg"))
                                        .Select(e => e.OpenEntryStream())
                                        .First();
            var dest = new MemoryStream();
            using var image = Image.Load(source);
            var resizeOptions = new ResizeOptions()
            {
                Mode = ResizeMode.Max,
                Size = new Size(170, 220)
            };
            image.Mutate(x => x.Resize(resizeOptions));
            await image.SaveAsJpegAsync(dest, cancellationToken).ConfigureAwait(false);
            dest.Position = 0;
            return dest;
        }
    }
}
