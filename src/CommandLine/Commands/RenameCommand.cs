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
        }

        public override bool CanEnumerate => Options.DryRun;

        public event EventHandler<DirectoryChangedEventArgs> DirectoryChanged;

        protected override void ExecuteFile(string filePath, SearchContext context)
        {
            SearchTelemetry telemetry = context.Telemetry;
            telemetry.FileCount++;

            const string indent = null;

            FileSystemFinderResult? maybeResult = MatchFile(filePath);

            if (maybeResult == null)
                return;

            FileSystemFinderResult result = maybeResult.Value;

            if (Options.ContentFilter != null)
            {
                string input = ReadFile(filePath, null, Options.DefaultEncoding, null, indent);

                if (input == null)
                    return;

                if (!Options.ContentFilter.IsMatch(input))
                    return;
            }

            telemetry.MatchingFileCount++;

            NamePart part = result.Part;

            WritePath(result, colors: Colors.Matched_Path, matchColors: (Options.HighlightMatch) ? Colors.Match : default, indent: indent);
            WriteLine();

            string newPath = GetNewPath(filePath, null, part, indent);

            if (newPath == null)
                return;

            bool success = false;

            if (!Options.DryRun
                && (!Options.Ask || AskToRename(context, indent)))
            {
                try
                {
                    File.Move(filePath, newPath);

                    telemetry.ProcessedFileCount++;
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

            if (Options.MaxMatchingFiles == telemetry.MatchingFileCount + telemetry.MatchingDirectoryCount)
            {
                context.State = SearchState.MaxReached;
            }
        }

        protected override void ExecuteDirectory(string directoryPath, SearchContext context, FileSystemFinderProgressReporter progress)
        {
            SearchTelemetry telemetry = context.Telemetry;
            string indent = (Options.PathDisplayStyle == PathDisplayStyle.Relative) ? Options.Indent : "";

            foreach (FileSystemFinderResult result in Find(directoryPath, progress, notifyDirectoryChanged: this, context.CancellationToken))
            {
                string path = result.Path;

                if (result.IsDirectory)
                {
                    telemetry.MatchingDirectoryCount++;
                }
                else
                {
                    if (Options.ContentFilter != null)
                    {
                        string input = ReadFile(path, directoryPath, Options.DefaultEncoding, progress, indent);

                        if (input == null)
                            continue;

                        if (!Options.ContentFilter.IsMatch(input))
                            continue;
                    }

                    telemetry.MatchingFileCount++;
                }

                NamePart part = result.Part;

                Debug.Assert(path.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase), $"{directoryPath}\r\n{path}");

                EndProgress(progress);

                WritePath(
                    result,
                    directoryPath,
                    relativePath: Options.PathDisplayStyle == PathDisplayStyle.Relative,
                    colors: Colors.Matched_Path,
                    matchColors: (Options.HighlightMatch) ? Colors.Match : default,
                    indent: indent);

                WriteLine();

                string newPath = GetNewPath(path, directoryPath, part, indent);

                if (newPath == null)
                    continue;

                bool success = false;

                if (!Options.DryRun
                    && (!Options.Ask || AskToRename(context, indent)))
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
                            telemetry.ProcessedDirectoryCount++;
                        }
                        else
                        {
                            telemetry.ProcessedFileCount++;
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

                if (context.State == SearchState.Canceled)
                    break;

                if (Options.MaxMatchingFiles == telemetry.MatchingFileCount + telemetry.MatchingDirectoryCount)
                {
                    context.State = SearchState.MaxReached;
                    break;
                }
            }
        }

        private bool AskToRename(SearchContext context, string indent = null)
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

        private string GetNewPath(string path, string directoryPath, NamePart part, string indent)
        {
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
                && directoryPath != null
                && path.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase))
            {
                indentCount -= directoryPath.Length;

                if (fileNameIndex > 0
                    && FileSystemHelpers.IsDirectorySeparator(path[fileNameIndex - 1]))
                {
                    indentCount--;
                }
            }

            indentCount += indent?.Length ?? 0;

            Write(' ', indentCount);
            Write(path, fileNameIndex, index - fileNameIndex);
            Write(replacement, (Options.HighlightReplacement) ? Colors.Replacement : default);
            Write(path, endIndex, path.Length - endIndex);

            if (string.Equals(path, newPath, StringComparison.Ordinal))
            {
                WriteLine(" NO CHANGE", Colors.Message_Warning);
                return null;
            }

            WriteLine();

            return newPath;
        }

        protected virtual void OnDirectoryChanged(DirectoryChangedEventArgs e)
        {
            DirectoryChanged?.Invoke(this, e);
        }

        protected override void WriteSummary(SearchTelemetry telemetry)
        {
            WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget);
            WriteProcessedFilesAndDirectories(telemetry, Options.SearchTarget, "Renamed files", "Renamed directories");
        }
    }
}
