using Microsoft.Build.Framework;
using Microsoft.Build.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace Microsoft.Build.Tasks
{
    /// <summary>
    /// A task that creates a zip archive of files
    /// </summary>
    public class Zip : TaskExtension
    {
        private ITaskItem _sourceDirectory;
        private ITaskItem _destinationDirectory;
        private ITaskItem _archiveName;

        [Required]
        public ITaskItem SourceDirectory
        {
            get
            {
                ErrorUtilities.VerifyThrowArgumentNull(_sourceDirectory, "sourceDirectory");
                return _sourceDirectory;
            }
            set
            {
                _sourceDirectory = value;
            }
        }

        [Required]
        public ITaskItem DestinationDirectory
        {
            get
            {
                ErrorUtilities.VerifyThrowArgumentNull(_destinationDirectory, "destinationArchive");
                return _destinationDirectory;
            }
            set
            {
                _destinationDirectory = value;
            }
        }

        [Required]
        public ITaskItem ArchiveName
        {
            get
            {
                ErrorUtilities.VerifyThrowArgumentNull(_archiveName, "destinationArchive");
                return _archiveName;
            }
            set
            {
                _archiveName = value;
            }
        }

        public CompressionLevel CompressionLevel { get; set; }

        public bool IncludeBaseDirectory { get; set; }
        
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Zip()
        {
        }

        public override bool Execute()
        {
            try
            {
                // Sanitize the directory path, fix relative path etc.
                var dir = FileUtilities.GetDirectory(SourceDirectory.ItemSpec);

                // Verify the directory actually exists on disk
                if (!FileUtilities.DirectoryExistsNoThrow(dir))
                {
                    throw new IOException($"Source Directory not found - {dir}");
                }

                // Sanitize the destination archive directory path
                var dest = FileUtilities.GetDirectory(DestinationDirectory.ItemSpec);

                // Verify the archive directory actually exists
                if (!FileUtilities.DirectoryExistsNoThrow(dest))
                {
                    throw new IOException($"Destination directory not found - {dest}");
                }

                // Fix the archive name extension. ie. append it if i wasn't supplied.
                var fileName = FileUtilities.HasExtension(ArchiveName.ItemSpec, new string[] { "zip" }) ?
                    ArchiveName.ItemSpec :
                    $"{ArchiveName.ItemSpec}.zip";
                
                // Do the zip operation on the folder, including the optional parameters
                ZipFile.CreateFromDirectory(dir, Path.Combine(dest, fileName), this.CompressionLevel, IncludeBaseDirectory);
            }
            catch (Exception e)
            {
                Log.LogErrorWithCodeFromResources("Zip.Error", SourceDirectory.ItemSpec, DestinationDirectory.ItemSpec, e.Message);
            }

            return !Log.HasLoggedErrors;
        }
    }
}
