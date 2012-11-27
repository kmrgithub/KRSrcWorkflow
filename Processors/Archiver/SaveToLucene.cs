using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Archiver
{
    public class SaveToLucene
    {
        public static void Save(List<Tag> fields, string location)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(location);
            Directory dir = FSDirectory.Open(dirInfo);
            Analyzer analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (IndexWriter writer = new IndexWriter(dir, analyzer, true, IndexWriter.MaxFieldLength.LIMITED))
            {
                SaveToLucene.SaveData(fields, writer);
            }
        }

        private static void SaveData(IEnumerable<Tag> fields, IndexWriter writer)
        {
            Document doc = new Document();
            foreach (Tag field in fields)
            {
                doc.Add(
                    new Field(field.Self, field.Content,
                              Field.Store.YES,
                              Field.Index.ANALYZED,
                              Field.TermVector.YES));
            }
            writer.AddDocument(doc);
            writer.Optimize();
        }
    }
}
