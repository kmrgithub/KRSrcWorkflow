using System.Collections.ObjectModel;
using Search.Models;

namespace Search.ViewModels
{
    public class SearchResultsViewModel
    {
        public ObservableCollection<SearchResult> Results;

        public SearchResultsViewModel(SearchResultsModel model)
        {
            this.Results = new ObservableCollection<SearchResult>();
            this.Update(model);
        }

        public void Update(SearchResultsModel model)
        {
            this.Results.Clear();
            foreach (SearchResult result in model.Results)
            {
                this.Results.Add(result);
            }
        }
    }
}
