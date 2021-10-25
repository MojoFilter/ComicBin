namespace ComicBin
{
    public class Book
    {
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = null!;
    }
}
