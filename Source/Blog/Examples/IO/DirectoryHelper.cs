namespace Examples.IO
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Examples.Common;

    public static class DirectoryHelper
    {
        public static void Delete(string directory)
        {
            directory.NotNullOrWhiteSpace(nameof(directory));

            Directory.EnumerateFiles(directory).ForEach(FileHelper.Delete);
            Directory.EnumerateDirectories(directory).ForEach(Delete);
            SetAttributes(directory, FileAttributes.Normal);
            Directory.Delete(directory, false);
        }

        public static void Rename(this DirectoryInfo directory, string newName)
        {
            directory.NotNull(nameof(directory));
            newName.NotNullOrWhiteSpace(nameof(newName));

            Directory.Move(directory.FullName, newName);
        }

        public static bool TryRename(this DirectoryInfo directory, string newName)
        {
            directory.NotNull(nameof(directory));
            newName.NotNullOrWhiteSpace(nameof(newName));

            if (directory.Exists && !directory.Name.EqualsOrdinal(newName))
            {
                try
                {
                    Directory.Move(directory.FullName, newName);
                }
                catch (Exception exception) when (exception.IsNotCritical())
                {
                }

                return true;
            }

            return false;
        }

        public static void Move(string source, string destination, bool overwrite = false)
        {
            source.NotNullOrWhiteSpace(nameof(source));
            destination.NotNullOrWhiteSpace(nameof(destination));

            if (overwrite && Directory.Exists(destination))
            {
                Directory.Delete(destination);
            }

            string? parent = Path.GetDirectoryName(destination);
            if (!string.IsNullOrWhiteSpace(parent) && !Directory.Exists(parent))
            {
                Directory.CreateDirectory(parent);
            }

            Directory.Move(source, destination);
        }

        public static void Empty(DirectoryInfo directory)
        {
            directory.NotNull(nameof(directory));

            if (directory.Exists)
            {
                directory.EnumerateFileSystemInfos().ForEach(fileSystemInfo => fileSystemInfo.Delete());
            }
        }

        public static void SetAttributes(string directory, FileAttributes fileAttributes)
        {
            directory.NotNull(nameof(directory));

            new DirectoryInfo(directory).Attributes = fileAttributes;
        }

        public static void AddPrefix(string directory, string prefix)
        {
            Directory.Move(directory, PathHelper.AddDirectoryPrefix(directory, prefix));
        }

        public static void AddPostfix(string directory, string postfix)
        {
            Directory.Move(directory, PathHelper.AddDirectoryPostfix(directory, postfix));
        }

        public static void RenameFileExtensionToLowerCase(string directory)
        {
            Directory
                .GetFiles(directory, "*", SearchOption.AllDirectories)
                .Where(file => Regex.IsMatch(Path.GetExtension(file), @"[A-Z]+"))
                .ToArray()
                .ForEach(file => File.Move(file, PathHelper.ReplaceFileName(file, Path.GetExtension(file).ToLowerInvariant())));
        }

        public static void MoveFiles(string sourceDirectory, string destinationDirectory, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            Directory
                .GetFiles(sourceDirectory, searchPattern, searchOption)
                .ForEach(sourceFile =>
                {
                    string destinationFile = Path.Combine(destinationDirectory, Path.GetRelativePath(sourceDirectory, sourceFile));
                    string newDirectory = Path.GetDirectoryName(destinationFile) ?? throw new ArgumentException(nameof(destinationDirectory));
                    if (!Directory.Exists(newDirectory))
                    {
                        Directory.CreateDirectory(newDirectory);
                    }

                    File.Move(sourceFile, destinationFile);
                });
        }
    }
}