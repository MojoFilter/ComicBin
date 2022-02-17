using ReactiveUI;

namespace ComicBin.Maui.ViewModels;

internal class SettingsViewModel : ReactiveObject, ISettingsViewModel
{
    public string ServiceAddress
    {
        get => _serviceAddress;
        set => this.RaiseAndSetIfChanged(ref _serviceAddress, value);
    }
    private string _serviceAddress;
}

public interface ISettingsViewModel
{
    string ServiceAddress { get; set; }
}