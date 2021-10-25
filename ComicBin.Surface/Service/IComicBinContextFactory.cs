namespace ComicBin.Service
{
    public interface IComicBinContextFactory
    {
        IComicBinContext NewComicBinContext();
    }
}
