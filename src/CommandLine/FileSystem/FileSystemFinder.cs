// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;

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

            var directories = new Queue<string>();
            Queue<string> subDirectories = (options.RecurseSubdirectories) ? new Queue<string>() : null;

            string currentDirectory = null;

            if (notifyDirectoryChanged != null)
            {
                notifyDirectoryChanged.DirectoryChanged += (object sender, DirectoryChangedEventArgs e) => currentDirectory = e.NewName;
            }

            string dirPath = directoryPath;

            while (true)
            {
                progress?.Report(new FileSystemFinderProgress(dirPath, ProgressKind.SearchedDirectory));

                if (options.SearchTarget != SearchTarget.Directories)
                {
                    IEnumerator<string> fi = null;

                    try
                    {
                        fi = (options.CanEnumerate)
                            ? Directory.EnumerateFiles(dirPath, "*", enumerationOptions).GetEnumerator()
                            : ((IEnumerable<string>)Directory.GetFiles(dirPath, "*", enumerationOptions)).GetEnumerator();
                    }
                    catch (Exception ex) when (IsWellKnownException(ex))
                    {
                        progress?.Report(new FileSystemFinderProgress(dirPath, ProgressKind.SearchedDirectory, ex));
                    }

                    if (fi != null)
                    {
                        using (fi)
                        {
                            while (fi.MoveNext())
                            {
                                FileSystemFinderResult? result = MatchFile(fi.Current, nameFilter, extensionFilter, options, progress, namePart);

                                if (result != null)
                                    yield return result.Value;

                                cancellationToken.ThrowIfCancellationRequested();
                            }
                        }
                    }
                }

                IEnumerator<string> di = null;

                try
                {
                    di = (options.CanEnumerate)
                        ? Directory.EnumerateDirectories(dirPath, "*", enumerationOptions).GetEnumerator()
                        : ((IEnumerable<string>)Directory.GetDirectories(dirPath, "*", enumerationOptions)).GetEnumerator();
                }
                catch (Exception ex) when (IsWellKnownException(ex))
                {
                    progress?.Report(new FileSystemFinderProgress(dirPath, ProgressKind.SearchedDirectory, ex));
                }

                if (di != null)
                {
                    using (di)
                    {
                        while (di.MoveNext())
                        {
                            currentDirectory = di.Current;

                            if (directoryFilter?.IsMatch(NamePart.FromDirectory(currentDirectory, directoryNamePart)) != false)
                            {
                                if (options.SearchTarget != SearchTarget.Files
                                    && namePart != NamePartKind.Extension)
                                {
                                    FileSystemFinderResult? result = MatchDirectory(currentDirectory, nameFilter, options, progress);

                                    if (result != null)
                                        yield return result.Value;
                                }

                                if (currentDirectory != null
                                    && options.RecurseSubdirectories)
                                {
                                    subDirectories!.Enqueue(currentDirectory);
                                }
                            }

                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }

                    if (options.RecurseSubdirectories)
                    {
                        while (subDirectories!.Count > 0)
                            directories.Enqueue(subDirectories.Dequeue());
                    }
                }

                if (directories.Count > 0)
                {
                    dirPath = directories.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        public static FileSystemFinderResult? MatchFile(
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

        internal static FileSystemFinderResult? MatchFile(
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
                        ? !FileSystemHelpers.IsEmptyFile(path)
                        : FileSystemHelpers.IsEmptyFile(path))
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

        public static FileSystemFinderResult? MatchDirectory(
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

        internal static FileSystemFinderResult? MatchDirectory(
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
                        ? !FileSystemHelpers.IsEmptyDirectory(path)
                        : FileSystemHelpers.IsEmptyDirectory(path))
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
    }
}
