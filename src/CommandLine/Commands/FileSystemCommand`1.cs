// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Orang.FileSystem;

namespace Orang.CommandLine;

internal abstract class FileSystemCommand<TOptions> : AbstractCommand<TOptions> where TOptions : FileSystemCommandOptions
{
    protected FileSystemCommand(TOptions options, Logger logger) : base(options, logger)
    {
    }

    private SearchState? _search;
    private ProgressReporter? _progressReporter;

    protected SearchState Search => _search ??= CreateSearch();

    private ProgressReporter? ProgressReporter => _progressReporter ??= CreateProgressReporter();

    public Matcher? NameFilter => Options.NameFilter;

    protected virtual bool CanDisplaySummary => true;

    public virtual bool CanEndProgress => !Options.OmitPath;

    public virtual bool CanUseResults => true;

    protected virtual void OnSearchCreating(SearchState search)
    {
    }

    private SearchState CreateSearch()
    {
        FilePropertyOptions properties = Options.FilePropertyOptions;
        Func<FileInfo, bool>? filePredicate = null;
        Func<DirectoryInfo, bool>? directoryPredicate = null;

        if (Options.SearchTarget == SearchTarget.Directories)
        {
            if (properties.CreationTimePredicate is not null)
            {
                if (properties.ModifiedTimePredicate is not null)
                {
                    directoryPredicate = di => properties.CreationTimePredicate(di.CreationTime)
                        && properties.ModifiedTimePredicate(di.LastWriteTime);
                }
                else
                {
                    directoryPredicate = di => properties.CreationTimePredicate(di.CreationTime);
                }
            }
            else if (properties.ModifiedTimePredicate is not null)
            {
                directoryPredicate = di => properties.ModifiedTimePredicate(di.LastWriteTime);
            }
        }
        else if (properties.CreationTimePredicate is not null)
        {
            if (properties.ModifiedTimePredicate is not null)
            {
                if (properties.SizePredicate is not null)
                {
                    filePredicate = fi => properties.CreationTimePredicate(fi.CreationTime)
                        && properties.ModifiedTimePredicate(fi.LastWriteTime)
                        && properties.SizePredicate(fi.Length);
                }
                else
                {
                    filePredicate = fi => properties.CreationTimePredicate(fi.CreationTime)
                        && properties.ModifiedTimePredicate(fi.LastWriteTime);
                }
            }
            else if (properties.SizePredicate is not null)
            {
                filePredicate = fi => properties.CreationTimePredicate(fi.CreationTime)
                    && properties.SizePredicate(fi.Length);
            }
            else
            {
                filePredicate = fi => properties.CreationTimePredicate(fi.CreationTime);
            }
        }
        else if (properties.ModifiedTimePredicate is not null)
        {
            if (properties.SizePredicate is not null)
            {
                filePredicate = fi => properties.ModifiedTimePredicate(fi.LastWriteTime)
                    && properties.SizePredicate(fi.Length);
            }
            else
            {
                filePredicate = fi => properties.ModifiedTimePredicate(fi.LastWriteTime);
            }
        }
        else if (properties.SizePredicate is not null)
        {
            filePredicate = fi => properties.SizePredicate(fi.Length);
        }

        FileMatcher? fileMatcher = null;
        DirectoryMatcher? directoryMatcher = null;

        if (Options.SearchTarget != SearchTarget.Directories)
        {
            fileMatcher = new FileMatcher()
            {
                Name = Options.NameFilter,
                NamePart = Options.NamePart,
                Extension = Options.ExtensionFilter,
                Content = Options.ContentFilter,
                MatchFileInfo = filePredicate,
                WithAttributes = Options.Attributes,
                WithoutAttributes = Options.AttributesToSkip,
                EmptyOption = Options.EmptyOption
            };
        }

        if (Options.SearchTarget != SearchTarget.Files)
        {
            directoryMatcher = new DirectoryMatcher()
            {
                Name = Options.NameFilter,
                NamePart = Options.NamePart,
                MatchDirectoryInfo = directoryPredicate,
                WithAttributes = Options.Attributes,
                WithoutAttributes = Options.AttributesToSkip,
                EmptyOption = Options.EmptyOption
            };
        }

        Func<string, bool>? includeDirectory = null;
        Func<string, bool>? excludeDirectory = null;

        Matcher? directoryFilter = Options.DirectoryFilter;

        if (directoryFilter is not null)
        {
            if (directoryFilter.Invert)
            {
                excludeDirectory = DirectoryPredicate.Create(directoryFilter, Options.DirectoryNamePart);
            }
            else
            {
                includeDirectory = DirectoryPredicate.Create(directoryFilter, Options.DirectoryNamePart);
            }
        }

        includeDirectory = CombinePredicates(includeDirectory, CreateIncludeDirectoryPredicate());
        excludeDirectory = CombinePredicates(excludeDirectory, CreateExcludeDirectoryPredicate());

        var search = new SearchState(fileMatcher, directoryMatcher)
        {
            IncludeDirectory = includeDirectory,
            ExcludeDirectory = excludeDirectory,
            LogProgress = (ProgressReporter is not null) ? p => ProgressReporter.Report(p) : null,
            RecurseSubdirectories = Options.RecurseSubdirectories,
            DefaultEncoding = Options.DefaultEncoding,
            MaxDirectoryDepth = Options.MaxDirectoryDepth,
        };

        OnSearchCreating(search);

        return search;

        static Func<string, bool>? CombinePredicates(Func<string, bool>? predicate, Func<string, bool>? predicate2)
        {
            if (predicate is not null)
            {
                if (predicate2 is not null)
                {
                    return path => predicate(path) && predicate2(path);
                }

                return predicate;
            }

            return predicate2;
        }
    }

    protected virtual Func<string, bool>? CreateIncludeDirectoryPredicate(Func<string, bool>? predicate = null)
    {
        return predicate;
    }

    protected virtual Func<string, bool>? CreateExcludeDirectoryPredicate(Func<string, bool>? predicate = null)
    {
        return predicate;
    }

    protected abstract void ExecuteDirectory(string directoryPath, SearchContext context);

    protected abstract void ExecuteFile(string filePath, SearchContext context);

    protected abstract void ExecuteMatchCore(
        FileMatch fileMatch,
        SearchContext context,
        string? baseDirectoryPath,
        ColumnWidths? columnWidths);

    protected abstract void ExecuteResult(SearchResult result, SearchContext context, ColumnWidths? columnWidths);

    protected abstract void WriteSummary(SearchTelemetry telemetry, Verbosity verbosity);

    protected virtual void WriteBeforeSummary()
    {
    }

    protected sealed override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
    {
        List<SearchResult>? results = null;

        if (CanUseResults)
        {
            if (Options.SortOptions is not null)
            {
                results = new List<SearchResult>();
            }
            else if (Options.AlignColumns
                && !Options.FilePropertyOptions.IsEmpty)
            {
                results = new List<SearchResult>();
            }
        }

        var context = new SearchContext(
            new SearchTelemetry(),
            progress: ProgressReporter,
            results: results,
            cancellationToken: cancellationToken);

        ExecuteCore(context);

        if (context.TerminationReason == TerminationReason.Canceled)
            return CommandResult.Canceled;

        return GetCommandResult(context);
    }

    protected virtual CommandResult GetCommandResult(SearchContext context)
    {
        return (context.Telemetry.MatchingFileCount > 0 || context.Telemetry.MatchCount > 0)
            ? CommandResult.Success
            : CommandResult.NoMatch;
    }

    private ProgressReporter? CreateProgressReporter()
    {
        ProgressReportMode consoleReportMode;
        if (_logger.ConsoleOut.ShouldWrite(Verbosity.Diagnostic))
        {
            consoleReportMode = ProgressReportMode.Path;
        }
        else if (Options.Progress)
        {
            consoleReportMode = ProgressReportMode.Dot;
        }
        else
        {
            consoleReportMode = ProgressReportMode.None;
        }

        ProgressReportMode fileReportMode;
        if (_logger.Out?.ShouldWrite(Verbosity.Diagnostic) == true)
        {
            fileReportMode = ProgressReportMode.Path;
        }
        else
        {
            fileReportMode = ProgressReportMode.None;
        }

        if (fileReportMode == ProgressReportMode.None)
        {
            if (consoleReportMode == ProgressReportMode.None)
            {
                return (ShouldWriteSummary()) ? new ProgressReporter(GetPathIndent(), _logger) : null;
            }
            else if (consoleReportMode == ProgressReportMode.Dot)
            {
                return new DotProgressReporter(GetPathIndent(), _logger);
            }
        }

        return new DiagnosticProgressReporter(consoleReportMode, fileReportMode, Options, GetPathIndent(), _logger);
    }

    protected virtual void ExecuteCore(SearchContext context)
    {
        var canceled = false;
        Stopwatch stopwatch = Stopwatch.StartNew();

        foreach (PathInfo pathInfo in Options.Paths)
        {
            ExecuteCore(pathInfo.Path, context);

            if (context.TerminationReason == TerminationReason.MaxReached)
                break;

            if (context.TerminationReason == TerminationReason.Canceled
                || context.CancellationToken.IsCancellationRequested)
            {
                canceled = true;
                OperationCanceled();
                break;
            }
        }

        if (context.Results is not null)
        {
            if (context.Progress?.ProgressReported == true
                && _logger.ConsoleOut.Verbosity >= Verbosity.Minimal)
            {
                _logger.ConsoleOut.WriteLine();
                context.Progress.ProgressReported = false;
            }

            if (!canceled
                && context.Results.Count > 0)
            {
                ExecuteResults(context);
            }
        }

        stopwatch.Stop();

        WriteBeforeSummary();

        if (ShouldWriteSummary())
        {
            if (context.Progress is not null)
            {
                context.Telemetry.SearchedDirectoryCount = context.Progress.SearchedDirectoryCount;
                context.Telemetry.FileCount = context.Progress.FileCount;
                context.Telemetry.DirectoryCount = context.Progress.DirectoryCount;
            }

            context.Telemetry.Elapsed = stopwatch.Elapsed;

            WriteSummary(context.Telemetry, (Options.IncludeSummary) ? Verbosity.Quiet : Verbosity.Detailed);
        }
    }

    protected bool ShouldWriteSummary()
    {
        if (CanDisplaySummary)
        {
            if (_logger.ShouldWrite(Verbosity.Detailed)
                || Options.IncludeSummary)
            {
                return true;
            }
        }

        return false;
    }

    private void ExecuteResults(SearchContext context)
    {
        IEnumerable<SearchResult> results = context.Results!;
        SortOptions? sortOptions = Options.SortOptions;

        if (sortOptions?.Descriptors.Any() == true)
        {
            PathDisplayStyle pathDisplayStyle = Options.PathDisplayStyle;

            if (pathDisplayStyle == PathDisplayStyle.Match
                && NameFilter is null)
            {
                pathDisplayStyle = PathDisplayStyle.Full;
            }

            results = SortHelpers.SortResults(context.Results!, sortOptions, pathDisplayStyle);

            if (sortOptions.MaxCount > 0)
                results = results.Take(sortOptions.MaxCount);
        }

        ColumnWidths? columnWidths = null;

        if (!Options.FilePropertyOptions.IsEmpty
            && Options.AlignColumns)
        {
            List<SearchResult> resultList = results.ToList();

            int maxNameWidth = resultList.Max(f => f.Path.Length);
            int maxSizeWidth = 0;

            if (Options.FilePropertyOptions.IncludeSize)
            {
                long maxSize = 0;

                foreach (SearchResult result in resultList)
                {
                    long size = result.GetSize();

                    if (result.IsDirectory)
                    {
                        if (context.DirectorySizeMap is null)
                            context.DirectorySizeMap = new Dictionary<string, long>();

                        context.DirectorySizeMap[result.Path] = size;
                    }

                    if (size > maxSize)
                        maxSize = size;
                }

                maxSizeWidth = maxSize.ToString("n0").Length;
            }

            columnWidths = new ColumnWidths(maxNameWidth, maxSizeWidth);

            results = resultList;
        }

        int i = 0;

        try
        {
            foreach (SearchResult result in results)
            {
                ExecuteResult(result, context, columnWidths);
                i++;

                if (context.TerminationReason == TerminationReason.Canceled)
                    break;

                context.CancellationToken.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException)
        {
            context.TerminationReason = TerminationReason.Canceled;
        }

        if (context.TerminationReason == TerminationReason.Canceled
            || context.CancellationToken.IsCancellationRequested)
        {
            OperationCanceled();
        }

        if (Options.FilePropertyOptions.IncludeSize
            && context.Telemetry.FilesTotalSize == 0)
        {
            foreach (SearchResult result in results.Take(i))
                context.Telemetry.FilesTotalSize += result.GetSize();
        }
    }

    private void ExecuteCore(string path, SearchContext context)
    {
        if (Directory.Exists(path))
        {
            ProgressReporter? progress = context.Progress;

            progress?.SetBaseDirectoryPath(path);

            try
            {
                ExecuteDirectory(path, context);
            }
            catch (OperationCanceledException)
            {
                context.TerminationReason = TerminationReason.Canceled;
            }

            if (progress?.ProgressReported == true)
            {
                _logger.ConsoleOut.WriteLine();
                progress.ProgressReported = false;
            }

            progress?.SetBaseDirectoryPath(null);
        }
        else if (File.Exists(path))
        {
            try
            {
                ExecuteFile(path, context);
            }
            catch (OperationCanceledException)
            {
                context.TerminationReason = TerminationReason.Canceled;
            }
        }
        else
        {
            string message = $"File or directory not found: {path}";

            _logger.WriteLine(message, Colors.Message_Warning, Verbosity.Minimal);
        }
    }

    protected void EndProgress(SearchContext context)
    {
        if (context.Progress?.ProgressReported == true
            && _logger.ConsoleOut.Verbosity >= Verbosity.Minimal
            && context.Results is null)
        {
            _logger.ConsoleOut.WriteLine();
            context.Progress.ProgressReported = false;
        }
    }

    protected void ExecuteMatch(FileMatch fileMatch, SearchContext context, string? baseDirectoryPath = null)
    {
        if (fileMatch.IsDirectory)
        {
            context.Telemetry.MatchingDirectoryCount++;
        }
        else
        {
            context.Telemetry.MatchingFileCount++;
        }

        if (Options.MaxMatchingFiles == context.Telemetry.MatchingFileDirectoryCount)
            context.TerminationReason = TerminationReason.MaxReached;

        if (context.Results is not null)
        {
            var searchResult = new SearchResult(fileMatch, baseDirectoryPath);

            context.Results.Add(searchResult);
        }
        else
        {
            if (CanEndProgress)
                EndProgress(context);

            ExecuteMatchCore(fileMatch, context, baseDirectoryPath, columnWidths: null);
        }
    }

    protected IEnumerable<FileMatch> GetMatches(
        string directoryPath,
        SearchContext context)
    {
        var notifyDirectoryChanged = this as INotifyDirectoryChanged;

        return GetMatches(
            directoryPath: directoryPath,
            context: context,
            notifyDirectoryChanged: notifyDirectoryChanged);
    }

    protected IEnumerable<FileMatch> GetMatches(
        string directoryPath,
        SearchContext context,
        INotifyDirectoryChanged? notifyDirectoryChanged)
    {
        return Search.Find(
            directoryPath: directoryPath,
            notifyDirectoryChanged: notifyDirectoryChanged,
            cancellationToken: context.CancellationToken);
    }

    protected FileMatch? MatchFile(string filePath)
    {
        return Search.MatchFile(filePath);
    }

    protected string GetPathIndent(string? baseDirectoryPath)
    {
        return (baseDirectoryPath is not null) ? GetPathIndent() : "";
    }

    private string GetPathIndent()
    {
        return "";
    }

    protected void WriteProperties(SearchContext context, FileMatch fileMatch, ColumnWidths? columnWidths)
    {
        long size = -1;

        if (Options.IncludeSummary
            && Options.FilePropertyOptions.IncludeSize)
        {
            size = (fileMatch.IsDirectory)
                ? (context.DirectorySizeMap?[fileMatch.Path] ?? FileSystemUtilities.GetDirectorySize(fileMatch.Path))
                : new FileInfo(fileMatch.Path).Length;

            context.Telemetry.FilesTotalSize += size;
        }

        if (!_logger.ShouldWrite(Verbosity.Minimal))
            return;

        if (Options.FilePropertyOptions.IsEmpty)
            return;

        StringBuilder sb = StringBuilderCache.GetInstance();

        if (columnWidths is not null)
            sb.Append(' ', columnWidths.NameWidth - fileMatch.Path.Length);

        if (Options.FilePropertyOptions.IncludeSize)
        {
            sb.Append("  ");

            if (size == -1)
            {
                size = (fileMatch.IsDirectory)
                    ? (context.DirectorySizeMap?[fileMatch.Path] ?? FileSystemUtilities.GetDirectorySize(fileMatch.Path))
                    : new FileInfo(fileMatch.Path).Length;
            }

            string sizeText = size.ToString(ApplicationOptions.Default.SizeFormat);

            if (columnWidths is not null)
                sb.Append(' ', columnWidths.SizeWidth - sizeText.Length);

            sb.Append(sizeText);

            context.Telemetry.FilesTotalSize += size;
        }

        if (Options.FilePropertyOptions.IncludeCreationTime)
        {
            sb.Append("  ");
            sb.Append(File.GetCreationTime(fileMatch.Path).ToString(ApplicationOptions.Default.DateFormat));
        }

        if (Options.FilePropertyOptions.IncludeModifiedTime)
        {
            sb.Append("  ");
            sb.Append(File.GetLastWriteTime(fileMatch.Path).ToString(ApplicationOptions.Default.DateFormat));
        }

        _logger.Write(StringBuilderCache.GetStringAndFree(sb), Verbosity.Minimal);
    }
}
