namespace ComicBin.Client
{
    internal class UriBuilder : IUriBuilder
    {
        public UriBuilder(IComicBinClientConfiguration config)
        {
            _baseUri = new Uri(config.ServiceUrl);
        }

        public Uri Build(params string[] parts)
        {
            return new Uri(_baseUri, String.Join("/", parts));
        }

        private readonly Uri _baseUri;
    }
}
