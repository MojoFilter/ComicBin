namespace ComicBin.Client.Ui;

public static class UiExtensions
{
    public static IObservable<Func<Book, bool>> SelectBookFilter<T>(this IObservable<T> src, Func<T, Book, bool> selector) =>
        src.Select<T, Func<Book, bool>>(d => (Book b) => selector(d, b));
}

public static class UiUtil
{
    public static IEnumerable<ListOption<T>> EnumOptions<T>() where T : System.Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().Select(v => new ListOption<T>(Enum.GetName(typeof(T), v)!, v));
    }


}
