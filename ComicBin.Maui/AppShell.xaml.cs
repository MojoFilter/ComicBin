using ComicBin.Maui.ViewModels;

namespace ComicBin.Maui;

public partial class AppShell : Shell
{
	public AppShell(IAppViewModel appViewModel)
	{
		this.BindingContext = appViewModel;
		InitializeComponent();
	}
}