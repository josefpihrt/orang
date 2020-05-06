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
    public class FileSystemFilter
    {
        public FileSystemFilter(
            NamePartKind namePart = NamePartKind.Name,
            Filter nameFilter = null,
            Filter extensionFilter = null,
            Filter contentFilter = null,
            NamePartKind directoryNamePart = NamePartKind.Name,
            Filter directoryFilter = null,
            FilePropertyFilter propertyFilter = null,
            FileAttributes attributes = default,
            FileAttributes attributesToSkip = default,
            FileEmptyFilter emptyFilter = FileEmptyFilter.None)
        {
            NamePart = namePart;
            NameFilter = nameFilter;
            ExtensionFilter = extensionFilter;
            ContentFilter = contentFilter;
            DirectoryNamePart = directoryNamePart;
            DirectoryFilter = directoryFilter;
            PropertyFilter = propertyFilter;
            Attributes = attributes;
            AttributesToSkip = attributesToSkip;
            EmptyFilter = emptyFilter;
        }

        public NamePartKind NamePart { get; }

        public Filter NameFilter { get; }

        public Filter ExtensionFilter { get; }

        public Filter ContentFilter { get; }

        public NamePartKind DirectoryNamePart { get; }

        public Filter DirectoryFilter { get; }

        public FilePropertyFilter PropertyFilter { get; }

        public FileAttributes Attributes { get; }

        public FileAttributes AttributesToSkip { get; }

        public FileEmptyFilter EmptyFilter { get; }

        public IEnumerable<FileMatch> Matches(
            string directoryPath,
            FileSystemFilterOptions options = null,
            IProgress<FileSystemFilterProgress> progress = null,
            INotifyDirectoryChanged notifyDirectoryChanged = null,
            CancellationToken cancellationToken = default)
        {
            if (options == null)
                options = FileSystemFilterOptions.Default;

            Filter directoryFilter = DirectoryFilter;
            NamePartKind directoryNamePart = DirectoryNamePart;

            var enumerationOptions = new EnumerationOptions()
            {
                AttributesToSkip = AttributesToSkip,
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
                ? directoryFilter.IsDirectoryMatch(directoryPath, directoryNamePart)
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
                                (FileMatch match, Exception ex) = MatchFile(fi.Current, options);

                                progress?.Report(fi.Current, ProgressKind.File, ex);

                                if (match != null)
                                    yield return match;

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
                                && NamePart != NamePartKind.Extension)
                            {
                                if (isMatch == null
                                    && directoryFilter != null)
                                {
                                    isMatch = directoryFilter.IsDirectoryMatch(currentDirectory, directoryNamePart);
                                }

                                if (isMatch != false)
                                {
                                    (FileMatch match, Exception ex) = MatchDirectory(currentDirectory, options);

                                    progress?.Report(currentDirectory, ProgressKind.Directory, ex);

                                    if (match != null)
                                        yield return match;
                                }
                            }

                            if (currentDirectory != null
                                && options.RecurseSubdirectories)
                            {
                                if (isMatch == null
                                    && directoryFilter != null)
                                {
                                    isMatch = directoryFilter.IsDirectoryMatch(currentDirectory, directoryNamePart);
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

        internal FileMatch MatchFile(
            string path,
            FileSystemFilterOptions options = null,
            IProgress<FileSystemFilterProgress> progress = null)
        {
            (FileMatch match, Exception ex) = MatchFile(
                path: path,
                options: options ?? FileSystemFilterOptions.Default);

            progress?.Report(path, ProgressKind.File, ex);

            return match;
        }

        private (FileMatch match, Exception ex) MatchFile(
            string path,
            FileSystemFilterOptions options)
        {
            if (ExtensionFilter?.IsFileMatch(path, NamePartKind.Extension) == false)
                return (null, null);

            NamePart namePart = FileSystem.NamePart.FromFile(path, NamePart);
            Match match = null;

            if (NameFilter != null)
            {
                match = (options.PartOnly)
                    ? NameFilter.Regex.Match(namePart.ToString())
                    : NameFilter.Regex.Match(path, namePart.Index, namePart.Length);

                if (!NameFilter.IsMatch(match))
                    return (null, null);
            }

            FileInfo fileInfo = null;
            bool isEmpty = false;
            FileEmptyFilter emptyFilter = EmptyFilter;

            if (Attributes != 0
                || emptyFilter != FileEmptyFilter.None
                || PropertyFilter != null)
            {
                try
                {
                    fileInfo = new FileInfo(path);

                    if (Attributes != 0
                        && (fileInfo.Attributes & Attributes) != Attributes)
                    {
                        return (null, null);
                    }

                    if (PropertyFilter != null)
                    {
                        if (PropertyFilter.SizePredicate?.Invoke(fileInfo.Length) == false)
                            return (null, null);

                        if (PropertyFilter.CreationTimePredicate?.Invoke(fileInfo.CreationTime) == false)
                            return (null, null);

                        if (PropertyFilter.ModifiedTimePredicate?.Invoke(fileInfo.LastWriteTime) == false)
                            return (null, null);
                    }

                    if (emptyFilter != FileEmptyFilter.None)
                    {
                        if (fileInfo.Length == 0)
                        {
                            if (emptyFilter == FileEmptyFilter.NonEmpty)
                                return (null, null);

                            isEmpty = true;
                        }
                        else if (fileInfo.Length > 4)
                        {
                            if (emptyFilter == FileEmptyFilter.Empty)
                                return (null, null);

                            emptyFilter = FileEmptyFilter.None;
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    return (null, ex);
                }
            }

            if (ContentFilter != null
                || emptyFilter != FileEmptyFilter.None)
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
                                || emptyFilter != FileEmptyFilter.None)
                            {
                                bomEncoding = EncodingHelpers.DetectEncoding(stream);

                                if (emptyFilter != FileEmptyFilter.None)
                                {
                                    if (bomEncoding?.Preamble.Length == stream.Length)
                                    {
                                        if (emptyFilter == FileEmptyFilter.NonEmpty)
                                            return (null, null);
                                    }
                                    else if (emptyFilter == FileEmptyFilter.Empty)
                                    {
                                        return (null, null);
                                    }
                                }

                                if (bomEncoding != null)
                                    encoding = bomEncoding;
                            }

                            if (ContentFilter != null)
                            {
                                if (options.SaveBomEncoding
                                    || emptyFilter != FileEmptyFilter.None)
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

                if (ContentFilter != null)
                {
                    Match contentMatch = ContentFilter.Match(fileContent.Text);

                    if (contentMatch == null)
                        return (null, null);

                    return (new FileMatch(namePart, match, fileContent, contentMatch, fileInfo), null);
                }
            }

            return (new FileMatch(namePart, match, fileInfo), null);
        }

        private (FileMatch match, Exception ex) MatchDirectory(
            string path,
            FileSystemFilterOptions options = null)
        {
            NamePart directoryNamePart = FileSystem.NamePart.FromDirectory(path, NamePart);
            Match match = null;

            if (NameFilter != null)
            {
                match = (options.PartOnly)
                    ? NameFilter.Regex.Match(directoryNamePart.ToString())
                    : NameFilter.Regex.Match(path, directoryNamePart.Index, directoryNamePart.Length);

                if (!NameFilter.IsMatch(match))
                    return (null, null);
            }

            DirectoryInfo directoryInfo = null;

            if (Attributes != 0
                || EmptyFilter != FileEmptyFilter.None
                || PropertyFilter != null)
            {
                try
                {
                    directoryInfo = new DirectoryInfo(path);

                    if (Attributes != 0
                        && (directoryInfo.Attributes & Attributes) != Attributes)
                    {
                        return (null, null);
                    }

                    if (PropertyFilter != null)
                    {
                        if (PropertyFilter.CreationTimePredicate?.Invoke(directoryInfo.CreationTime) == false)
                            return (null, null);

                        if (PropertyFilter.ModifiedTimePredicate?.Invoke(directoryInfo.LastWriteTime) == false)
                            return (null, null);
                    }

                    if (EmptyFilter == FileEmptyFilter.Empty)
                    {
                        if (!IsEmptyDirectory(path))
                            return (null, null);
                    }
                    else if (EmptyFilter == FileEmptyFilter.NonEmpty)
                    {
                        if (IsEmptyDirectory(path))
                            return (null, null);
                    }
                }
                catch (Exception ex) when (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    return (null, ex);
                }
            }

            return (new FileMatch(directoryNamePart, match, directoryInfo, isDirectory: true), null);
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
            private string DebuggerDisplay => Path;
        }
    }
}
