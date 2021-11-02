namespace ComicBin
{
    public class Book
    {
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();
        public string Series { get; set; } = null!;
        public string? Number { get; set; } = null!;
        public string? Summary { get; set; } = null!;
        public string? Publisher {  get; set; } = null!;
        public DateTime? AddedUtc { get; set; }
        public bool Read { get; set; }
        public int CurrentPage { get; set; }
    }
}
