using ReactiveUI;
using DynamicData;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;

namespace ComicBin.Client.Ui
{
    internal class LibraryViewModel : ReactiveObject, ILibraryViewModel, IActivatableViewModel
    {
        public LibraryViewModel(IComicBinClient client, IScheduler uiScheduler)
        {
            _client = client;
            _filter = new BehaviorSubject<Func<Book, bool>>(_ => true);
            _sort = new BehaviorSubject<IComparer<Book>>(new DefaultBookSort());            

            var bookSource = new SourceCache<Book, string>(b => b.Id);
            var folderSource = new SourceList<IComicContainer>();
            folderSource.Add(new LibraryList());
            folderSource.Add(new ComicList("Just X", b => b.Series?.Contains('x', StringComparison.InvariantCultureIgnoreCase) == true));

            var refreshCommand = ReactiveCommand.CreateFromTask(async ct =>
            {
                var books = await _client.GetAllBooksAsync(ct).ConfigureAwait(false);
                bookSource.Edit(u =>
                {
                    u.Clear();
                    u.AddOrUpdate(books);
                });
            });
            this.RefreshCommand = refreshCommand;

            
            _status = this.WhenAnyValue(x => x.SelectedBook, b => b is Book ? $"{b.Series} #{b.Number}" : String.Empty)
                          .ToProperty(this, nameof(Status));

            bookSource.Connect()
                      .Filter(_filter)
                      .Sort(_sort)
                      .ObserveOn(uiScheduler)
                      .Bind(out _currentView)
                      .Subscribe();

            bookSource.Connect()
                      .DistinctValues(b => b.Series)
                      .Sort(Comparer<string>.Default)
                      .ObserveOn(uiScheduler)
                      .Bind(out _series)
                      .DisposeMany()
                      .Subscribe();

            folderSource.Connect()
                        .ObserveOn(uiScheduler)
                        .Bind(out _folders)
                        .Subscribe();

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                this.WhenAnyValue(x => x.SelectedSeries)
                    .Select(s => (Book b) => String.IsNullOrWhiteSpace(s) || b.Series.Equals(s))
                    .Subscribe(_filter)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.SelectedContainer)
                    .Where(c => c is IComicContainer)
                    .Select(c => c.Filter)
                    .Subscribe(_filter)
                    .DisposeWith(disposables);

                refreshCommand.Execute();
            });
        }

        public ICommand RefreshCommand { get; }

        public IEnumerable<Book> Books => _currentView;

        public IEnumerable<string> Series => _series;

        public IEnumerable<IComicContainer> Folders => _folders;

        private string? _selectedSeries;
        public string? SelectedSeries
        {
            get => _selectedSeries;
            set => this.RaiseAndSetIfChanged(ref _selectedSeries, value);
        }

        private Book? _selectedBook;
        public Book? SelectedBook
        {
            get => _selectedBook;
            set => this.RaiseAndSetIfChanged(ref _selectedBook, value);
        }

        private IComicContainer? _selectedContainer;
        public IComicContainer? SelectedContainer
        {
            get => _selectedContainer;
            set => this.RaiseAndSetIfChanged(ref _selectedContainer, value);
        }

        public string Status => _status.Value;


        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        private readonly ReadOnlyObservableCollection<Book> _currentView;
        private readonly ReadOnlyObservableCollection<string> _series;
        private readonly ReadOnlyObservableCollection<IComicContainer> _folders;

        private readonly IComicBinClient _client;
        private readonly ObservableAsPropertyHelper<string> _status;
        private readonly ISubject<Func<Book, bool>> _filter;
        private readonly ISubject<IComparer<Book>> _sort;

        private class DefaultBookSort : IComparer<Book>
        {
            public int Compare(Book? x, Book? y)
            {
                if (x?.Series == y?.Series)
                {
                    return x!.Number?.CompareTo(y?.Number) ?? 0;
                }
                return x?.Series.CompareTo(y?.Series) ?? 0;
            }
        }
    }


}
