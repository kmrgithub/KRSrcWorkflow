using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;
using System.Xml.Linq;

namespace Archiver
{
    public class Archiver : Processor<ArchiverData>
    {
        public override void Process(ArchiverData t)
        {
            WFLogger.NLogger.Debug("Starting to process: " + t.DocumentToProcess);
            string text = File.ReadAllText(t.DocumentToProcess);
            List<Tag> fields = Flatten.Go(new Tag(text));
            SaveToLucene.Save(fields, t.ArchiveLocation);
        }
    }
}
