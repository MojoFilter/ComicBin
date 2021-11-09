using ComicBin.Client.Ui;
using ReactiveUI;
using System.Windows;
using System.Windows.Controls;

namespace ComicBin.Wpf
{

    public class LibraryToolbarBase : ReactiveUserControl<ILibraryViewModel> { }

    /// <summary>
    /// Interaction logic for LibraryToolbar.xaml
    /// </summary>
    public partial class LibraryToolbar : LibraryToolbarBase
    {
        public LibraryToolbar()
        {
            InitializeComponent();
        }

        private void ClearSearch(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ILibraryViewModel lib)
            {
                lib.SearchQuery = null;
            }
        }
    }
}
