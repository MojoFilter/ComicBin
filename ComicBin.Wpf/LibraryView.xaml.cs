using ComicBin.Client.Ui;
using ReactiveUI;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Data;

namespace ComicBin.Wpf
{

    public class LibraryViewBase : ReactiveUserControl<ILibraryViewModel> { }

    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : LibraryViewBase
    {
        public LibraryView()
        {
            InitializeComponent();
            this.SetBinding(ViewModelProperty, new Binding(nameof(DataContext)) { Source = this });
            this.WhenActivated((CompositeDisposable disposables) => { });
        }

        private void bookList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.DataContext is ILibraryViewModel lib && sender is ListBox list)
            {
                lib.SelectedBooks = list.SelectedItems.Cast<Book>();
            }
        }
    }
}
