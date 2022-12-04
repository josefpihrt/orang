// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Orang.Text;
using static Orang.FileSystem.FileSystemHelpers;

#pragma warning disable RCS1223

namespace Orang.FileSystem;

internal class SearchState
{
    private bool? _shouldAnalyzeFileProperties;
    private bool? _shouldAnalyzeDirectoryProperties;

    public SearchState(FileMatcher matcher)
    {
        Matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
    }

    public FileMatcher Matcher { get; }

    public Func<string, bool>? IncludeDirectory { get; set; }

    public Func<string, bool>? ExcludeDirectory { get; set; }

    public IProgress<SearchProgress>? Progress { get; set; }

    public bool DisallowEnumeration { get; set; }

    public bool MatchPartOnly { get; set; }

    public bool CanRecurseMatch { get; set; } = true;

    public SearchTarget SearchTarget { get; set; } = SearchTarget.All;

    public bool RecurseSubdirectories { get; set; } = true;

    public bool IgnoreInaccessible { get; set; } = true;

    public Encoding? DefaultEncoding { get; set; }

    private FileNamePart Part => Matcher.Part;

    private Matcher? Name => Matcher.Name;

    private Matcher? Extension => Matcher.Extension;

    private Matcher? Content => Matcher.Content;

    private bool ShouldAnalyzeFileProperties
    {
        get
        {
            return _shouldAnalyzeFileProperties ??= Matcher.CreationTimePredicate is not null
                || Matcher.ModifiedTimePredicate is not null
                || Matcher.SizePredicate is not null;
        }
    }

    private bool ShouldAnalyzeDirectoryProperties
    {
        get
        {
            return _shouldAnalyzeDirectoryProperties ??= Matcher.ModifiedTimePredicate is not null
                || Matcher.SizePredicate is not null;
        }
    }

    private FileAttributes Attributes => Matcher.Attributes;

    private FileAttributes AttributesToSkip => Matcher.AttributesToSkip;

    private FileEmptyOption EmptyOption => Matcher.FileEmptyOption;

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
            AttributesToSkip = AttributesToSkip,
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

            if (SearchTarget != SearchTarget.Directories
                && !directory.IsFail)
            {
                IEnumerator<string> fi = null!;

                try
                {
                    fi = (DisallowEnumeration)
                        ? ((IEnumerable<string>)GetFiles(directory.Path, enumerationOptions)).GetEnumerator()
                        : EnumerateFiles(directory.Path, enumerationOptions).GetEnumerator();
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
                di = (DisallowEnumeration)
                    ? ((IEnumerable<string>)GetDirectories(directory.Path, enumerationOptions)).GetEnumerator()
                    : EnumerateDirectories(directory.Path, enumerationOptions).GetEnumerator();
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
                            && SearchTarget != SearchTarget.Files
                            && Part != FileNamePart.Extension)
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
        (FileMatch? fileMatch, Exception? exception) = MatchFileImpl(path);

        Report(path, SearchProgressKind.File, exception: exception);

        return fileMatch;
    }

    private (FileMatch? fileMatch, Exception? exception) MatchFileImpl(string path)
    {
        if (Extension?.IsMatch(FileNameSpan.FromFile(path, FileNamePart.Extension)) == false)
            return default;

        FileNameSpan span = FileNameSpan.FromFile(path, Part);
        Match? match = null;

        if (Name is not null)
        {
            match = Name.Match(span, MatchPartOnly);

            if (match is null)
                return default;
        }

        FileEmptyOption emptyOption = EmptyOption;
        var isEmpty = false;
        FileInfo? fileInfo = null;

        if (Attributes != 0
            || emptyOption != FileEmptyOption.None
            || ShouldAnalyzeFileProperties)
        {
            try
            {
                fileInfo = new FileInfo(path);

                if (Attributes != 0
                    && (fileInfo.Attributes & Attributes) != Attributes)
                {
                    return default;
                }

                if (ShouldAnalyzeFileProperties
                    && !Matcher.IsMatch(fileInfo))
                {
                    return default;
                }

                if (emptyOption != FileEmptyOption.None)
                {
                    if (fileInfo.Length == 0)
                    {
                        if (emptyOption == FileEmptyOption.NonEmpty)
                            return default;

                        isEmpty = true;
                    }
                    else if (fileInfo.Length > 4)
                    {
                        if (emptyOption == FileEmptyOption.Empty)
                            return default;

                        emptyOption = FileEmptyOption.None;
                    }
                }
            }
            catch (Exception ex) when (ex is IOException
                || ex is UnauthorizedAccessException)
            {
                return (null, ex);
            }
        }

        if (Content is not null
            || emptyOption != FileEmptyOption.None)
        {
            FileContent fileContent = default;

            if (isEmpty)
            {
                fileContent = new FileContent("", DefaultEncoding ?? EncodingHelpers.UTF8NoBom, hasBom: false);
            }
            else
            {
                try
                {
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        Encoding? bomEncoding = DetectEncoding(stream);

                        if (emptyOption != FileEmptyOption.None)
                        {
                            if (bomEncoding?.Preamble.Length == stream.Length)
                            {
                                if (emptyOption == FileEmptyOption.NonEmpty)
                                    return default;
                            }
                            else if (emptyOption == FileEmptyOption.Empty)
                            {
                                return default;
                            }
                        }

                        if (Content is not null)
                        {
                            using (var reader = new StreamReader(
                                stream: stream,
                                encoding: bomEncoding ?? DefaultEncoding ?? EncodingHelpers.UTF8NoBom,
                                detectEncodingFromByteOrderMarks: bomEncoding is null))
                            {
                                string content = reader.ReadToEnd();

                                fileContent = new FileContent(
                                    content,
                                    reader.CurrentEncoding,
                                    hasBom: bomEncoding is not null);
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

            if (Content is not null)
            {
                Match? contentMatch = Content.Match(fileContent.Text);

                if (contentMatch is null)
                    return default;

                return (new FileMatch(span, match, fileContent, contentMatch, fileInfo), null);
            }
        }

        return (new FileMatch(span, match, fileInfo), null);
    }

    public FileMatch? MatchDirectory(string path)
    {
        (FileMatch? fileMatch, Exception? exception) = MatchDirectoryImpl(path);

        Report(path, SearchProgressKind.Directory, isDirectory: true, exception);

        return fileMatch;
    }

    private (FileMatch? fileMatch, Exception? exception) MatchDirectoryImpl(string path)
    {
        FileNameSpan span = FileNameSpan.FromDirectory(path, Part);
        Match? match = null;

        if (Name is not null)
        {
            match = Name.Match(span, MatchPartOnly);

            if (match is null)
                return default;
        }

        DirectoryInfo? directoryInfo = null;

        if (Attributes != 0
            || EmptyOption != FileEmptyOption.None
            || ShouldAnalyzeDirectoryProperties)
        {
            try
            {
                directoryInfo = new DirectoryInfo(path);

                if (Attributes != 0
                    && (directoryInfo.Attributes & Attributes) != Attributes)
                {
                    return default;
                }

                if (ShouldAnalyzeDirectoryProperties
                    && !Matcher.IsMatch(directoryInfo))
                {
                    return default;
                }

                if (EmptyOption == FileEmptyOption.Empty)
                {
                    if (!IsEmptyDirectory(path))
                        return default;
                }
                else if (EmptyOption == FileEmptyOption.NonEmpty)
                {
                    if (IsEmptyDirectory(path))
                        return default;
                }
            }
            catch (Exception ex) when (ex is IOException
                || ex is UnauthorizedAccessException)
            {
                return (null, ex);
            }
        }

        return (new FileMatch(span, match, directoryInfo, isDirectory: true), null);
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
        Progress?.Report(path, kind, isDirectory: isDirectory, exception);
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
