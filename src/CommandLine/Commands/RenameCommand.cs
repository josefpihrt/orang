// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Orang.FileSystem;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class RenameCommand : CommonFindCommand<RenameCommandOptions>, INotifyDirectoryChanged
    {
        public RenameCommand(RenameCommandOptions options) : base(options)
        {
            Debug.Assert(!options.ContentFilter.IsNegative);
        }

        public override bool CanEnumerate => Options.DryRun;

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

        protected override void ExecuteResult(SearchResult result, SearchContext context, ColumnWidths columnWidths)
        {
            ExecuteResult(result.Result, context, result.BaseDirectoryPath, columnWidths);
        }

        protected override void ExecuteResult(
            FileSystemFinderResult result,
            SearchContext context,
            string baseDirectoryPath,
            ColumnWidths columnWidths)
        {
            string indent = (baseDirectoryPath != null && Options.PathDisplayStyle == PathDisplayStyle.Relative)
                ? Options.Indent
                : "";

            string path = result.Path;

            if (!Options.OmitPath)
            {
                WritePath(
                    result,
                    baseDirectoryPath,
                    relativePath: Options.PathDisplayStyle == PathDisplayStyle.Relative,
                    colors: Colors.Matched_Path,
                    matchColors: (Options.HighlightMatch) ? Colors.Match : default,
                    indent: indent,
                    fileProperties: Options.Format.FileProperties,
                    columnWidths: columnWidths,
                    verbosity: Verbosity.Minimal);

                WriteLine(Verbosity.Minimal);
            }

            NamePart part = result.Part;

            Match match = Options.NameFilter.Regex.Match(path.Substring(part.Index, part.Length));

            string replacement = (Options.MatchEvaluator != null)
                ? Options.MatchEvaluator(match)
                : match.Result(Options.Replacement);

            int index = part.Index + match.Index;
            int endIndex = index + match.Length;

            string newPath = path.Remove(index) + replacement + path.Substring(endIndex);

            int fileNameIndex = part.GetFileNameIndex();

            int indentCount = fileNameIndex;

            if (Options.PathDisplayStyle == PathDisplayStyle.Relative
                && baseDirectoryPath != null
                && path.StartsWith(baseDirectoryPath, StringComparison.OrdinalIgnoreCase))
            {
                indentCount -= baseDirectoryPath.Length;

                if (fileNameIndex > 0
                    && FileSystemHelpers.IsDirectorySeparator(path[fileNameIndex - 1]))
                {
                    indentCount--;
                }
            }

            indentCount += indent?.Length ?? 0;

            if (!Options.OmitPath)
            {
                Write(' ', indentCount);
                Write(path, fileNameIndex, index - fileNameIndex);
                Write(replacement, (Options.HighlightReplacement) ? Colors.Replacement : default);
                Write(path, endIndex, path.Length - endIndex);

                if (string.Equals(path, newPath, StringComparison.Ordinal))
                {
                    WriteLine(" NO CHANGE", Colors.Message_Warning);
                    return;
                }

                WriteLine();
            }

            if (newPath == null)
                return;

            bool success = false;

            if (!Options.DryRun
                && (!Options.Ask || AskToRename()))
            {
                try
                {
                    if (result.IsDirectory)
                    {
                        Directory.Move(path, newPath);
                    }
                    else
                    {
                        File.Move(path, newPath);
                    }

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
                context.Output?.WriteLine(newPath);
            }

            if (result.IsDirectory
                && success)
            {
                OnDirectoryChanged(new DirectoryChangedEventArgs(path, newPath));
            }

            bool AskToRename()
            {
                try
                {
                    return ConsoleHelpers.Question("Rename?", indent);
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
            WriteProcessedFilesAndDirectories(telemetry, Options.SearchTarget, "Renamed files", "Renamed directories", verbosity);
        }
    }
}
