using ComicBin.Client.Ui;
using ReactiveUI;
using System.Reactive.Disposables;
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
    }
}
