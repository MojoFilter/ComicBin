﻿using ComicBin.Client;
using ComicBin.Client.Ui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace ComicBin.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            _host = Host.CreateDefaultBuilder()
                        .ConfigureServices((context, services) => this.ConfigureServices(context.Configuration, services))
                        .Build();

            this.Resources[typeof(IComicBinClient)] = _host.Services.GetRequiredService<IComicBinClient>();
        }

        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<MainWindow>(p => new MainWindow(p.GetRequiredService<ILibraryViewModel>()))
                    .AddSingleton<IComicBinClientConfiguration, ComicBinConfiguration>()
                    .AddComicBinClient()
                    .AddComicBinClientUi();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            //await _host.StartAsync().ConfigureAwait(true);
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            //base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync().ConfigureAwait(true);
            _host.Dispose();
            base.OnExit(e);
        }

        private readonly IHost _host;
    }
}
