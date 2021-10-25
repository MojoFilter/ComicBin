using ComicBin.Service;

namespace ComicBin.Web
{
    internal class ComicBinWebConfiguration : IComicBinServiceConfiguration
    {
        public ComicBinWebConfiguration(IConfiguration config)
        {
            _configuration = config;
        }


        public string LibraryRootPath => _configuration[nameof(LibraryRootPath)];

        private IConfiguration _configuration;
    }

    internal static class ComicBinWebConfigurationExtensions
    {
        public static IServiceCollection UseComicBinWebConfiguration(this IServiceCollection services) =>
            services.AddTransient<IComicBinServiceConfiguration, ComicBinWebConfiguration>();
    }
}
