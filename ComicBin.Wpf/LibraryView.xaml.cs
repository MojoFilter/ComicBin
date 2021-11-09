using ComicBin.Client.Ui;
using ReactiveUI;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.Reactive.Linq;
using ComicBin.Client;

namespace ComicBin.Wpf
{

    public class LibraryViewBase : ReactiveUserControl<ILibraryViewModel> { }

    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : LibraryViewBase
    {
        public LibraryView()
        {
            InitializeComponent();
            this.SetBinding(ViewModelProperty, new Binding(nameof(DataContext)) { Source = this });
            this.WhenActivated((CompositeDisposable disposables) => 
            {
                this.ViewModel.WhenAnyValue(x => x.SelectedGroupType)
                              .ObserveOnDispatcher()
                              .Subscribe(groupType =>
                              {
                                  if (this.layoutRoot.Resources["booksView"] is CollectionViewSource cvs)
                                  {
                                      cvs.GroupDescriptions.Clear();
                                      if (groupType != GroupTypeEnum.None)
                                      {
                                          cvs.GroupDescriptions.Add(new LibraryGroupDescription(this.ViewModel!));
                                      }
                                      cvs.View.Refresh();
                                  }
                              }).DisposeWith(disposables);
            });

            CollectionViewSource cvs;            
        }

        private void bookList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.DataContext is ILibraryViewModel lib && sender is ListBox list)
            {
                lib.SelectedBooks = list.SelectedItems.Cast<Book>();
            }
        }

        private class LibraryGroupDescription : GroupDescription
        {
            public LibraryGroupDescription(ILibraryViewModel library)
            {
                _library = library;
            }


            public override object GroupNameFromItem(object item, int level, CultureInfo culture)
            {
                if (item is Book b)
                {
                    return _library.GroupFromBook(b);
                }
                return "???";
            }

            private readonly ILibraryViewModel _library;
        }
    }
}
