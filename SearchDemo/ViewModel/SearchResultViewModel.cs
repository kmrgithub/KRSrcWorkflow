using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using SearchDemo.Model;
using GalaSoft.MvvmLight.Messaging;
using SearchDemo.Message;

namespace SearchDemo.ViewModel
{
    public class SearchResultViewModel : ViewModelBase
    {
        private ObservableCollection<SearchResult> items = new ObservableCollection<SearchResult>();

        public ObservableCollection<SearchResult> SearchItems
        {
            get { return this.items; }
        }

        public SearchResultViewModel()
        {
            Messenger.Default.Register<SearchResultMessage>(this, this.HandleSearchResultMessage);
        }

        private void HandleSearchResultMessage(SearchResultMessage resultMessage)
        {
            var result = resultMessage.Content;
            this.items.Clear();

            foreach (var item in result)
                this.items.Add(item);
        }
    }
}
