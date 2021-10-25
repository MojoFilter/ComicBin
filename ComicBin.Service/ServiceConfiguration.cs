using Microsoft.Extensions.DependencyInjection;

namespace ComicBin.Service
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection UseComicBinService(this IServiceCollection services) =>
            services.AddTransient<IComicBinRepo, DirectRepo>()
                    .AddTransient<IComicBinContextFactory, ComicBinContextFactory>()
                    .AddTransient<IComicFileRepo, LocalFileRepo>()
                    .AddTransient<IComicFileReader, LocalFileReader>();
    }
}
