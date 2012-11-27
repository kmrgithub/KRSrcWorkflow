using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SearchDemo.Model
{


    public class SearchModel
    {
        private const string ContentField = "Section";
        public SearchModel()
        {
            
        }

        public void PerformSearch(string searchTerm)
        {


            this.Results = new List<SearchResult>();

            //DirectoryInfo dirInfo = new DirectoryInfo(archiveLocation);
            //Directory dir = FSDirectory.Open(dirInfo);

            //using (IndexSearcher searcher = new IndexSearcher(dir, true))
            //{
            //    Term term = new Term(SearchResultsModel.ContentField, searchQuery);
            //    Query query = new TermQuery(term);
            //    ScoreDoc[] hits = searcher.Search(query, 200).ScoreDocs;

            //    foreach (ScoreDoc hit in hits)
            //    {
            //        string content = searcher.Doc(hit.Doc).Get(SearchModel.ContentField);
            //        this.Results.Add(new SearchResult { Confidence = hit.Score, Content = content, Number = hit.Doc });
            //    }
            //}
            this.Results.Add(new SearchResult { Confidence =34, Content = "What", Number = 54 });
                   
        }

        public List<SearchResult> Results
        {
            // just for demo purposes
            get;
            private set;
        }
    }

    public class SearchResult
    {
        public int Confidence { get; set; }
        public string Content { get; set; }
        public int Number { get; set; }
    }
}
