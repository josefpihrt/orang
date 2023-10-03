// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using static Orang.FileSystem.FileSystemUtilities;

#pragma warning disable RCS1223

namespace Orang.FileSystem;

internal class SearchState
{
    public SearchState(FileMatcher? matcher, DirectoryMatcher? directoryMatcher)
    {
        Debug.Assert(matcher is not null || directoryMatcher is not null);

        FileMatcher = matcher;
        DirectoryMatcher = directoryMatcher;
    }

    public FileMatcher? FileMatcher { get; }

    public DirectoryMatcher? DirectoryMatcher { get; }

    public GlobMatcher? GlobFilter { get; set; }

    public Func<string, bool>? IncludeDirectory { get; set; }

    public Func<string, bool>? ExcludeDirectory { get; set; }

    public Action<SearchProgress>? LogProgress { get; set; }

    public bool SupportsEnumeration { get; set; } = true;

    public bool CanRecurseMatch { get; set; } = true;

    public bool RecurseSubdirectories { get; set; } = true;

    public bool IgnoreInaccessible { get; set; } = true;

    public Encoding? DefaultEncoding { get; set; }

    public SearchTelemetry? Telemetry { get; set; }

    public int MinDirectoryDepth { get; set; }

    public int MaxDirectoryDepth { get; set; } = int.MaxValue;

    public IEnumerable<FileMatch> Find(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        return Find(directoryPath, default(INotifyDirectoryChanged), cancellationToken);
    }

    internal IEnumerable<FileMatch> Find(
        string directoryPath,
        INotifyDirectoryChanged? notifyDirectoryChanged = null,
        CancellationToken cancellationToken = default)
    {
        var enumerationOptions = new EnumerationOptions()
        {
            AttributesToSkip = FileMatcher?.WithoutAttributes ?? 0,
            IgnoreInaccessible = IgnoreInaccessible,
            MatchCasing = MatchCasing.PlatformDefault,
            MatchType = MatchType.Simple,
            RecurseSubdirectories = false,
            ReturnSpecialDirectories = false,
        };

        var isRoot = true;
        int matchMinDepth = -1;

        var directories = new Stack<Directory>();
        var subdirectories = new Stack<Directory>();

        string? newDirectoryPath = null;

        if (notifyDirectoryChanged is not null)
        {
            notifyDirectoryChanged.DirectoryChanged
                += (object sender, DirectoryChangedEventArgs e) => newDirectoryPath = e.NewName;
        }

        var directory = new Directory(directoryPath, 0, MatchStatus.Success);

        directories.Push(directory);

        while (directories.Count > 0)
        {
            directory = directories.Pop();
            newDirectoryPath = null;

            Report(directory.Path, SearchProgressKind.SearchDirectory, isDirectory: true);

            if (FileMatcher is not null
                && !directory.IsFail
                && directory.Depth >= MinDirectoryDepth)
            {
                IEnumerator<string> fi = null!;

                try
                {
                    fi = (SupportsEnumeration)
                        ? EnumerateFiles(directory.Path, enumerationOptions).GetEnumerator()
                        : ((IEnumerable<string>)GetFiles(directory.Path, enumerationOptions)).GetEnumerator();
                }
                catch (Exception ex) when (IsWellKnownException(ex))
                {
                    Debug.Fail(ex.ToString());
                    Report(directory.Path, SearchProgressKind.SearchDirectory, isDirectory: true, ex);
                }

                if (fi is not null)
                {
                    using (fi)
                    {
                        while (fi.MoveNext())
                        {
                            FileMatch? match = MatchFile(fi.Current, directoryPath);

                            if (match is not null)
                                yield return match;

                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                }
            }

            FileMatch? directoryMatch = null;

            MatchStatus matchStatus = (ExcludeDirectory is null && directory.IsSuccess)
                ? MatchStatus.Success
                : MatchStatus.Unknown;

            if (!isRoot
                && !directory.IsFail
                && DirectoryMatcher is not null
                && DirectoryMatcher?.NamePart != FileNamePart.Extension
                && directory.Depth >= MinDirectoryDepth)
            {
                if (matchStatus == MatchStatus.Unknown
                    && (IncludeDirectory is not null || ExcludeDirectory is not null))
                {
                    matchStatus = IncludeOrExcludeDirectory(directory.Path);
                }

                if (matchStatus == MatchStatus.Success
                    || matchStatus == MatchStatus.Unknown)
                {
                    directoryMatch = MatchDirectory(directory.Path, directoryPath);

                    if (directoryMatch is not null)
                    {
                        if (matchMinDepth > 0
                            && directory.Depth > matchMinDepth)
                        {
                            directoryMatch.IsSubmatch = true;
                        }

                        yield return directoryMatch;
                    }
                }
            }

            if (isRoot)
                isRoot = false;

            if (matchMinDepth == -1)
            {
                if (directoryMatch is not null)
                    matchMinDepth = directory.Depth;
            }
            else if (directory.Depth <= matchMinDepth)
            {
                matchMinDepth = (directoryMatch is not null)
                    ? directory.Depth
                    : -1;
            }

            if ((directory.Depth == 0 || RecurseSubdirectories)
                && (CanRecurseMatch || directoryMatch is null)
                && directory.Depth < MaxDirectoryDepth)
            {
                if (matchStatus == MatchStatus.Unknown
                    && (IncludeDirectory is not null || ExcludeDirectory is not null))
                {
                    matchStatus = IncludeOrExcludeDirectory(directory.Path);
                }

                if (matchStatus != MatchStatus.FailFromNegative)
                {
                    IEnumerator<string> di = null!;

                    try
                    {
                        di = (SupportsEnumeration)
                            ? EnumerateDirectories(newDirectoryPath ?? directory.Path, enumerationOptions).GetEnumerator()
                            : ((IEnumerable<string>)GetDirectories(directory.Path, enumerationOptions)).GetEnumerator();
                    }
                    catch (Exception ex) when (IsWellKnownException(ex))
                    {
                        Debug.Fail(ex.ToString());
                        Report(directory.Path, SearchProgressKind.SearchDirectory, isDirectory: true, ex);
                    }

                    if (di is not null)
                    {
                        using (di)
                        {
                            while (di.MoveNext())
                            {
                                subdirectories.Push(new Directory(di.Current, directory.Depth + 1, matchStatus));
                            }
                        }

                        while (subdirectories.Count > 0)
                            directories.Push(subdirectories.Pop());
                    }
                }
            }
        }
    }

    public FileMatch? MatchFile(string path)
    {
        (FileMatch? fileMatch, Exception? exception) = FileMatcher!.Match(path, DefaultEncoding);

        if (fileMatch is not null
            && GlobFilter is not null)
        {
            string rootDirPath = Path.GetDirectoryName(path);

            if (!GlobFilter.IsMatch(rootDirPath, path))
                fileMatch = null;
        }

        Report(path, SearchProgressKind.File, exception: exception);

        return fileMatch;
    }

    public FileMatch? MatchFile(string path, string rootDirectoryPath)
    {
        (FileMatch? fileMatch, Exception? exception) = FileMatcher!.Match(path, DefaultEncoding);

        if (fileMatch is not null
            && GlobFilter?.IsMatch(rootDirectoryPath, path) == false)
        {
            fileMatch = null;
        }

        Report(path, SearchProgressKind.File, exception: exception);

        return fileMatch;
    }

    public FileMatch? MatchDirectory(string path, string rootDirectoryPath)
    {
        (FileMatch? fileMatch, Exception? exception) = DirectoryMatcher!.Match(path);

        if (fileMatch is not null
            && GlobFilter?.IsMatch(rootDirectoryPath, path) == false)
        {
            fileMatch = null;
        }

        Report(path, SearchProgressKind.Directory, isDirectory: true, exception);

        return fileMatch;
    }

    private MatchStatus IncludeOrExcludeDirectory(string path)
    {
        if (ExcludeDirectory?.Invoke(path) == true)
            return MatchStatus.FailFromNegative;

        if (IncludeDirectory?.Invoke(path) == false)
            return MatchStatus.FailFromPositive;

        return MatchStatus.Success;
    }

    private void Report(string path, SearchProgressKind kind, bool isDirectory = false, Exception? exception = null)
    {
        LogProgress?.Invoke(new SearchProgress(path, kind, isDirectory: isDirectory, exception));

        if (Telemetry is not null)
        {
            switch (kind)
            {
                case SearchProgressKind.SearchDirectory:
                    {
                        Telemetry.SearchedDirectoryCount++;
                        break;
                    }
                case SearchProgressKind.Directory:
                    {
                        Telemetry.DirectoryCount++;
                        break;
                    }
                case SearchProgressKind.File:
                    {
                        Telemetry.FileCount++;
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException($"Unknown enum value '{kind}'.");
                    }
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

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    private readonly record struct Directory(string Path, int Depth, MatchStatus Status)
    {
        public bool IsSuccess => Status == MatchStatus.Success;

        public bool IsFail => Status == MatchStatus.FailFromPositive || Status == MatchStatus.FailFromNegative;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => Path;
    }

    private enum MatchStatus
    {
        Unknown = 0,
        Success = 1,
        FailFromPositive = 2,
        FailFromNegative = 3,
    }
}
