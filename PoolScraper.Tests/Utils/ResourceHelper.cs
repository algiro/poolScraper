using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolScraper.Tests.Utils
{
    public class ResourceHelper
    {
        public static Stream Open7ZipStream(string fileName)
        {
            MemoryStream memoryStream = new MemoryStream();
            
            using (var archive = SevenZipArchive.Open(fileName))
            {
                var singleEntry = archive.Entries.SingleOrDefault();
                if (singleEntry != null)
                {
                    singleEntry.OpenEntryStream().CopyTo(memoryStream);
                }
            }
            memoryStream.Position = 0; // Reset the position of the stream to the beginning
            return memoryStream;
        }
    }
}
