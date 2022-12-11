﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

    public Func<string, bool>? IncludeDirectory { get; set; }

    public Func<string, bool>? ExcludeDirectory { get; set; }

    public Action<SearchProgress>? LogProgress { get; set; }

    public bool SupportsEnumeration { get; set; } = true;

    public bool CanRecurseMatch { get; set; } = true;

    public bool RecurseSubdirectories { get; set; } = true;

    public bool IgnoreInaccessible { get; set; } = true;

    public Encoding? DefaultEncoding { get; set; }

    public SearchTelemetry? Telemetry { get; set; }

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
            AttributesToSkip = FileMatcher?.AttributesToSkip ?? 0,
            IgnoreInaccessible = IgnoreInaccessible,
            MatchCasing = MatchCasing.PlatformDefault,
            MatchType = MatchType.Simple,
            RecurseSubdirectories = false,
            ReturnSpecialDirectories = false
        };

        var directories = new Queue<Directory>();
        Queue<Directory>? subdirectories = (RecurseSubdirectories) ? new Queue<Directory>() : null;

        string? currentDirectory = null;

        if (notifyDirectoryChanged is not null)
        {
            notifyDirectoryChanged.DirectoryChanged
                += (object sender, DirectoryChangedEventArgs e) => currentDirectory = e.NewName;
        }

        MatchStatus matchStatus = (IncludeDirectory is not null || ExcludeDirectory is not null)
            ? MatchStatus.Unknown
            : MatchStatus.Success;

        if (IncludeDirectory is not null)
        {
            matchStatus = (IncludeDirectory(directoryPath))
                ? MatchStatus.Success
                : MatchStatus.FailFromPositive;
        }

        var directory = new Directory(directoryPath, matchStatus);

        while (true)
        {
            Report(directory.Path, SearchProgressKind.SearchDirectory, isDirectory: true);

            if (FileMatcher is not null
                && !directory.IsFail)
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
                            FileMatch? match = MatchFile(fi.Current);

                            if (match is not null)
                                yield return match;

                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                }
            }

            IEnumerator<string> di = null!;

            try
            {
                di = (SupportsEnumeration)
                    ? EnumerateDirectories(directory.Path, enumerationOptions).GetEnumerator()
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
                        currentDirectory = di.Current;

                        matchStatus = (ExcludeDirectory is null && directory.IsSuccess)
                            ? MatchStatus.Success
                            : MatchStatus.Unknown;

                        if (!directory.IsFail
                            && DirectoryMatcher is not null
                            && DirectoryMatcher?.NamePart != FileNamePart.Extension)
                        {
                            if (matchStatus == MatchStatus.Unknown
                                && (IncludeDirectory is not null || ExcludeDirectory is not null))
                            {
                                matchStatus = IncludeOrExcludeDirectory(currentDirectory);
                            }

                            if (matchStatus == MatchStatus.Success
                                || matchStatus == MatchStatus.Unknown)
                            {
                                FileMatch? match = MatchDirectory(currentDirectory);

                                if (match is not null)
                                {
                                    yield return match;

                                    if (!CanRecurseMatch)
                                        currentDirectory = null;
                                }
                            }
                        }

                        if (currentDirectory is not null
                            && RecurseSubdirectories)
                        {
                            if (matchStatus == MatchStatus.Unknown
                                && (IncludeDirectory is not null || ExcludeDirectory is not null))
                            {
                                matchStatus = IncludeOrExcludeDirectory(currentDirectory);
                            }

                            if (matchStatus != MatchStatus.FailFromNegative)
                                subdirectories!.Enqueue(new Directory(currentDirectory, matchStatus));
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                if (RecurseSubdirectories)
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

    public FileMatch? MatchFile(string path)
    {
        (FileMatch? fileMatch, Exception? exception) = FileMatcher!.Match(path, DefaultEncoding);

        Report(path, SearchProgressKind.File, exception: exception);

        return fileMatch;
    }

    public FileMatch? MatchDirectory(string path)
    {
        (FileMatch? fileMatch, Exception? exception) = DirectoryMatcher!.Match(path);

        Report(path, SearchProgressKind.Directory, isDirectory: true, exception);

        return fileMatch;
    }

    private MatchStatus IncludeOrExcludeDirectory(string path)
    {
        Debug.Assert(IncludeDirectory is not null || ExcludeDirectory is not null);

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
    private readonly struct Directory
    {
        public Directory(string path, MatchStatus status)
        {
            Path = path;
            Status = status;
        }

        public string Path { get; }

        public MatchStatus Status { get; }

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
