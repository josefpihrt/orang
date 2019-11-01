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
                                string path = fi.Current;

                                if (options.Attributes == 0
                                    || (File.GetAttributes(path) & options.Attributes) == options.Attributes)
                                {
                                    progress?.Report(new FileSystemFinderProgress(path, ProgressKind.File));

                                    if (extensionFilter?.IsMatch(NamePart.FromFile(path, extensionFilter.NamePart)) != false)
                                    {
                                        NamePart part = NamePart.FromFile(path, namePart);

                                        Match match = nameFilter?.Regex.Match(path, part.Index, part.Length);

                                        if (match == null
                                            || nameFilter.IsMatch(match))
                                        {
                                            if (options.Empty == null
                                                || CheckEmptyFile(path, options.Empty.Value))
                                            {
                                                yield return new FileSystemFinderResult(part, match);
                                            }
                                        }

                                        cancellationToken.ThrowIfCancellationRequested();
                                    }
                                }
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
                                if (options.SearchTarget != SearchTarget.Files)
                                {
                                    if (options.Attributes == 0
                                        || (options.Attributes & File.GetAttributes(currentDirectory)) != 0)
                                    {
                                        progress?.Report(new FileSystemFinderProgress(currentDirectory, ProgressKind.Directory));

                                        NamePart part = NamePart.FromDirectory(currentDirectory, namePart);

                                        if (part.Path == null)
                                            continue;

                                        Match match = nameFilter?.Regex.Match(currentDirectory, part.Index, part.Length);

                                        if (match == null
                                            || nameFilter.IsMatch(match))
                                        {
                                            if (options.Empty == null
                                                || CheckEmptyDirectory(currentDirectory, options.Empty.Value))
                                            {
                                                yield return new FileSystemFinderResult(part, match, isDirectory: true);
                                            }
                                        }
                                    }

                                    cancellationToken.ThrowIfCancellationRequested();
                                }

                                if (currentDirectory != null
                                    && options.RecurseSubdirectories)
                                {
                                    subDirectories.Enqueue(currentDirectory);
                                }
                            }

                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }

                    if (options.RecurseSubdirectories)
                    {
                        while (subDirectories.Count > 0)
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

            bool CheckEmptyFile(string path, bool empty)
            {
                try
                {
                    return (empty) ? FileSystemHelpers.IsEmptyFile(path) : !FileSystemHelpers.IsEmptyFile(path);
                }
                catch (Exception ex) when (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    progress?.Report(new FileSystemFinderProgress(path, ProgressKind.File, ex));
                    return false;
                }
            }

            bool CheckEmptyDirectory(string path, bool empty)
            {
                try
                {
                    return (empty) ? FileSystemHelpers.IsEmptyDirectory(path) : !FileSystemHelpers.IsEmptyDirectory(path);
                }
                catch (Exception ex) when (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    progress?.Report(new FileSystemFinderProgress(path, ProgressKind.Directory, ex));
                    return false;
                }
            }
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
