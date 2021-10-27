using ComicBin.Client.Ui;
using Microsoft.Extensions.DependencyInjection;

namespace ComicBin
{
    public static class ClientUiConfiguration
    {
        public static IServiceCollection AddComicBinClientUi(this IServiceCollection services) =>
            services.AddTransient<ILibraryViewModel, LibraryViewModel>();
    }
}
