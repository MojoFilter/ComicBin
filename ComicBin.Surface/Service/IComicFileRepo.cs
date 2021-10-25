namespace ComicBin.Service
{
    public interface IComicFileRepo
    {
        Task<IEnumerable<string>> ListAllAsync(CancellationToken cancellationToken = default);
    }
}
