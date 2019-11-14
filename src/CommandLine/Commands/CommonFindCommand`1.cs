// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        private ProgressReportMode? _consoleReportMode;
        private ProgressReportMode? _fileLogReportMode;
        private FileSystemFinderOptions _finderOptions;

        public TOptions Options { get; }

        private ProgressReportMode ConsoleReportMode
        {
            get
            {
                if (_consoleReportMode == null)
                {
                    if (ConsoleOut.ShouldWrite(Verbosity.Detailed))
                    {
                        _consoleReportMode = ProgressReportMode.Path;
                    }
                    else if (Options.Progress)
                    {
                        _consoleReportMode = ProgressReportMode.Dot;
                    }
                    else
                    {
                        _consoleReportMode = ProgressReportMode.None;
                    }
                }

                return _consoleReportMode.Value;
            }
        }

        private ProgressReportMode FileLogReportMode
        {
            get
            {
                if (_fileLogReportMode == null)
                {
                    if (Out?.ShouldWrite(Verbosity.Detailed) == true)
                    {
                        _fileLogReportMode = ProgressReportMode.Path;
                    }
                    else
                    {
                        _fileLogReportMode = ProgressReportMode.None;
                    }
                }

                return _fileLogReportMode.Value;
            }
        }

        protected FileSystemFinderOptions FinderOptions
        {
            get
            {
                return _finderOptions ?? (_finderOptions = new FileSystemFinderOptions(
                    searchTarget: Options.SearchTarget,
                    recurseSubdirectories: Options.RecurseSubdirectories,
                    attributes: Options.Attributes,
                    attributesToSkip: Options.AttributesToSkip,
                    empty: Options.Empty,
                    canEnumerate: CanEnumerate));
            }
        }

        public virtual bool CanEnumerate => true;

        protected abstract void WriteSummary(SearchTelemetry telemetry);

        protected sealed override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            SearchContext context = null;
            StreamWriter writer = null;

            try
            {
                string path = Options.OutputPath;

                if (path != null)
                {
                    WriteLine($"Opening '{path}'", Verbosity.Diagnostic);

                    writer = new StreamWriter(path, false, Options.Output.Encoding);
                }

                context = new SearchContext(output: writer, cancellationToken: cancellationToken);

                ExecuteCore(context);
            }
            catch (Exception ex) when (ex is IOException
                || ex is UnauthorizedAccessException)
            {
                WriteWarning(ex);
            }
            finally
            {
                writer?.Dispose();
            }

            return (context?.Telemetry.MatchingFileCount > 0) ? CommandResult.Success : CommandResult.NoSuccess;
        }

        protected virtual void ExecuteCore(SearchContext context)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (string path in Options.Paths)
            {
                ExecuteCore(path, context);

                if (context.State == SearchState.MaxReached)
                {
                    break;
                }
                else if (context.State == SearchState.Canceled
                    || context.CancellationToken.IsCancellationRequested)
                {
                    OperationCanceled();
                    break;
                }
            }

            stopwatch.Stop();

            SearchTelemetry telemetry = context.Telemetry;

            telemetry.Elapsed = stopwatch.Elapsed;

            WriteSummary(telemetry);
        }

        private void ExecuteCore(string path, SearchContext context)
        {
            if (Directory.Exists(path))
            {
                var progress = new FileSystemFinderProgressReporter(path, ConsoleReportMode, FileLogReportMode, Options);

                if (Options.PathDisplayStyle == PathDisplayStyle.Relative)
                    WriteLine($"Searching in {path}", Colors.Path_Progress, Verbosity.Minimal);

                try
                {
                    ExecuteDirectory(path, context, progress);
                }
                catch (OperationCanceledException)
                {
                    context.State = SearchState.Canceled;
                }

                context.Telemetry.SearchedDirectoryCount = progress.SearchedDirectoryCount;
                context.Telemetry.FileCount = progress.FileCount;
                context.Telemetry.DirectoryCount = progress.DirectoryCount;

                if (progress?.ProgressReported == true)
                {
                    ConsoleOut.WriteLine();
                    progress.ProgressReported = false;
                }

                if (Options.PathDisplayStyle == PathDisplayStyle.Relative)
                    WriteLine($"Done searching in {path}", Colors.Path_Progress, Verbosity.Minimal);
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

        protected abstract void ExecuteDirectory(
            string directoryPath,
            SearchContext context,
            FileSystemFinderProgressReporter progress);

        protected abstract void ExecuteFile(
            string filePath,
            SearchContext context);

        protected void EndProgress(FileSystemFinderProgressReporter progress)
        {
            if (progress?.ProgressReported == true
                && ConsoleOut.Verbosity >= Verbosity.Minimal)
            {
                ConsoleOut.WriteLine();
                progress.ProgressReported = false;
            }
        }

        protected string ReadFile(
            string filePath,
            string basePath,
            Encoding encoding,
            FileSystemFinderProgressReporter progress = null,
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
                EndProgress(progress);
                LogHelpers.WriteFileError(ex, filePath, basePath, indent: indent);
                return null;
            }
        }

        protected IEnumerable<FileSystemFinderResult> Find(
            string directoryPath,
            FileSystemFinderProgressReporter progress,
            in CancellationToken cancellationToken = default)
        {
            return Find(
                directoryPath: directoryPath,
                progress: progress,
                notifyDirectoryChanged: default(INotifyDirectoryChanged),
                cancellationToken: cancellationToken);
        }

        protected IEnumerable<FileSystemFinderResult> Find(
            string directoryPath,
            FileSystemFinderProgressReporter progress,
            INotifyDirectoryChanged notifyDirectoryChanged,
            in CancellationToken cancellationToken = default)
        {
            return FileSystemFinder.Find(
                directoryPath: directoryPath,
                nameFilter: Options.NameFilter,
                extensionFilter: Options.ExtensionFilter,
                directoryFilter: Options.DirectoryFilter,
                options: FinderOptions,
                progress: progress,
                notifyDirectoryChanged: notifyDirectoryChanged,
                cancellationToken: cancellationToken);
        }

        protected FileSystemFinderResult? MatchFile(
            string filePath,
            FileSystemFinderProgressReporter progress = null)
        {
            return FileSystemFinder.MatchFile(
                filePath,
                nameFilter: Options.NameFilter,
                extensionFilter: Options.ExtensionFilter,
                options: FinderOptions,
                progress: progress);
        }
    }
}
