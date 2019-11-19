// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using Orang.FileSystem;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class DeleteCommand : CommonFindCommand<DeleteCommandOptions>, INotifyDirectoryChanged
    {
        public DeleteCommand(DeleteCommandOptions options) : base(options)
        {
        }

        public event EventHandler<DirectoryChangedEventArgs> DirectoryChanged;

        protected override void ExecuteFile(string filePath, SearchContext context)
        {
            context.Telemetry.FileCount++;

            FileSystemFinderResult? maybeResult = MatchFile(filePath);

            if (maybeResult != null)
                ProcessResult(maybeResult.Value, context);
        }

        protected override void ExecuteDirectory(string directoryPath, SearchContext context)
        {
            foreach (FileSystemFinderResult result in Find(directoryPath, context, notifyDirectoryChanged: this))
            {
                Debug.Assert(result.Path.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase), $"{directoryPath}\r\n{result.Path}");

                ProcessResult(result, context, directoryPath);

                if (context.State == SearchState.Canceled)
                    break;

                if (context.State == SearchState.MaxReached)
                    break;
            }
        }

        private void ProcessResult(
            FileSystemFinderResult result,
            SearchContext context,
            string baseDirectoryPath = null)
        {
            Debug.Assert(baseDirectoryPath == null || result.Path.StartsWith(baseDirectoryPath, StringComparison.OrdinalIgnoreCase), $"{baseDirectoryPath}\r\n{result.Path}");

            if (!result.IsDirectory
                && Options.ContentFilter != null)
            {
                string indent = (baseDirectoryPath != null && Options.PathDisplayStyle == PathDisplayStyle.Relative)
                    ? Options.Indent
                    : "";

                string input = ReadFile(result.Path, baseDirectoryPath, Options.DefaultEncoding, context, indent);

                if (input == null)
                    return;

                if (!Options.ContentFilter.IsMatch(input))
                    return;
            }

            ExecuteOrAddResult(result, context, baseDirectoryPath);
        }

        protected override void ExecuteResult(SearchResult result, SearchContext context)
        {
            ExecuteResult(result.Result, context, result.BaseDirectoryPath);
        }

        protected override void ExecuteResult(
            FileSystemFinderResult result,
            SearchContext context,
            string baseDirectoryPath = null)
        {
            string indent = (baseDirectoryPath != null && Options.PathDisplayStyle == PathDisplayStyle.Relative)
                ? Options.Indent
                : "";

            if (!Options.OmitPath)
            {
                WritePath(
                    result,
                    baseDirectoryPath,
                    relativePath: Options.PathDisplayStyle == PathDisplayStyle.Relative,
                    colors: Colors.Matched_Path,
                    matchColors: (Options.HighlightMatch) ? Colors.Match : default,
                    indent: indent,
                    verbosity: Verbosity.Minimal);

                WriteLine(Verbosity.Minimal);
            }

            bool success = false;

            if (!Options.DryRun
                && (!Options.Ask || AskToDelete()))
            {
                try
                {
                    FileSystemHelpers.Delete(
                        result,
                        contentOnly: Options.ContentOnly,
                        includingBom: Options.IncludingBom,
                        filesOnly: Options.FilesOnly,
                        directoriesOnly: Options.DirectoriesOnly);

                    if (result.IsDirectory)
                    {
                        context.Telemetry.ProcessedDirectoryCount++;
                    }
                    else
                    {
                        context.Telemetry.ProcessedFileCount++;
                    }

                    success = true;
                }
                catch (Exception ex) when (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    WriteFileError(ex, indent: indent);
                }
            }

            if (Options.DryRun
                || success)
            {
                context.Output?.WriteLine(result.Path);
            }

            if (result.IsDirectory
                && success)
            {
                OnDirectoryChanged(new DirectoryChangedEventArgs(result.Path, null));
            }

            bool AskToDelete()
            {
                try
                {
                    return ConsoleHelpers.Question(
                        (Options.ContentOnly) ? "Delete content?" : "Delete?",
                        indent);
                }
                catch (OperationCanceledException)
                {
                    context.State = SearchState.Canceled;
                    return false;
                }
            }
        }

        protected virtual void OnDirectoryChanged(DirectoryChangedEventArgs e)
        {
            DirectoryChanged?.Invoke(this, e);
        }

        protected override void WriteSummary(SearchTelemetry telemetry, Verbosity verbosity)
        {
            WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget, verbosity);

            string filesTitle = (Options.ContentOnly)
                ? "Deleted files content"
                : "Deleted files";

            string directoriesTitle = (Options.ContentOnly)
                ? "Deleted directories content"
                : "Deleted directories";

            WriteProcessedFilesAndDirectories(telemetry, Options.SearchTarget, filesTitle, directoriesTitle, verbosity);
        }
    }
}
