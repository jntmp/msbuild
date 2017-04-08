using Microsoft.Build.Framework;
using Microsoft.Build.Shared;
using Microsoft.Build.Tasks;
using Microsoft.Build.UnitTests;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Build.UnitTests
{
    public class Zip_Tests
    {
        [Fact]
        public void ZipFiles()
        {
            string sourceDir = FileUtilities.GetTemporaryDirectory();
            string destDir = FileUtilities.GetTemporaryDirectory();

            const string archiveName = "test.zip";
            string archivePath = Path.Combine(destDir, archiveName);
            
            using (StreamWriter sw = FileUtilities.OpenWrite(Path.Combine(sourceDir, "test.txt"), true))    // HIGHCHAR: Test writes in UTF8 without preamble.
                sw.Write("This is a source temp file.");

            var zipper = new Zip
            {
                SourceDirectory = new TaskItem(sourceDir),
                DestinationDirectory = new TaskItem(destDir),
                ArchiveName = new TaskItem(archiveName),
                Encoding = Encoding.UTF8,
                IncludeBaseDirectory = false,
                CompressionLevel = CompressionLevel.Fastest,

                BuildEngine = new MockEngine()
            };

            bool success = zipper.Execute();

            

            // result was true
            Assert.True(success);
            
            // archive exists
            Assert.True(File.Exists(archivePath));

            // archive contains test file
            using (var a = ZipFile.OpenRead(archivePath))
                Assert.True(a.Entries.Any(z => z.Name == archiveName));
        }
    }
}
