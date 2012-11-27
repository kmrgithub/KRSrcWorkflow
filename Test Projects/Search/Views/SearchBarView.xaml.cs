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
using Search.ViewModels;

namespace Search.Views
{
    /// <summary>
    /// Interaction logic for SearchBarView.xaml
    /// </summary>
    public partial class SearchBarView : Page
    {
        public SearchBarView(SearchBarViewModel model)
        {
            this.searchBar = model;
            InitializeComponent();
            this.DataContext = model;
        }

        private readonly SearchBarViewModel searchBar;
    }
}
