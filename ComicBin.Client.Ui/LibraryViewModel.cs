using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace ComicBin.Client.Ui
{
    internal class LibraryViewModel : ReactiveObject, ILibraryViewModel, IActivatableViewModel
    {
        public LibraryViewModel(IComicBinClient client)
        {
            _client = client;
            var refreshCommand = ReactiveCommand.CreateFromTask(ct => this.RefreshBooksAsync(ct));
            this.RefreshCommand = refreshCommand;
            _books = refreshCommand.ToProperty(this, nameof(Books));
            _status = this.WhenAnyValue(x => x.SelectedBook, b => b is Book ? $"{b.Series} #{b.Number}" : String.Empty)
                          .ToProperty(this, nameof(Status));

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                refreshCommand.Execute();
            });
        }

        public ICommand RefreshCommand { get; }

        public IEnumerable<Book> Books => _books.Value;

        private Book? _selectedBook;
        public Book? SelectedBook
        {
            get => _selectedBook;
            set => this.RaiseAndSetIfChanged(ref _selectedBook, value);
        }

        public string Status => _status.Value;


        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        private async Task<IEnumerable<Book>> RefreshBooksAsync(CancellationToken cancellationToken)
        {
            return await _client.GetAllBooksAsync(cancellationToken).ConfigureAwait(false);
        }

        private readonly IComicBinClient _client;
        private readonly ObservableAsPropertyHelper<IEnumerable<Book>> _books;
        private readonly ObservableAsPropertyHelper<string> _status;
    }
}
