using System.Windows.Input;

namespace ComicBin.Client.Ui
{
    public interface ILibraryViewModel
    {
        IEnumerable<Book> Books { get; }
        ICommand RefreshCommand { get; }
    }
}
