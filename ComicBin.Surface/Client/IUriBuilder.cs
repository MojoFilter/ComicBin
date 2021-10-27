namespace ComicBin.Client
{
    public interface IUriBuilder
    {
        Uri Build(params string[] parts);
    }
}
