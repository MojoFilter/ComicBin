using ComicBin.Client.Ui;
using System.Windows;
using System.Windows.Controls;

namespace ComicBin.Wpf.BrowserViews
{
    /// <summary>
    /// Interaction logic for FolderBrowserView.xaml
    /// </summary>
    public partial class FolderBrowserView : UserControl
    {
        public FolderBrowserView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.DataContext is ILibraryViewModel vm && e.NewValue is IComicContainer c)
            {
                vm.SelectedContainer = c;
            }
        }
    }
}
