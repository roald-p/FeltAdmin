using System;
using System.IO;

namespace FeltAdmin.Diagnostics
{
    public static class PathUtils
    {
        /// <summary>Convert a path into a fully qualified local file path.</summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>The fully qualified path.</returns>
        /// <remarks>
        /// <para>
        /// Converts the path specified to a fully
        /// qualified path. If the path is relative it is
        /// taken as relative from the application base 
        /// directory.
        /// </para>
        /// <para>The path specified must be a local file path, a URI is not supported.</para>
        /// </remarks>
        public static string ConvertToFullPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            string baseDirectory = String.Empty;
            string applicationBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (applicationBaseDirectory != null)
            {
                // applicationBaseDirectory may be a URI not a local file path
                Uri applicationBaseDirectoryUri = new Uri(applicationBaseDirectory);

                if (applicationBaseDirectoryUri.IsFile)
                {
                    baseDirectory = applicationBaseDirectoryUri.LocalPath;
                }
            }

            if (!String.IsNullOrEmpty(baseDirectory))
            {
                // Note that Path.Combine will return the second path if it is rooted
                return Path.GetFullPath(Path.Combine(baseDirectory, path));
            }

            return Path.GetFullPath(path);
        }
    }
}
