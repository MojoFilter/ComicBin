namespace ComicBin.Client.Ui
{
    public static class UiExtensions
    {

    }

    public static class UiUtil
    {
        public static IEnumerable<ListOption<T>> EnumOptions<T>() where T : System.Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Select(v => new ListOption<T>(Enum.GetName(typeof(T), v)!, v));
        }
    }
}
