using ReactiveUI;

namespace ComicBin.Maui.ViewModels
{
    internal class AppViewModel : ReactiveObject, IAppViewModel
    {
        public AppViewModel(ISettingsViewModel settings)
        {
            Settings = settings;
        }


        public ISettingsViewModel Settings { get; }
    }

    public interface IAppViewModel
    {
        ISettingsViewModel Settings { get; }
    }
}
