using System.Windows.Controls;
using Search.Models;

namespace Search.Views
{
    public class SearchResultLabel : Label
    {
        public int DocNumber { get; private set; }

        public SearchResultLabel(SearchResult result)
        {
            this.DocNumber = result.Number;
            string content = SearchResultLabel.GetContent(result.Content);
            this.Content = string.Format("Confidence {0}, Document {1}: {2}", result.Confidence, result.Number, content);
        }

        private static string GetContent(string content)
        {
            string trimmed = content.Trim(new char[] {' ', '\t', '\r', '\n'});
            return (trimmed.Length > 100) ? trimmed.Substring(0, 100) : trimmed;
        }
    }
}
