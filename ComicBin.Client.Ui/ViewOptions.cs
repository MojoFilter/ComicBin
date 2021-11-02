using ReactiveUI;
using System.Reactive.Linq;

namespace ComicBin.Client.Ui
{
    internal class ViewOptions : ReactiveObject, IViewOptions
    {
        public bool Read
        {
            get => _read;
            set => this.RaiseAndSetIfChanged(ref _read, value);
        }

        public bool Unread
        {
            get => _unread;
            set => this.RaiseAndSetIfChanged(ref _unread, value);
        }

        public bool Reading
        {
            get => _reading;
            set => this.RaiseAndSetIfChanged(ref _reading, value);
        }

        public IObservable<IViewOptions> Changes => this.Changed.Select(_ => this).AsObservable();

        private bool _read = true;
        private bool _unread = true;
        private bool _reading = true;
    }
}
