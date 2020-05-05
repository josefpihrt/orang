// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Orang.CommandLine;
using static Orang.FileSystem.FileSystemHelpers;

namespace Orang.FileSystem
{
    public static class FileSystemFinder
    {
        public static IEnumerable<FileSystemFinderResult> Find(
            string directoryPath,
            Filter nameFilter = null,
            NamePartKind namePart = NamePartKind.Name,
            Filter extensionFilter = null,
            Filter directoryFilter = null,
            NamePartKind directoryNamePart = NamePartKind.Name,
            Filter contentFilter = null,
            FilePropertyFilter propertyFilter = null,
            FileSystemFinderOptions options = null,
            IProgress<FileSystemFinderProgress> progress = null,
            INotifyDirectoryChanged notifyDirectoryChanged = null,
            CancellationToken cancellationToken = default)
        {
            if (options == null)
                options = FileSystemFinderOptions.Default;

            var enumerationOptions = new EnumerationOptions()
            {
                AttributesToSkip = options.AttributesToSkip,
                IgnoreInaccessible = options.IgnoreInaccessible,
                MatchCasing = options.MatchCasing,
                MatchType = options.MatchType,
                RecurseSubdirectories = false,
                ReturnSpecialDirectories = options.ReturnSpecialDirectories
            };

            var directories = new Queue<Directory>();
            Queue<Directory> subdirectories = (options.RecurseSubdirectories) ? new Queue<Directory>() : null;

            string currentDirectory = null;

            if (notifyDirectoryChanged != null)
            {
                notifyDirectoryChanged.DirectoryChanged += (object sender, DirectoryChangedEventArgs e) => currentDirectory = e.NewName;
            }

            bool? isMatch = (directoryFilter?.IsNegative == false)
                ? IsMatch(directoryFilter, NamePart.FromDirectory(directoryPath, directoryNamePart))
                : default(bool?);

            var directory = new Directory(directoryPath, isMatch);

            while (true)
            {
                progress?.Report(directory.Path, ProgressKind.SearchedDirectory);

                if (options.SearchTarget != SearchTarget.Directories
                    && directory.IsMatch != false)
                {
                    IEnumerator<string> fi = null;

                    try
                    {
                        fi = (options.CanEnumerate)
                            ? EnumerateFiles(directory.Path, enumerationOptions).GetEnumerator()
                            : ((IEnumerable<string>)GetFiles(directory.Path, enumerationOptions)).GetEnumerator();
                    }
                    catch (Exception ex) when (IsWellKnownException(ex))
                    {
                        Debug.Fail(ex.ToString());
                        progress?.Report(directory.Path, ProgressKind.SearchedDirectory, ex);
                    }

                    if (fi != null)
                    {
                        using (fi)
                        {
                            while (fi.MoveNext())
                            {
                                (FileSystemFinderResult result, Exception ex) = MatchFile(fi.Current, namePart, nameFilter, extensionFilter, contentFilter, propertyFilter, options);

                                progress?.Report(fi.Current, ProgressKind.File, ex);

                                if (result != null)
                                    yield return result;

                                cancellationToken.ThrowIfCancellationRequested();
                            }
                        }
                    }
                }

                IEnumerator<string> di = null;

                try
                {
                    di = (options.CanEnumerate)
                        ? EnumerateDirectories(directory.Path, enumerationOptions).GetEnumerator()
                        : ((IEnumerable<string>)GetDirectories(directory.Path, enumerationOptions)).GetEnumerator();
                }
                catch (Exception ex) when (IsWellKnownException(ex))
                {
                    Debug.Fail(ex.ToString());
                    progress?.Report(directory.Path, ProgressKind.SearchedDirectory, ex);
                }

                if (di != null)
                {
                    using (di)
                    {
                        while (di.MoveNext())
                        {
                            currentDirectory = di.Current;

                            isMatch = (directoryFilter?.IsNegative == false && directory.IsMatch == true)
                                ? true
                                : default(bool?);

                            if (directory.IsMatch != false
                                && options.SearchTarget != SearchTarget.Files
                                && namePart != NamePartKind.Extension)
                            {
                                if (isMatch == null
                                    && directoryFilter != null)
                                {
                                    isMatch = IsMatch(directoryFilter, NamePart.FromDirectory(currentDirectory, directoryNamePart));
                                }

                                if (isMatch != false)
                                {
                                    (FileSystemFinderResult result, Exception ex) = MatchDirectory(currentDirectory, namePart, nameFilter, propertyFilter, options);

                                    progress?.Report(currentDirectory, ProgressKind.Directory, ex);

                                    if (result != null)
                                        yield return result;
                                }
                            }

                            if (currentDirectory != null
                                && options.RecurseSubdirectories)
                            {
                                if (isMatch == null
                                    && directoryFilter != null)
                                {
                                    isMatch = IsMatch(directoryFilter, NamePart.FromDirectory(currentDirectory, directoryNamePart));
                                }

                                if (isMatch != false
                                    || !directoryFilter!.IsNegative)
                                {
                                    subdirectories!.Enqueue(new Directory(currentDirectory, isMatch));
                                }
                            }

                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }

                    if (options.RecurseSubdirectories)
                    {
                        while (subdirectories!.Count > 0)
                            directories.Enqueue(subdirectories.Dequeue());
                    }
                }

                if (directories.Count > 0)
                {
                    directory = directories.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        internal static FileSystemFinderResult MatchFile(
            string path,
            NamePartKind namePartKind = NamePartKind.Name,
            Filter nameFilter = null,
            Filter extensionFilter = null,
            Filter contentFilter = null,
            FilePropertyFilter propertyFilter = null,
            FileSystemFinderOptions options = null,
            IProgress<FileSystemFinderProgress> progress = null)
        {
            (FileSystemFinderResult result, Exception ex) = MatchFile(
                path: path,
                namePartKind: namePartKind,
                nameFilter: nameFilter,
                extensionFilter: extensionFilter,
                contentFilter: contentFilter,
                propertyFilter: propertyFilter,
                options: options ?? FileSystemFinderOptions.Default);

            progress?.Report(path, ProgressKind.File, ex);

            return result;
        }

        private static (FileSystemFinderResult result, Exception ex) MatchFile(
            string path,
            NamePartKind namePartKind,
            Filter nameFilter,
            Filter extensionFilter,
            Filter contentFilter,
            FilePropertyFilter propertyFilter,
            FileSystemFinderOptions options)
        {
            if (extensionFilter != null
                && !IsMatch(extensionFilter, NamePart.FromFile(path, NamePartKind.Extension)))
            {
                return (null, null);
            }

            NamePart namePart = NamePart.FromFile(path, namePartKind);
            Match match = null;

            if (nameFilter != null)
            {
                match = (options.PartOnly)
                    ? nameFilter.Regex.Match(namePart.ToString())
                    : nameFilter.Regex.Match(path, namePart.Index, namePart.Length);

                if (!nameFilter.IsMatch(match))
                    return (null, null);
            }

            bool isEmpty = false;
            bool? empty = options.Empty;

            if (options.Attributes != 0
                || empty != null
                || propertyFilter != null)
            {
                try
                {
                    var fileInfo = new FileInfo(path);

                    if (options.Attributes != 0
                        && (fileInfo.Attributes & options.Attributes) != options.Attributes)
                    {
                        return (null, null);
                    }

                    if (propertyFilter != null)
                    {
                        if (propertyFilter.SizePredicate?.Invoke(fileInfo.Length) == false)
                            return (null, null);

                        if (propertyFilter.CreationTimePredicate?.Invoke(fileInfo.CreationTime) == false)
                            return (null, null);

                        if (propertyFilter.ModifiedTimePredicate?.Invoke(fileInfo.LastWriteTime) == false)
                            return (null, null);
                    }

                    if (empty != null)
                    {
                        if (fileInfo.Length == 0)
                        {
                            if (!empty.Value)
                                return (null, null);

                            isEmpty = true;
                        }
                        else if (fileInfo.Length > 4)
                        {
                            if (empty.Value)
                                return (null, null);

                            empty = null;
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    return (null, ex);
                }
            }

            if (contentFilter != null
                || empty != null)
            {
                FileContent fileContent = default;

                if (isEmpty)
                {
                    fileContent = new FileContent("", null, options.Encoding);
                }
                else
                {
                    try
                    {
                        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            Encoding encoding = options.Encoding;
                            Encoding bomEncoding = null;

                            if (options.SaveBomEncoding
                                || empty != null)
                            {
                                bomEncoding = EncodingHelpers.DetectEncoding(stream);

                                if (empty != null)
                                {
                                    if (bomEncoding?.Preamble.Length == stream.Length)
                                    {
                                        if (!empty.Value)
                                            return (null, null);
                                    }
                                    else if (empty.Value)
                                    {
                                        return (null, null);
                                    }
                                }

                                if (bomEncoding != null)
                                    encoding = bomEncoding;
                            }

                            if (contentFilter != null)
                            {
                                if (options.SaveBomEncoding
                                    || empty != null)
                                {
                                    stream.Position = 0;
                                }

                                using (var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: bomEncoding == null))
                                {
                                    string content = reader.ReadToEnd();

                                    fileContent = new FileContent(content, bomEncoding, reader.CurrentEncoding);
                                }
                            }
                        }
                    }
                    catch (Exception ex) when (ex is IOException
                        || ex is UnauthorizedAccessException)
                    {
                        return (null, ex);
                    }
                }

                if (contentFilter != null)
                {
                    Match contentMatch = contentFilter.Match(fileContent.Text);

                    if (contentMatch == null)
                        return (null, null);

                    return (new FileSystemFinderResult(namePart, match, fileContent, contentMatch), null);
                }
            }

            return (new FileSystemFinderResult(namePart, match), null);
        }

        private static (FileSystemFinderResult result, Exception ex) MatchDirectory(
            string path,
            NamePartKind namePartKind = NamePartKind.Name,
            Filter nameFilter = null,
            FilePropertyFilter propertyFilter = null,
            FileSystemFinderOptions options = null)
        {
            NamePart directoryNamePart = NamePart.FromDirectory(path, namePartKind);
            Match match = null;

            if (nameFilter != null)
            {
                match = (options.PartOnly)
                    ? nameFilter.Regex.Match(directoryNamePart.ToString())
                    : nameFilter.Regex.Match(path, directoryNamePart.Index, directoryNamePart.Length);

                if (!nameFilter.IsMatch(match))
                    return (null, null);
            }

            if (options.Attributes != 0
                || options.Empty != null
                || propertyFilter != null)
            {
                try
                {
                    var directoryInfo = new DirectoryInfo(path);

                    if (options.Attributes != 0
                        && (directoryInfo.Attributes & options.Attributes) != options.Attributes)
                    {
                        return (null, null);
                    }

                    if (propertyFilter != null)
                    {
                        if (propertyFilter.CreationTimePredicate?.Invoke(directoryInfo.CreationTime) == false)
                            return (null, null);

                        if (propertyFilter.ModifiedTimePredicate?.Invoke(directoryInfo.LastWriteTime) == false)
                            return (null, null);
                    }

                    if (options.Empty != null
                        && ((options.Empty.Value)
                            ? !IsEmptyDirectory(path)
                            : IsEmptyDirectory(path)))
                    {
                        return (null, null);
                    }
                }
                catch (Exception ex) when (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    return (null, ex);
                }
            }

            return (new FileSystemFinderResult(directoryNamePart, match, isDirectory: true), null);
        }

        private static bool IsMatch(Filter filter, in NamePart part)
        {
            return filter.IsMatch(filter.Regex.Match(part.Path, part.Index, part.Length));
        }

        private static bool IsWellKnownException(Exception ex)
        {
            return ex is IOException
                || ex is ArgumentException
                || ex is UnauthorizedAccessException
                || ex is SecurityException
                || ex is NotSupportedException;
        }

        [DebuggerDisplay("{DebuggerDisplay,nq}")]
        private readonly struct Directory
        {
            public Directory(string path, bool? isMatch = null)
            {
                Path = path;
                IsMatch = isMatch;
            }

            public string Path { get; }

            public bool? IsMatch { get; }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private string DebuggerDisplay => $"{Path}";
        }
    }
}
