// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using static Orang.FileSystem.FileSystemHelpers;

namespace Orang.FileSystem
{
    public static class FileSystemFinder
    {
        public static IEnumerable<FileSystemFinderResult> Find(
            string directoryPath,
            Filter nameFilter = null,
            Filter extensionFilter = null,
            Filter directoryFilter = null,
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

            NamePartKind namePart = nameFilter?.NamePart ?? NamePartKind.Name;
            NamePartKind directoryNamePart = directoryFilter?.NamePart ?? NamePartKind.Name;

            var directories = new Queue<Directory>();
            Queue<Directory> subdirectories = (options.RecurseSubdirectories) ? new Queue<Directory>() : null;

            string currentDirectory = null;

            if (notifyDirectoryChanged != null)
            {
                notifyDirectoryChanged.DirectoryChanged += (object sender, DirectoryChangedEventArgs e) => currentDirectory = e.NewName;
            }

            bool? isMatch = (directoryFilter?.IsNegative == false)
                ? directoryFilter.IsMatch(NamePart.FromDirectory(directoryPath, directoryNamePart))
                : default(bool?);

            var directory = new Directory(directoryPath, isMatch);

            while (true)
            {
                progress?.Report(new FileSystemFinderProgress(directory.Path, ProgressKind.SearchedDirectory));

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
                        progress?.Report(new FileSystemFinderProgress(directory.Path, ProgressKind.SearchedDirectory, ex));
                    }

                    if (fi != null)
                    {
                        using (fi)
                        {
                            while (fi.MoveNext())
                            {
                                FileSystemFinderResult result = MatchFile(fi.Current, nameFilter, extensionFilter, options, progress, namePart);

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
                    progress?.Report(new FileSystemFinderProgress(directory.Path, ProgressKind.SearchedDirectory, ex));
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
                                if (isMatch == null)
                                    isMatch = directoryFilter?.IsMatch(NamePart.FromDirectory(currentDirectory, directoryNamePart));

                                if (isMatch != false)
                                {
                                    FileSystemFinderResult result = MatchDirectory(currentDirectory, nameFilter, options, progress);

                                    if (result != null)
                                        yield return result;
                                }
                            }

                            if (currentDirectory != null
                                && options.RecurseSubdirectories)
                            {
                                if (isMatch == null)
                                    isMatch = directoryFilter?.IsMatch(NamePart.FromDirectory(currentDirectory, directoryNamePart));

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

        public static FileSystemFinderResult MatchFile(
            string path,
            Filter nameFilter = null,
            Filter extensionFilter = null,
            FileSystemFinderOptions options = null,
            IProgress<FileSystemFinderProgress> progress = null)
        {
            return MatchFile(
                path: path,
                nameFilter: nameFilter,
                extensionFilter: extensionFilter,
                options: options ?? FileSystemFinderOptions.Default,
                progress: progress,
                namePartKind: nameFilter?.NamePart ?? NamePartKind.Name);
        }

        internal static FileSystemFinderResult MatchFile(
            string path,
            Filter nameFilter,
            Filter extensionFilter,
            FileSystemFinderOptions options,
            IProgress<FileSystemFinderProgress> progress,
            NamePartKind namePartKind)
        {
            if (options.Attributes != 0
                && (File.GetAttributes(path) & options.Attributes) != options.Attributes)
            {
                return null;
            }

            progress?.Report(new FileSystemFinderProgress(path, ProgressKind.File));

            if (extensionFilter?.IsMatch(NamePart.FromFile(path, extensionFilter.NamePart)) == false)
                return null;

            NamePart namePart = NamePart.FromFile(path, namePartKind);
            Match match = null;

            if (nameFilter != null)
            {
                match = (options.PartOnly)
                    ? nameFilter.Regex.Match(namePart.ToString())
                    : nameFilter.Regex.Match(path, namePart.Index, namePart.Length);

                if (!nameFilter.IsMatch(match))
                    return null;
            }

            if (options.Empty != null)
            {
                try
                {
                    if ((options.Empty.Value)
                        ? !IsEmptyFile(path)
                        : IsEmptyFile(path))
                    {
                        return null;
                    }
                }
                catch (Exception ex) when (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    progress?.Report(new FileSystemFinderProgress(path, ProgressKind.File, ex));
                    return null;
                }
            }

            return new FileSystemFinderResult(namePart, match);
        }

        public static FileSystemFinderResult MatchDirectory(
            string path,
            Filter nameFilter = null,
            FileSystemFinderOptions options = null,
            IProgress<FileSystemFinderProgress> progress = null)
        {
            return MatchDirectory(
                path: path,
                nameFilter: nameFilter,
                options: options ?? FileSystemFinderOptions.Default,
                progress: progress,
                namePartKind: nameFilter?.NamePart ?? NamePartKind.Name);
        }

        internal static FileSystemFinderResult MatchDirectory(
            string path,
            Filter nameFilter,
            FileSystemFinderOptions options,
            IProgress<FileSystemFinderProgress> progress,
            NamePartKind namePartKind)
        {
            if (options.Attributes != 0
                && (File.GetAttributes(path) & options.Attributes) != options.Attributes)
            {
                return null;
            }

            progress?.Report(new FileSystemFinderProgress(path, ProgressKind.Directory));

            NamePart namePart = NamePart.FromDirectory(path, namePartKind);
            Match match = null;

            if (nameFilter != null)
            {
                match = (options.PartOnly)
                    ? nameFilter.Regex.Match(namePart.ToString())
                    : nameFilter.Regex.Match(path, namePart.Index, namePart.Length);

                if (!nameFilter.IsMatch(match))
                    return null;
            }

            if (options.Empty != null)
            {
                try
                {
                    if ((options.Empty.Value)
                        ? !IsEmptyDirectory(path)
                        : IsEmptyDirectory(path))
                    {
                        return null;
                    }
                }
                catch (Exception ex) when (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    progress?.Report(new FileSystemFinderProgress(path, ProgressKind.Directory, ex));
                    return null;
                }
            }

            return new FileSystemFinderResult(namePart, match, isDirectory: true);
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
