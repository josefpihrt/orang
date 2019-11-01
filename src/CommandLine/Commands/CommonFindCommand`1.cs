// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        private ProgressReporterMode? _reporterMode;

        public TOptions Options { get; }

        protected virtual bool CanExecuteFile => false;

        private ProgressReporterMode ReporterMode
        {
            get
            {
                if (_reporterMode == null)
                {
                    if (ShouldLog(Verbosity.Detailed))
                    {
                        _reporterMode = ProgressReporterMode.Path;
                    }
                    else if (Options.Progress)
                    {
                        _reporterMode = ProgressReporterMode.Dot;
                    }
                    else
                    {
                        _reporterMode = ProgressReporterMode.None;
                    }
                }

                return _reporterMode.Value;
            }
        }

        protected abstract void WriteSummary(SearchTelemetry telemetry);

        protected sealed override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            var context = new SearchContext(cancellationToken: cancellationToken);

            ExecuteCore(context);

            return (context.Telemetry.MatchingFileCount > 0) ? CommandResult.Success : CommandResult.NoSuccess;
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
                var progress = new FileSystemFinderProgressReporter(path, mode: ReporterMode, Options);

                WriteLine($"Searching in {path}", Colors.Path_Progress, Verbosity.Minimal);

                try
                {
                    ExecuteDirectory(path, context, progress);
                }
                catch (OperationCanceledException)
                {
                    context.State = SearchState.Canceled;
                }

                if (progress?.ProgressReported == true)
                {
                    ConsoleOut.WriteLine();
                    progress.ProgressReported = false;
                }

                WriteLine($"Done searching in {path}", Colors.Path_Progress, Verbosity.Minimal);
            }
            else if (CanExecuteFile
                && File.Exists(path))
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
                string message = (CanExecuteFile)
                    ? $"File or directory not found: {path}"
                    : $"Directory not found: {path}";

                WriteLine(message, Colors.Message_Warning, Verbosity.Minimal);
            }
        }

        protected abstract void ExecuteDirectory(
            string directoryPath,
            SearchContext context,
            FileSystemFinderProgressReporter progress);

        protected virtual void ExecuteFile(
            string filePath,
            SearchContext context)
        {
        }

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
    }
}
