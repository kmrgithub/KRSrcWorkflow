using System;
using System.Collections.Specialized;
using System.Windows.Controls;
using Search.Models;
using Search.ViewModels;

namespace Search.Views
{
    /// <summary>
    /// Interaction logic for SearchResultsPage.xaml
    /// </summary>
    public partial class SearchResultsView : Page
    {
        public SearchResultsView(SearchResultsViewModel model)
        {
            this.results = model;
            this.InitializeComponent();
            this.DataContext = this.results;
            this.results.Results.CollectionChanged += SearchResultsChanged;
            this.InitCollection();
        }

        private readonly SearchResultsViewModel results;

        private void InitCollection()
        {
            foreach (SearchResult result in this.results.Results)
            {
                this.Results.Children.Add(new SearchResultLabel(result));
            }
        }

        private void SearchResultsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.AddResult(e);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    this.RemoveResult(e);
                    break;
            }
        }

        private void AddResult(NotifyCollectionChangedEventArgs e)
        {
            foreach (SearchResult result in e.NewItems)
            {
                this.Results.Children.Add(new SearchResultLabel(result));
            }
        }

        private void RemoveResult(NotifyCollectionChangedEventArgs e)
        {
            foreach (SearchResult result in e.OldItems)
            {
                int index = this.FindResultByDocNumber(result.Number);
                this.Results.Children.RemoveAt(index);
            }
        }

        private int FindResultByDocNumber(int number)
        {
            for (int i=0; i<this.Results.Children.Count; i++)
            {
                SearchResultLabel label = (SearchResultLabel) this.Results.Children[i];
                if (label.DocNumber == number)
                {
                    return i;
                }
            }

            throw new Exception(
                string.Format(
                    "Collection out of sync, expected to find document number {0} in it, but was unable to find it.",
                    number));
        }
    }
}
