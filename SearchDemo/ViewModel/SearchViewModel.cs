using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SearchDemo.Model;
using GalaSoft.MvvmLight.Messaging;
using SearchDemo.Message;

namespace SearchDemo.ViewModel
{
    public class SearchViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="SearchText" /> property's name.
        /// </summary>
        public const string SearchTextPropertyName = "SearchText";

        private string _searchText = "example string";
            
        /// <summary>
        /// Sets and gets the SearchText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the MessengerInstance when it changes.
        /// </summary>
        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                Set(() => SearchText, ref _searchText, value, true);
            }
        }

        private RelayCommand<string> _searchCommand;

        /// <summary>
        /// Gets the SearchCommand.
        /// </summary>
        public RelayCommand<string> SearchCommand
        {
            get
            {
                return _searchCommand
                    ?? (_searchCommand = new RelayCommand<string>(this.PerformSearch));
            }
        }

        private void PerformSearch(string searchTerm)
        {
            this.searchModel.PerformSearch(searchTerm);
            var result = this.searchModel.Results;

            Messenger.Default.Send<SearchResultMessage>(new SearchResultMessage(result));
        }

        public SearchViewModel(SearchModel searchModel)
        {
            this.searchModel = searchModel;
        }

        private SearchModel searchModel;
    }
}
