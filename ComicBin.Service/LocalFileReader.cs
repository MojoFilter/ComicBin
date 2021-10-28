using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ComicBin.Service
{
    internal class LocalFileReader : IComicFileReader
    {
        public async IAsyncEnumerable<BookEntry> ReadAll(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                BookEntry? book = null;
                try
                {
                    book = await this.BuildFromMetadataAsync(path).ConfigureAwait(false);
                } catch { }
                if (book is BookEntry)
                {
                    yield return book;
                }
            }
        }

        public async Task<Stream> GetCoverAsync(string bookPath, CancellationToken cancellationToken = default)
        {
            Stream? source;
            try
            {
                using var archive = await this.OpenArchiveAsync(bookPath).ConfigureAwait(false);
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

        private Task<IArchive> OpenArchiveAsync(string path) => Task.Run(() =>
            Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".cbz" => ZipArchive.Open(path),
                _ => RarArchive.Open(path) as IArchive
            });

        private async Task<BookEntry> BuildFromMetadataAsync(string path)
        {
            using var archive = await this.OpenArchiveAsync(path).ConfigureAwait(false);
            var entry = archive.Entries
                               .FirstOrDefault(e => string.Equals(e.Key, "comicinfo.xml", StringComparison.InvariantCultureIgnoreCase));

            var book = new BookEntry() { Path = path };
            var filename = Path.GetFileNameWithoutExtension(path);
            var match = Regex.Match(filename, @"(?<title>.*) (?<number>\d+)");
            book.Series = filename;
            if (match.Success)
            {
                book.Series = match.Groups["title"].Value;
                book.Number = match.Groups["number"].Value;
            }
            if (entry is IArchiveEntry)
            {
                try
                {
                    await Task.Yield();
                    using var stream = entry.OpenEntryStream();
                    var doc = XDocument.Load(stream);
                    string node(XName name, string defaultValue = "") => doc.Root!.Descendants(name).Select(n => n.Value).DefaultIfEmpty(defaultValue).FirstOrDefault()!;
                    book.Series = node("Series", book.Series);
                    book.Number = node("Number", book.Number);
                    book.Summary = node("Summary");
                    book.Publisher = node("Publisher");
                }
                catch { }
            }

            return book;
        }

        //private static async Task<BookEntry> ReadMetadataAsync(string path, IArchiveEntry? entry)
        //{
        //    var book = new BookEntry();
        //    var filename = Path.GetFileNameWithoutExtension(path);
        //    var match = Regex.Match(filename, @"(?<title>.*) (?<number>\d+)");
        //    book.Series = filename;
        //    if (match.Success)
        //    {
        //        book.Series = match.Groups["title"].Value;
        //        book.Number = match.Groups["number"].Value;
        //    }
        //    try
        //    {
        //        await Task.Yield();
        //        using var stream = entry.OpenEntryStream();
        //        var doc = XDocument.Load(stream);
        //        string node(XName name, string defaultValue = "") => doc.Root.Descendants(name).Select(n => n.Value).DefaultIfEmpty(defaultValue).FirstOrDefault();
        //        book.Series = node("Series", book.Series);
        //        book.Number = node("Number", book.Number);
        //        book.Summary = node("Summary");
        //        book.Publisher = node("Publisher");
        //    }
        //    catch { }

        //    return book;
        //}
    }
}
