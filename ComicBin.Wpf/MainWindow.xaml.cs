using ComicBin.Client.Ui;
using System.Windows;

namespace ComicBin.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(ILibraryViewModel library)
        {
            InitializeComponent();
            this.DataContext = library;
        }
    }
}
