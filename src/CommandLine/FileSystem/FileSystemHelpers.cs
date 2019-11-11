// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Orang.CommandLine;

namespace Orang.FileSystem
{
    internal static class FileSystemHelpers
    {
        private static readonly EnumerationOptions _enumerationOptions = new EnumerationOptions()
        {
            AttributesToSkip = 0,
            IgnoreInaccessible = true,
            RecurseSubdirectories = false,
        };

        public static void Delete(
            in FileSystemFinderResult result,
            bool contentOnly = false,
            bool includingBom = false,
            bool filesOnly = false,
            bool directoriesOnly = false)
        {
            if (result.IsDirectory)
            {
                DeleteDirectory(result.Path, contentOnly: contentOnly, filesOnly: filesOnly, directoriesOnly: directoriesOnly);
            }
            else
            {
                DeleteFile(result.Path, contentOnly: contentOnly, includingBom: includingBom);
            }
        }

        public static void DeleteDirectory(string directoryPath, bool contentOnly, bool filesOnly, bool directoriesOnly)
        {
            if (contentOnly)
            {
                if (!directoriesOnly)
                {
                    foreach (string path in Directory.EnumerateFiles(directoryPath, "*", _enumerationOptions))
                        File.Delete(path);
                }

                if (!filesOnly)
                {
                    foreach (string path in Directory.EnumerateDirectories(directoryPath, "*", _enumerationOptions))
                        Directory.Delete(path, recursive: true);
                }
            }
            else
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }

        public static void DeleteFile(string filePath, bool contentOnly, bool includingBom)
        {
            if (contentOnly)
            {
                FileAccess fileAccess = (includingBom) ? FileAccess.Write : FileAccess.ReadWrite;

                using (var stream = new FileStream(filePath, FileMode.Open, fileAccess))
                {
                    int length = 0;

                    if (includingBom)
                    {
                        Encoding encoding = EncodingHelpers.DetectEncoding(stream);

                        if (encoding != null)
                            length = encoding.Preamble.Length;
                    }

                    stream.SetLength(length);
                }
            }
            else
            {
                File.Delete(filePath);
            }
        }

        internal static bool TryReadAllText(string path, out string content)
        {
            try
            {
                content = File.ReadAllText(path);
                return true;
            }
            catch (Exception ex) when (ex is ArgumentException
                || ex is IOException
                || ex is UnauthorizedAccessException)
            {
                Logger.WriteError(ex);
                content = null;
                return false;
            }
        }

        public static bool IsEmptyFile(string path)
        {
            var fileInfo = new FileInfo(path);

            if (fileInfo.Length == 0)
                return true;

            if (fileInfo.Length <= 4)
            {
                using (FileStream stream = fileInfo.OpenRead())
                {
                    Encoding encoding = EncodingHelpers.DetectEncoding(stream);

                    return encoding?.Preamble.Length == stream.Length;
                }
            }

            return false;
        }

        public static bool IsEmptyDirectory(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        public static int GetFileNameIndex(string path)
        {
            int rootLength = Path.GetPathRoot(path).Length;

            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (i < rootLength
                    || IsDirectorySeparator(path[i]))
                {
                    return i + 1;
                }
            }

            return 0;
        }

        public static int GetExtensionIndex(string path)
        {
            int length = path.Length;

            for (int i = length - 1; i >= 0; i--)
            {
                char ch = path[i];

                if (ch == '.')
                    return i;

                if (IsDirectorySeparator(ch))
                    break;
            }

            return path.Length;
        }

        public static bool IsDirectorySeparator(char ch)
        {
            return ch == Path.DirectorySeparatorChar
                || ch == Path.AltDirectorySeparatorChar;
        }
    }
}
