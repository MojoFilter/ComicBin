namespace ComicBin
{
    public class ComicList : IComicContainer
    {
        public ComicList(string name, Func<Book, bool> filter)
        {
            _filter = filter;
            this.Name = name;
        }
        public string Name { get; }

        public bool Filter(Book book) => _filter(book);

        private readonly Func<Book, bool> _filter;
    }
}
