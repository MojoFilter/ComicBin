using ComicBin.Client;

namespace ComicBin.Wpf
{
    internal class ComicBinConfiguration : IComicBinClientConfiguration
    {
        public string ServiceUrl => "http://localhost:5172/";
    }
}
