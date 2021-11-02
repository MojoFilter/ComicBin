using ReactiveUI;
using DynamicData;
using System;
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

            this.SortTypeOptions = UiUtil.EnumOptions<SortTypeEnum>();
            
            _status = this.WhenAnyValue(x => x.SelectedBooks, FormatSelection)
                          .ToProperty(this, nameof(Status));

            _isMultipleSelected = this.WhenAnyValue(x=>x.SelectedBooks, bs => bs?.Skip(1).Any() is true)
                                      .ToProperty(this, nameof(IsMultipleSelected));

            this.WhenAnyValue(x => x.SelectedSortType, x => x.SortDescending)
                .Select(_ => this.BuildSort())
                .Subscribe(_sort);

            var bookChanges = bookSource.Connect().Publish().RefCount();

            bookChanges.Filter(_filter)
                       .Sort(_sort)
                       .ObserveOn(uiScheduler)
                       .Bind(out _currentView)
                       .Subscribe();

            // series source
            bookChanges.DistinctValues(b => b.Series)
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

        private string FormatSelection(IEnumerable<Book> books)
        {
            books ??= Enumerable.Empty<Book>();
            // multiple
            if (books.Skip(1).Any())
            {
                return $"({books.Count()} books selected)";
            }
            else if (books.FirstOrDefault() is Book b)
            {
                return $"{b.Series} #{b.Number}";
            }
            return String.Empty;
        }

        private IComparer<Book> BuildSort()
        {
            var direction = this.SortDescending ? -1 : 1;
            Comparison<Book> sort = this.SelectedSortType switch
            {
                SortTypeEnum.Series => (b1, b2) => b1.Series.CompareTo(b2.Series) * direction,
                SortTypeEnum.Added => (b1, b2) => ((DateTime)b1.AddedUtc!).CompareTo(b2.AddedUtc) * direction,
                _ => (_, _) => 0
            };
            return Comparer<Book>.Create(sort);
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

        private IEnumerable<Book> _selectedBooks = Enumerable.Empty<Book>();
        public IEnumerable<Book> SelectedBooks
        {
            get => _selectedBooks;
            set => this.RaiseAndSetIfChanged(ref _selectedBooks, value);
        }

        private IComicContainer? _selectedContainer;
        public IComicContainer? SelectedContainer
        {
            get => _selectedContainer;
            set => this.RaiseAndSetIfChanged(ref _selectedContainer, value);
        }

        public string Status => _status.Value;

        public IEnumerable<ListOption<SortTypeEnum>> SortTypeOptions { get; }

        private SortTypeEnum _selectedSortType = SortTypeEnum.None;
        public SortTypeEnum SelectedSortType
        {
            get => _selectedSortType;
            set => this.RaiseAndSetIfChanged(ref this._selectedSortType, value);
        }

        private bool _sortDescending;
        public bool SortDescending
        {
            get => _sortDescending;
            set => this.RaiseAndSetIfChanged(ref _sortDescending, value);
        }

        public bool IsMultipleSelected => _isMultipleSelected.Value;

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        private readonly ReadOnlyObservableCollection<Book> _currentView;
        private readonly ReadOnlyObservableCollection<string> _series;
        private readonly ReadOnlyObservableCollection<IComicContainer> _folders;

        private readonly IComicBinClient _client;
        private readonly ObservableAsPropertyHelper<string> _status;
        private readonly ObservableAsPropertyHelper<bool> _isMultipleSelected;
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
