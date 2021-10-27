using ComicBin.Client;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows.Media.Imaging;

namespace ComicBin.Wpf
{
    public class BookTileBase : ReactiveUserControl<Book> { }

    /// <summary>
    /// Interaction logic for BookTile.xaml
    /// </summary>
    public partial class BookTile : BookTileBase
    {
        public BookTile()
        {
            InitializeComponent();
            this.Loaded += (s, e) => loaded++;
            this.WhenActivated(async (CompositeDisposable disposables) =>
            {
                activated++;
                var cancellation = new CancellationDisposable();
                cancellation.DisposeWith(disposables);
                var client = this.FindResource(typeof(IComicBinClient)) as IComicBinClient;
                var coverStream = await client!.GetCoverAsync(this.ViewModel.Id, cancellation.Token).ConfigureAwait(false);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var frame = BitmapFrame.Create(coverStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    this.coverImage.Source = frame;
                });
            });
        }

        private static int loaded = 0;
        private static int activated = 0;
    }
}
