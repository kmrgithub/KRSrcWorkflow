using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Search.Models;
using Search.ViewModels;
using Search.Views;

namespace Search
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.resultsViewModel = new SearchResultsViewModel(new SearchResultsModel());
            this.Search.Content = new SearchBarView(new SearchBarViewModel(this.resultsViewModel));
            this.Results.Content = new SearchResultsView(this.resultsViewModel);
        }

        private SearchResultsViewModel resultsViewModel;
    }
}
