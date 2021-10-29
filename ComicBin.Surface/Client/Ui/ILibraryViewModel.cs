using System.Windows.Input;

namespace ComicBin.Client.Ui
{
    public interface ILibraryViewModel
    {
        IEnumerable<Book> Books { get; }
        Book SelectedBook { get; set; }
        string Status { get; }
        ICommand RefreshCommand { get; }
    }
}
