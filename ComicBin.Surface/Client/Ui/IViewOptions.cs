namespace ComicBin.Client.Ui
{
    public interface IViewOptions
    {
        bool Read { get; set; }
        bool Unread { get; set; }
        bool Reading { get; set; }

        IObservable<IViewOptions> Changes { get; }
    }
}
