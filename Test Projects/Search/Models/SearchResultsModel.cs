using System.Collections.Generic;
using System.IO;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Search.Models
{
    public class SearchResultsModel
    {
        public List<SearchResult> Results { get; private set; }
        private const string ContentField = "Section";

        public SearchResultsModel(string archiveLocation, string searchQuery)
        {
            this.Results = new List<SearchResult>();

            DirectoryInfo dirInfo = new DirectoryInfo(archiveLocation);
            Directory dir = FSDirectory.Open(dirInfo);

            using (IndexSearcher searcher = new IndexSearcher(dir, true))
            {
                Term term = new Term(SearchResultsModel.ContentField, searchQuery);
                Query query = new TermQuery(term);
                ScoreDoc[] hits = searcher.Search(query, 200).ScoreDocs;
                
                foreach (ScoreDoc hit in hits)
                {
                    string content = searcher.Doc(hit.Doc).Get(SearchResultsModel.ContentField);
                    this.Results.Add(new SearchResult {Confidence = hit.Score, Content = content, Number = hit.Doc});
                }
            }
        }

        public SearchResultsModel()
        {
            this.Results = new List<SearchResult>();
        }
    }
}
