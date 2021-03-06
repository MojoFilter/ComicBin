using System.Windows.Input;

namespace ComicBin.Client.Ui
{
    public interface ILibraryViewModel
    {
        IEnumerable<Book> Books { get; }
        IEnumerable<string> Series { get; }
        IEnumerable<Book> SelectedBooks { get; set; }
        string Status { get; }
        ICommand RefreshCommand { get; }
        string? SelectedSeries { get; set; }
        IEnumerable<IComicContainer> Folders { get; }
        IComicContainer? SelectedContainer { get; set; }
        IEnumerable<ListOption<SortTypeEnum>> SortTypeOptions { get; }
        SortTypeEnum SelectedSortType { get; set; }
        bool SortDescending { get; set; }
        bool IsMultipleSelected { get; }
        IViewOptions ViewOptions { get; }
        string? SearchQuery { get; set; }
        IEnumerable<ListOption<GroupTypeEnum>> GroupTypeOptions { get; }
        GroupTypeEnum SelectedGroupType { get; set; }
        bool GroupDescending { get; set; }
        ICommand MarkAsUnreadCommand { get; }
        ICommand MarkAsReadCommand { get; }

        string GroupFromBook(Book book);
    }

    public record class ListOption<T>(string Label, T Value);
}
