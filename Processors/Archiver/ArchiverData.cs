using System;
using System.IO;
using KRSrcWorkflow.Abstracts;

namespace Archiver
{
    [Serializable]
    public class ArchiverData : ProcessorData
    {
        public static string StateXmlArchiveLocation = "ArchiveLocation";

        public string ArchiveLocation
        {
            get
            {
                string directory = this.GetProperty<string>(ArchiverData.StateXmlArchiveLocation);
                //this will eventually have to be pulled from this.State

                if (string.IsNullOrEmpty(directory))
                {
                    directory = Path.Combine(Path.GetTempPath(), "Archive");
                }

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                return directory;
            }
            set
            {
                this.SetProperty(ArchiverData.StateXmlArchiveLocation, value);
            }
        }
    }
}
