#nullable enable

using System;
using System.IO;

namespace UFeel
{
    internal static class ProfileStorage
    {
        private static readonly string[] FileSuffixes = { string.Empty, ".tmp", ".bak" };

        internal static string DefaultPath(string fileName)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException("Cannot determine the per-user application data directory.");

            return Path.Combine(directory, "UFeel", fileName);
        }

        internal static string AbsolutePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A profile path is required.", nameof(path));

            return Path.GetFullPath(path);
        }

        internal static void CreateDirectoryFor(string path)
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
        }

        internal static bool HasFiles(string path)
        {
            foreach (string suffix in FileSuffixes)
            {
                if (File.Exists(path + suffix))
                    return true;
            }

            return false;
        }

        internal static bool RemoveFiles(string path)
        {
            bool removed = true;
            foreach (string suffix in FileSuffixes)
            {
                try
                {
                    File.Delete(path + suffix);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    removed = false;
                }
            }

            return removed;
        }
    }
}
