using ComicBin.Client.Ui;
using System.Windows;
using System.Windows.Controls;

namespace ComicBin.Wpf
{
    /// <summary>
    /// Interaction logic for LibraryToolbar.xaml
    /// </summary>
    public partial class LibraryToolbar : UserControl
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
