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
            Stream? source;
            try
            {
                using var archive = RarArchive.Open(bookPath);
                var entries = archive.Entries.OrderBy(e => e.Key).ToList();
                source = await archive.Entries.OrderBy(e => e.Key)
                                            .Where(e => !e.IsDirectory && e.Key.Contains(".jpg"))
                                            .Select(async e =>
                                            {
                                                var outStream = new MemoryStream();
                                                using var inStream = e.OpenEntryStream();
                                                await inStream.CopyToAsync(outStream);
                                                outStream.Position = 0;
                                                return outStream;
                                            })
                                            .First()
                                            .ConfigureAwait(false);
            }
            catch (Exception)
            {
                var ass = this.GetType().Assembly;
                var resources = ass.GetManifestResourceNames();
                source = this.GetType().Assembly.GetManifestResourceStream("ComicBin.Service.MissingBook.jpg");
            }
            var dest = new MemoryStream();
            using var image = Image.Load(source);
            source?.Dispose();
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
