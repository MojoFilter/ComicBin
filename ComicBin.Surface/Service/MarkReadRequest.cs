namespace ComicBin.Service
{
    public class MarkReadRequest
    {
        public bool Read { get; set; }
        public string[] BookIds { get; set; }
    }
}
