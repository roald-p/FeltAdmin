using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace FeltAdmin.Diagnostics
{
    /// <summary>
    /// Provides a simple appender that directs tracing or debugging output to file.
    /// </summary>
    public class FileAppender : LogAppender
    {
        /// <summary>
        /// Object used for locking.
        /// </summary>
        private static object s_syncObject = new object();

        /// <summary>
        /// The actual filename that is currently being written to
        /// or will be the file transferred to on roll over.
        /// </summary>
        private string m_fileName;

        /// <summary>
        /// The default maximum file size is 10MB
        /// </summary>
        private long m_maxFileSize = 10 * 1024 * 1024;

        /// <summary>
        /// Initializes a new instance of the FileAppender class with the specified file name.
        /// </summary>
        /// <remarks>
        /// The constructor defaults to Trace logging level.
        /// </remarks>
        /// <param name="filename">The fully qualified name, or the relative file name of the log file.</param>
        /// <exception cref="ArgumentNullException">File name is null.</exception>
        /// <exception cref="ArgumentException">File name empty.</exception>
        public FileAppender(string fileName) : this(fileName, LoggingLevels.Trace) { }

        /// <summary>
        /// Initializes a new instance of the FileAppender class with the specified file name
        /// and the specified LoggingLevel.
        /// </summary>
        /// <param name="filename">The fully qualified name, or the relative file name of the log file.</param>
        /// <exception cref="ArgumentNullException">File name is null.</exception>
        /// <exception cref="ArgumentException">File name empty.</exception>
        public FileAppender(string fileName, LoggingLevels level) : this(fileName, level, LoggingLevels.None) { }

        /// <summary>
        /// Initializes a new instance of the FileAppender class with the specified file name
        /// and the specified LoggingLevel.
        /// </summary>
        /// <param name="filename">The fully qualified name, or the relative file name of the log file.</param>
        /// <exception cref="ArgumentNullException">File name is null.</exception>
        /// <exception cref="ArgumentException">File name empty.</exception>
        public FileAppender(string fileName, LoggingLevels level, LoggingLevels minLoggingLevel)
            : base(level, minLoggingLevel)
        {
            this.FileName = fileName;
        }

        /// <summary>
        /// Filename with optional relative or full path
        /// </summary>
        /// <exception cref="ArgumentNullException">File name is null.</exception>
        public string FileName
        {
            get { return this.m_fileName; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }

                this.m_fileName = PathUtils.ConvertToFullPath(value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum size that the output file is allowed to reach
        /// before being rolled over to backup files.
        /// </summary>
        /// <value>
        /// The maximum size in bytes that the output file is allowed to reach before being 
        /// rolled over to backup files.
        /// </value>
        /// <remarks>
        /// <para>
        /// The default maximum file size is 10MB (10*1024*1024).
        /// </para>
        /// </remarks>
        public long MaxFileSize
        {
            get { return this.m_maxFileSize; }
            set { this.m_maxFileSize = value; }
        }

        /// <summary>
        /// Gets the size in bytes of the current log file.
        /// </summary>
        private long FileSize
        {
            get
            {
                FileInfo fi = new FileInfo(this.FileName);
                if (fi.Exists)
                {
                    return fi.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        private FileStream PrepeareAndCreateFileStream()
        {
            string directory = Path.GetDirectoryName(this.FileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (this.m_maxFileSize > 0 && this.FileSize >= this.m_maxFileSize)
            {
                this.RollOverRenameFile();
            }

            return new FileStream(this.FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
        }

        private void RollOverRenameFile()
        {
            string directory = Path.GetDirectoryName(this.FileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.FileName);
            string extension = Path.GetExtension(this.FileName);
            int logNumber = 0;
            string currentDate = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string backupFileName;

            do
            {
                backupFileName = String.Format(CultureInfo.InvariantCulture, "{0}{1} ({2:00}){3}", fileNameWithoutExtension, currentDate, ++logNumber, extension);
            }
            while (File.Exists(Path.Combine(directory, backupFileName)));

            RollFile(this.FileName, Path.Combine(directory, backupFileName));
        }

        /// <summary>
        /// Renames file <paramref name="fromFile"/> to file <paramref name="toFile"/>.
        /// </summary>
        /// <param name="fromFile">Name of existing file to roll.</param>
        /// <param name="toFile">New name for file.</param>
        /// <remarks>
        /// <para>
        /// Renames file <paramref name="fromFile"/> to file <paramref name="toFile"/>. It
        /// also checks for existence of target file and deletes if it does.
        /// </para>
        /// </remarks>
        private static void RollFile(string fromFile, string toFile)
        {
            if (File.Exists(fromFile))
            {
                // Delete the toFile if it exists
                DeleteFile(toFile);

                // We may not have permission to move the file, or the file may be locked
                try
                {
                    File.Move(fromFile, toFile);
                }
                catch //(Exception moveEx)
                {
                }
            }
        }

        /// <summary>
        /// Deletes the specified file if it exists.
        /// </summary>
        /// <param name="fileName">The file to delete.</param>
        /// <remarks>
        /// <para>
        /// Delete a file if is exists.
        /// The file is first moved to a new filename then deleted.
        /// This allows the file to be removed even when it cannot
        /// be deleted, but it still can be moved.
        /// </para>
        /// </remarks>
        private static void DeleteFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                // We may not have permission to delete the file, or the file may be locked

                string fileToDelete = fileName;

                // Try to move the file to temp name.
                // If the file is locked we may still be able to move it
                string tempFileName = fileName + "." + Environment.TickCount + ".DeletePending";
                try
                {
                    File.Move(fileName, tempFileName);
                    fileToDelete = tempFileName;
                }
                catch //(Exception moveEx)
                {
                }

                // Try to delete the file (either the original or the moved file)
                try
                {
                    File.Delete(fileToDelete);
                }
                catch //(Exception deleteEx)
                {
                    //todo:
                    if (fileToDelete == fileName)
                    {
                        // Unable to move or delete the file
                    }
                    else
                    {
                        // Moved the file, but the delete failed. File is probably locked.
                        // The file should automatically be deleted when the lock is released.
                    }
                }
            }
        }

        #region LogAppender Members

        public override void Append(LoggingLevels level, LoggingEvent loggingEvent)
        {
            try
            {
                lock (FileAppender.s_syncObject)
                {
                    using (FileStream stream = this.PrepeareAndCreateFileStream())
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.WriteLine("{0:dd.MM.yyyy HH:mm:ss} [{1,-8}]: {2}", DateTime.Now, level, this.FormatMessage(level, loggingEvent));
                            writer.Flush();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Exception occured when writing to logfile. Details: {0}", e.ToString()));
            }
        }

        #endregion
    }
}
