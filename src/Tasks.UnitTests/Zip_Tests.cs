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
    public class Zip_Tests : IDisposable
    {
        string sourceDir { get; set; }
        string destDir { get; set; }

        const string archiveName = "test.zip";
        const string testFileName = "test.txt";

        public Zip_Tests()
        {
            FileUtilities.ClearCacheDirectoryPath();

            sourceDir = FileUtilities.GetTemporaryDirectory();
            destDir = FileUtilities.GetTemporaryDirectory();

            using (StreamWriter sw = FileUtilities.OpenWrite(Path.Combine(sourceDir, testFileName), true))
                sw.Write("This is a source temp file.");
        }

        public void Dispose()
        {
            FileUtilities.DeleteDirectoryNoThrow(sourceDir, true);
            FileUtilities.DeleteDirectoryNoThrow(destDir, true);
        }

        private Zip BuildZipSpec(Encoding encoding,
            bool includeBaseDirectory = false,
            CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            return new Zip
            {
                SourceDirectory = new TaskItem(sourceDir),
                DestinationDirectory = new TaskItem(destDir),
                ArchiveName = new TaskItem(archiveName),
                Encoding = encoding,
                IncludeBaseDirectory = includeBaseDirectory,
                CompressionLevel = compressionLevel,

                BuildEngine = new MockEngine()
            };
        }

        [Fact]
        public void ZipFilesSuccess()
        {
            string archivePath = Path.Combine(destDir, archiveName);

            var zipper = BuildZipSpec(Encoding.UTF8);

            bool success = zipper.Execute();

            Assert.True(success);

            Assert.True(File.Exists(archivePath));

            using (var a = ZipFile.OpenRead(archivePath))
                Assert.True(a.Entries.Any(z => z.Name == testFileName));
        }

        [Fact]
        public void ZipFileSourceDirDoesNotExist()
        {
            FileUtilities.DeleteDirectoryNoThrow(sourceDir, true);

            var zipper = BuildZipSpec(Encoding.UTF8);

            bool success = zipper.Execute();

            Assert.False(success);

            Assert.False(FileUtilities.DirectoryExistsNoThrow(sourceDir));
        }

        [Fact]
        public void ZipFileDestDirDoesNotExist()
        {
            FileUtilities.DeleteDirectoryNoThrow(destDir, true);

            var zipper = BuildZipSpec(Encoding.UTF8);

            bool success = zipper.Execute();

            Assert.False(success);

            Assert.False(FileUtilities.DirectoryExistsNoThrow(destDir));
        }

        [Fact]
        public void ZipFileSourceDirEmpty()
        {
            var sourceFile = Path.Combine(sourceDir, testFileName);

            FileUtilities.DeleteNoThrow(sourceFile);

            var zipper = BuildZipSpec(Encoding.UTF8);

            bool success = zipper.Execute();

            Assert.False(success);

            Assert.False(FileUtilities.FileExistsNoThrow(sourceFile));
        }
    }
}
