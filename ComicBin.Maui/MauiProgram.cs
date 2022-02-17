using ComicBin.Maui.ViewModels;

namespace ComicBin.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services
                   .AddTransient<AppShell>()
                   .AddSingleton<IAppViewModel, AppViewModel>()
                   .AddSingleton<ISettingsViewModel, SettingsViewModel>();

            return builder.Build();
        }
    }
}