// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Orang.FileSystem;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal abstract class CommonFindCommand<TOptions> : AbstractCommand where TOptions : CommonFindCommandOptions
    {
        protected CommonFindCommand(TOptions options)
        {
            Options = options;
        }

        private FileSystemFinderOptions _finderOptions;

        public TOptions Options { get; }

        protected FileSystemFinderOptions FinderOptions => _finderOptions ?? (_finderOptions = CreateFinderOptions());

        public virtual bool CanEndProgress => !Options.OmitPath;

        protected virtual FileSystemFinderOptions CreateFinderOptions()
        {
            return new FileSystemFinderOptions(
                searchTarget: Options.SearchTarget,
                recurseSubdirectories: Options.RecurseSubdirectories,
                attributes: Options.Attributes,
                attributesToSkip: Options.AttributesToSkip,
                empty: Options.Empty);
        }

        protected abstract void ExecuteDirectory(string directoryPath, SearchContext context);

        protected abstract void ExecuteFile(string filePath, SearchContext context);

        protected abstract void ExecuteResult(FileSystemFinderResult result, SearchContext context, string baseDirectoryPath, ColumnWidths columnWidths);

        protected abstract void ExecuteResult(SearchResult result, SearchContext context, ColumnWidths columnWidths);

        protected abstract void WriteSummary(SearchTelemetry telemetry, Verbosity verbosity);

        protected sealed override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            List<SearchResult> results = (Options.SortOptions != null || Options.Format.FileProperties.Any())
                ? new List<SearchResult>()
                : null;

            ProgressReportMode consoleReportMode;
            if (ConsoleOut.ShouldWrite(Verbosity.Diagnostic))
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
            if (Out?.ShouldWrite(Verbosity.Diagnostic) == true)
            {
                fileReportMode = ProgressReportMode.Path;
            }
            else
            {
                fileReportMode = ProgressReportMode.None;
            }

            var progress = new FileSystemFinderProgressReporter(consoleReportMode, fileReportMode, Options, GetPathIndent());

            var context = new SearchContext(progress: progress, results: results, cancellationToken: cancellationToken);

            ExecuteCore(context);

            return (context?.Telemetry.MatchingFileCount > 0) ? CommandResult.Success : CommandResult.NoSuccess;
        }

        protected virtual void ExecuteCore(SearchContext context)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (string path in Options.Paths)
            {
                ExecuteCore(path, context);

                if (context.State == SearchState.MaxReached)
                    break;

                if (context.State == SearchState.Canceled
                    || context.CancellationToken.IsCancellationRequested)
                {
                    OperationCanceled();
                    break;
                }
            }

            if (context.Results != null)
            {
                if (context.Progress?.ProgressReported == true
                    && ConsoleOut.Verbosity >= Verbosity.Minimal)
                {
                    ConsoleOut.WriteLine();
                    context.Progress.ProgressReported = false;
                }

                if (context.Results.Count > 0)
                    ExecuteResults(context);
            }

            stopwatch.Stop();

            if (ShouldLog(Verbosity.Detailed)
                || Options.IncludeSummary)
            {
                context.Telemetry.Elapsed = stopwatch.Elapsed;

                WriteSummary(context.Telemetry, (Options.IncludeSummary) ? Verbosity.Minimal : Verbosity.Detailed);
            }
        }

        private void ExecuteResults(SearchContext context)
        {
            IEnumerable<SearchResult> results = context.Results;
            SortOptions sortOptions = Options.SortOptions;

            if (sortOptions?.Descriptors.Any() == true)
            {
                results = SortHelpers.SortResults(context.Results, sortOptions.Descriptors, Options.PathDisplayStyle);

                if (sortOptions.MaxCount > 0)
                    results = results.Take(sortOptions.MaxCount);
            }

            ImmutableArray<FileProperty> fileProperties = Options.Format.FileProperties;
            ColumnWidths columnWidths = null;

            if (fileProperties.Any())
            {
                List<SearchResult> resultList = results.ToList();

                int maxNameWidth = resultList.Max(f => f.Path.Length);
                int maxSizeWidth = 0;

                if (fileProperties.Contains(FileProperty.Size))
                {
                    long maxSize = 0;
                    if (context.Telemetry.MaxFileSize > 0)
                    {
                        maxSize = context.Telemetry.MaxFileSize;
                    }
                    else
                    {
                        foreach (SearchResult result in resultList)
                        {
                            long size = result.GetSize();

                            if (result.IsDirectory)
                            {
                                if (context.DirectorySizeMap == null)
                                    context.DirectorySizeMap = new Dictionary<string, long>();

                                context.DirectorySizeMap[result.Path] = size;
                            }

                            if (size > maxSize)
                                maxSize = size;
                        }
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

                    if (context.State == SearchState.Canceled)
                        break;

                    context.CancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException)
            {
                context.State = SearchState.Canceled;
            }

            if (context.State == SearchState.Canceled
                || context.CancellationToken.IsCancellationRequested)
            {
                OperationCanceled();
            }

            if (Options.Format.FileProperties.Contains(FileProperty.Size)
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
                FileSystemFinderProgressReporter progress = context.Progress;

                progress.BaseDirectoryPath = path;

                if (Options.DisplayRelativePath
                    && Options.IncludeBaseDirectory)
                {
                    WriteLine(path, Colors.BasePath, Verbosity.Minimal);
                }

                try
                {
                    ExecuteDirectory(path, context);
                }
                catch (OperationCanceledException)
                {
                    context.State = SearchState.Canceled;
                }

                context.Telemetry.SearchedDirectoryCount = progress.SearchedDirectoryCount;
                context.Telemetry.FileCount = progress.FileCount;
                context.Telemetry.DirectoryCount = progress.DirectoryCount;

                if (progress.ProgressReported)
                {
                    ConsoleOut.WriteLine();
                    progress.ProgressReported = false;
                }

                progress.BaseDirectoryPath = null;
            }
            else if (File.Exists(path))
            {
                try
                {
                    ExecuteFile(path, context);
                }
                catch (OperationCanceledException)
                {
                    context.State = SearchState.Canceled;
                }
            }
            else
            {
                string message = $"File or directory not found: {path}";

                WriteLine(message, Colors.Message_Warning, Verbosity.Minimal);
            }
        }

        protected void EndProgress(SearchContext context)
        {
            if (context.Progress?.ProgressReported == true
                && ConsoleOut.Verbosity >= Verbosity.Minimal
                && context.Results == null
                && CanEndProgress)
            {
                ConsoleOut.WriteLine();
                context.Progress.ProgressReported = false;
            }
        }

        protected void ExecuteOrAddResult(FileSystemFinderResult result, SearchContext context, string baseDirectoryPath)
        {
            if (result.IsDirectory)
            {
                context.Telemetry.MatchingDirectoryCount++;
            }
            else
            {
                context.Telemetry.MatchingFileCount++;
            }

            if (Options.MaxMatchingFiles == context.Telemetry.MatchingFileCount + context.Telemetry.MatchingDirectoryCount)
                context.State = SearchState.MaxReached;

            if (context.Results != null)
            {
                context.AddResult(result, baseDirectoryPath);
            }
            else
            {
                EndProgress(context);

                ExecuteResult(result, context, baseDirectoryPath, columnWidths: null);
            }
        }

        protected string ReadFile(
            string filePath,
            string basePath,
            Encoding encoding,
            SearchContext context,
            string indent = null)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex) when (ex is IOException
                || ex is UnauthorizedAccessException)
            {
                EndProgress(context);
                LogHelpers.WriteFileError(ex, filePath, basePath, relativePath: Options.DisplayRelativePath, indent: indent);
                return null;
            }
        }

        protected IEnumerable<FileSystemFinderResult> Find(
            string directoryPath,
            SearchContext context)
        {
            return Find(
                directoryPath: directoryPath,
                context: context,
                notifyDirectoryChanged: default(INotifyDirectoryChanged));
        }

        protected IEnumerable<FileSystemFinderResult> Find(
            string directoryPath,
            SearchContext context,
            INotifyDirectoryChanged notifyDirectoryChanged)
        {
            IEnumerable<FileSystemFinderResult> results = FileSystemFinder.Find(
                directoryPath: directoryPath,
                nameFilter: Options.NameFilter,
                extensionFilter: Options.ExtensionFilter,
                directoryFilter: Options.DirectoryFilter,
                options: FinderOptions,
                progress: context.Progress,
                notifyDirectoryChanged: notifyDirectoryChanged,
                cancellationToken: context.CancellationToken);

            if (Options.FilePropertyFilter != null)
                results = results.Where(Options.FilePropertyFilter.IsMatch);

            return results;
        }

        protected FileSystemFinderResult? MatchFile(string filePath, FileSystemFinderProgressReporter progress = null)
        {
            FileSystemFinderResult? result = FileSystemFinder.MatchFile(
                filePath,
                nameFilter: Options.NameFilter,
                extensionFilter: Options.ExtensionFilter,
                options: FinderOptions,
                progress: progress);

            if (result != null
                && Options.FilePropertyFilter?.IsMatch(result.Value) == false)
            {
                return null;
            }

            return result;
        }

        protected string GetPathIndent(string baseDirectoryPath)
        {
            return (baseDirectoryPath != null) ? GetPathIndent() : "";
        }

        private string GetPathIndent()
        {
            return (Options.DisplayRelativePath && Options.IncludeBaseDirectory)
                ? Options.Indent
                : "";
        }

        protected virtual void WritePath(SearchContext context, FileSystemFinderResult result, string baseDirectoryPath, string indent, ColumnWidths columnWidths)
        {
            WritePath(context, result, baseDirectoryPath, indent, columnWidths, Colors.Match);

            WriteLine(Verbosity.Minimal);
        }

        protected void WritePath(SearchContext context, FileSystemFinderResult result, string baseDirectoryPath, string indent, ColumnWidths columnWidths, ConsoleColors matchColors)
        {
            if (Options.PathDisplayStyle == PathDisplayStyle.Match)
            {
                if (ShouldLog(Verbosity.Minimal))
                {
                    Write(indent, Verbosity.Minimal);
                    Write(result.Match.Value, (Options.HighlightMatch) ? matchColors : default, Verbosity.Minimal);
                }
            }
            else
            {
                LogHelpers.WritePath(
                    result,
                    baseDirectoryPath,
                    relativePath: Options.DisplayRelativePath,
                    colors: Colors.Matched_Path,
                    matchColors: (Options.HighlightMatch) ? matchColors : default,
                    indent: indent,
                    verbosity: Verbosity.Minimal);
            }

            WriteProperties(context, result, columnWidths);
        }

        protected void WriteProperties(SearchContext context, FileSystemFinderResult result, ColumnWidths columnWidths)
        {
            if (columnWidths != null
                && ShouldLog(Verbosity.Minimal))
            {
                StringBuilder sb = StringBuilderCache.GetInstance();

                sb.Append(' ', columnWidths.NameWidth - result.Path.Length);

                foreach (FileProperty fileProperty in Options.Format.FileProperties)
                {
                    if (fileProperty == FileProperty.Size)
                    {
                        sb.Append("  ");

                        long size = (result.IsDirectory)
                            ? context.DirectorySizeMap[result.Path]
                            : new FileInfo(result.Path).Length;

                        string sizeText = size.ToString("n0");

                        sb.Append(' ', columnWidths.SizeWidth - sizeText.Length);
                        sb.Append(sizeText);

                        context.Telemetry.FilesTotalSize += size;

                        if (size > context.Telemetry.MaxFileSize)
                            context.Telemetry.MaxFileSize = size;
                    }
                    else if (fileProperty == FileProperty.CreationTime)
                    {
                        sb.Append("  ");
                        sb.Append(File.GetCreationTime(result.Path).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else if (fileProperty == FileProperty.ModifiedTime)
                    {
                        sb.Append("  ");
                        sb.Append(File.GetLastWriteTime(result.Path).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unknown enum value '{fileProperty}'.");
                    }
                }

                Write(StringBuilderCache.GetStringAndFree(sb), Verbosity.Minimal);
            }
        }
    }
}
