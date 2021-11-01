namespace ComicBin
{
    public interface IComicContainer
    {
        string Name { get; }
        bool Filter(Book book);
    }
}
