using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace ComicBin.Client.Ui
{
    internal class LibraryViewModel : ReactiveObject, ILibraryViewModel, IActivatableViewModel
    {
        public LibraryViewModel(
            IComicBinClient client,
            IViewOptions viewOptions,
            IScheduler uiScheduler)
        {
            this.ViewOptions = viewOptions;
            var filter = new FilterSubject();
            var viewFilter = new FilterSubject();
            var searchFilter = new FilterSubject();
            var sort = new BehaviorSubject<IComparer<Book>>(new DefaultBookSort());

            var bookSource = new SourceCache<Book, string>(b => b.Id);
            var folderSource = new SourceList<IComicContainer>();
            folderSource.Add(new LibraryList());
            folderSource.Add(new ComicList("Just X", b => b.Series?.Contains('x', StringComparison.InvariantCultureIgnoreCase) == true));

            void refreshFilters()
            {
                filter!.Refresh();
                viewFilter!.Refresh();
                searchFilter!.Refresh();
            }

            var refreshCommand = ReactiveCommand.CreateFromTask(async ct =>
            {
                var books = await client.GetAllBooksAsync(ct).ConfigureAwait(false);
                bookSource.Edit(u =>
                {
                    u.Clear();
                    u.AddOrUpdate(books);
                });
            });
            this.RefreshCommand = refreshCommand;

            async Task markRead(bool read, CancellationToken ct)
            {
                if (this.SelectedBooks.Any())
                {
                    await client.MarkReadAsync(this.SelectedBooks, read, ct).ConfigureAwait(false);
                    refreshFilters();
                }
            }

            this.MarkAsReadCommand = ReactiveCommand.CreateFromTask(ct => markRead(true, ct));
            this.MarkAsUnreadCommand = ReactiveCommand.CreateFromTask(ct => markRead(false, ct));

            this.SortTypeOptions = UiUtil.EnumOptions<SortTypeEnum>();
            this.GroupTypeOptions = UiUtil.EnumOptions<GroupTypeEnum>();
            
            _status = this.WhenAnyValue(x => x.SelectedBooks, FormatSelection)
                          .ToProperty(this, nameof(Status));

            _isMultipleSelected = this.WhenAnyValue(x=>x.SelectedBooks, bs => bs?.Skip(1).Any() is true)
                                      .ToProperty(this, nameof(IsMultipleSelected));

            this.WhenAnyValue(x => x.SelectedSortType, x => x.SortDescending)
                .Select(_ => this.BuildSort())
                .Subscribe(sort);

            viewOptions.Changes
                       .Select(o => (Book b) => (o.Read || !b.Read)
                                                && (o.Reading || (b.Read || b.CurrentPage == 0) )
                                                && (o.Unread || (b.Read || b.CurrentPage > 0)))                       
                       .Subscribe(viewFilter);

            this.WhenAnyValue(x => x.SearchQuery, this.BuildSearchFilter)
                .Throttle(TimeSpan.FromSeconds(0.5))
                .Subscribe(searchFilter);

            var bookChanges = bookSource.Connect().Publish().RefCount();

            var bookset = bookChanges.Filter(filter)
                                     .Filter(viewFilter)
                                     .Filter(searchFilter)
                                     .Sort(sort);

            bookset.ObserveOn(uiScheduler)
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
                    .Subscribe(filter)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.SelectedContainer)
                    .Where(c => c is IComicContainer)
                    .Select(c => c.Filter)
                    .Subscribe(filter)
                    .DisposeWith(disposables);

                refreshCommand.Execute();
            });
        }

        private string GroupBook(Book book)
        {
            return book.Series;
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

        private Func<Book, bool> BuildSearchFilter(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return _ => true;
            }
            var searchFields = new Func<Book, string?>[]
            {
                b => b.Series,
                b=> b.Summary
            };
            return b => searchFields.Any(f => Regex.IsMatch(f(b) ?? string.Empty, query, RegexOptions.IgnoreCase));
        }

        public ICommand RefreshCommand { get; }

        public ICommand MarkAsReadCommand { get; }
        public ICommand MarkAsUnreadCommand { get; }

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


        public IEnumerable<ListOption<GroupTypeEnum>> GroupTypeOptions { get; }

        private GroupTypeEnum _selectedGroupType = GroupTypeEnum.None;
        public GroupTypeEnum SelectedGroupType
        {
            get => _selectedGroupType;
            set => this.RaiseAndSetIfChanged(ref _selectedGroupType, value);
        }

        private bool _groupDescending;
        public bool GroupDescending
        {
            get => _groupDescending;
            set => this.RaiseAndSetIfChanged(ref _groupDescending, value);
        }


        public bool IsMultipleSelected => _isMultipleSelected.Value;

        public IViewOptions ViewOptions { get; }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        private string? _searchQuery;
        public string? SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }


        public string GroupFromBook(Book book)
        {
            return book.Series;
        }

        private readonly ReadOnlyObservableCollection<Book> _currentView;
        private readonly ReadOnlyObservableCollection<string> _series;
        private readonly ReadOnlyObservableCollection<IComicContainer> _folders;

        //private readonly IComicBinClient _client;
        private readonly ObservableAsPropertyHelper<string> _status;
        private readonly ObservableAsPropertyHelper<bool> _isMultipleSelected;


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

        private class FilterSubject : ISubject<Func<Book, bool>>
        {
            public void OnCompleted() => _subject.OnCompleted();

            public void OnError(Exception error) => _subject.OnError(error);

            public void OnNext(Func<Book, bool> value) => _subject.OnNext(value);

            public IDisposable Subscribe(IObserver<Func<Book, bool>> observer) => _subject.Subscribe(observer);

            public void Refresh() => this.OnNext(_subject.Value);

            private BehaviorSubject<Func<Book, bool>> _subject = new BehaviorSubject<Func<Book, bool>>(_ => true);
        }

        public sealed class ComicGroup : ObservableCollectionExtended<Book>, IComicGroup
        {
            public static ComicGroup Create(IGroup<Book, string, string> group) => new ComicGroup(group);

            public ComicGroup(IGroup<Book, string, string> group)
            {
                this.Key = group.Key;
                _cleanUp = group.Cache.Connect().Bind(this).Subscribe();
            }

            public string Key { get; }

            public void Dispose()
            {
                _cleanUp.Dispose();
            }

            private readonly IDisposable _cleanUp;
        }
    }    

    public record class ComicItem(string Group, Book Book);

}
