using ComicBin.Client;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ComicBin.Wpf
{
    public class BookTileBase : ReactiveUserControl<Book> 
    {


        public double CoverScale
        {
            get { return (double)GetValue(CoverScaleProperty); }
            set { SetValue(CoverScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CoverScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CoverScaleProperty =
            DependencyProperty.Register("CoverScale", typeof(double), typeof(BookTileBase), new PropertyMetadata(1.0));


    }

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
                try
                {
                    var client = this.FindResource(typeof(IComicBinClient)) as IComicBinClient;
                    var coverStream = await client!.GetCoverAsync(this.ViewModel!.Id, cancellation.Token).ConfigureAwait(false);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        var frame = BitmapFrame.Create(coverStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        this.coverImage.Source = frame;
                    });
                }
                catch (Exception)
                {
                    // no cover
                }
            });
        }

        private static int loaded = 0;
        private static int activated = 0;
    }
}
