using ComicBin.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ComicBin
{
    public static class ClientConfiguration
    {
        public static IServiceCollection AddComicBinClient(this IServiceCollection services) =>
            services.AddTransient<IComicBinClient, ComicBinClient>()
                    .AddTransient<IUriBuilder, Client.UriBuilder>()
                    .AddSingleton<HttpClient>();
    }
}
