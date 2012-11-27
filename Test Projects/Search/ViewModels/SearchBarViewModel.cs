using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Search.Models;

namespace Search.ViewModels
{
    public class SearchBarViewModel : ViewModelBase
    {
        public SearchBarViewModel(SearchResultsViewModel results)
        {
            this.results = results;
            this.searchTerms = string.Empty;
            this.Search = new RelayCommand(this.SearchArchive);
        }

        private readonly SearchResultsViewModel results;
        public RelayCommand Search;

        private string searchTerms;
        public string SearchTerms
        {
            get { return this.searchTerms; }
            set { this.Set("SearchTerms", ref this.searchTerms, value); }
        }

        public string ArchiveLocation
        {
            get
            {
                string directory = Path.Combine(Path.GetTempPath(), "Archive");

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                return directory;
            }
        }

        private void SearchArchive()
        {
            this.results.Update(new SearchResultsModel(this.ArchiveLocation, this.SearchTerms));
        }
    }
}
