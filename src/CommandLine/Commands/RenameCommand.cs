// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Orang.FileSystem;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class RenameCommand : CommonFindCommand<RenameCommandOptions>, INotifyDirectoryChanged
    {
        public RenameCommand(RenameCommandOptions options) : base(options)
        {
            Debug.Assert(options.ContentFilter?.IsNegative != true);
        }

        public event EventHandler<DirectoryChangedEventArgs> DirectoryChanged;

        protected override FileSystemFinderOptions CreateFinderOptions()
        {
            return new FileSystemFinderOptions(
                searchTarget: Options.SearchTarget,
                recurseSubdirectories: Options.RecurseSubdirectories,
                attributes: Options.Attributes,
                attributesToSkip: Options.AttributesToSkip,
                empty: Options.Empty,
                canEnumerate: Options.DryRun,
                partOnly: true);
        }

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
                Debug.Assert(result.Path.StartsWith(directoryPath, FileSystemHelpers.Comparison), $"{directoryPath}\r\n{result.Path}");

                ProcessResult(result, context, directoryPath);

                if (context.TerminationReason == TerminationReason.Canceled)
                    break;

                if (context.TerminationReason == TerminationReason.MaxReached)
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
                string indent = GetPathIndent(baseDirectoryPath);

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
            string indent = GetPathIndent(baseDirectoryPath);

            List<ReplaceItem> replaceItems = GetReplaceItems(result, context.CancellationToken);

            if (!Options.OmitPath)
            {
                LogHelpers.WritePath(
                    result,
                    replaceItems,
                    baseDirectoryPath,
                    relativePath: Options.DisplayRelativePath,
                    colors: Colors.Matched_Path,
                    matchColors: (Options.HighlightMatch) ? Colors.Match : default,
                    replaceColors: (Options.HighlightReplacement) ? Colors.Replacement : default,
                    indent: indent,
                    verbosity: Verbosity.Minimal);

                WriteProperties(context, result, columnWidths);
                WriteLine(Verbosity.Minimal);
            }

            string path = result.Path;
            string newPath = GetNewPath(result, replaceItems);

            ListCache<ReplaceItem>.Free(replaceItems);

            if (string.Equals(path, newPath, StringComparison.Ordinal))
                return;

            bool renamed = false;

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

                    renamed = true;
                }
                catch (Exception ex) when (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    WriteFileError(ex, indent: indent);
                }
            }

            if (Options.DryRun || renamed)
            {
                if (result.IsDirectory)
                {
                    context.Telemetry.ProcessedDirectoryCount++;
                }
                else
                {
                    context.Telemetry.ProcessedFileCount++;
                }
            }

            if (result.IsDirectory
                && renamed)
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
                    context.TerminationReason = TerminationReason.Canceled;
                    return false;
                }
            }
        }

        private List<ReplaceItem> GetReplaceItems(FileSystemFinderResult result, CancellationToken cancellationToken)
        {
            List<Match> matches = GetMatches(result.Match);

            int offset = 0;
            List<ReplaceItem> items = ListCache<ReplaceItem>.GetInstance();

            foreach (Match match in matches)
            {
                string value = Options.ReplaceOptions.Replace(match);

                items.Add(new ReplaceItem(match, value, match.Index + offset));

                offset += value.Length - match.Length;

                cancellationToken.ThrowIfCancellationRequested();
            }

            ListCache<Match>.Free(matches);

            return items;

            static List<Match> GetMatches(Match match)
            {
                List<Match> matches = ListCache<Match>.GetInstance();

                do
                {
                    matches.Add(match);

                    match = match.NextMatch();

                } while (match.Success);

                if (matches.Count > 1
                    && matches[0].Index > matches[1].Index)
                {
                    matches.Reverse();
                }

                return matches;
            }
        }

        private string GetNewPath(FileSystemFinderResult result, List<ReplaceItem> items)
        {
            StringBuilder sb = StringBuilderCache.GetInstance();

            string path = result.Path;
            NamePart part = result.Part;

            sb.Append(path, 0, part.Index);

            int lastPos = part.Index;

            foreach (ReplaceItem item in items)
            {
                Match match = item.Match;

                sb.Append(path, lastPos, part.Index + match.Index - lastPos);
                sb.Append(item.Value);

                lastPos = part.Index + match.Index + match.Length;
            }

            sb.Append(path, lastPos, path.Length - lastPos);

            return StringBuilderCache.GetStringAndFree(sb);
        }

        protected virtual void OnDirectoryChanged(DirectoryChangedEventArgs e)
        {
            DirectoryChanged?.Invoke(this, e);
        }

        protected override void WriteSummary(SearchTelemetry telemetry, Verbosity verbosity)
        {
            WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget, verbosity);
            WriteProcessedFilesAndDirectories(telemetry, Options.SearchTarget, "Renamed files", "Renamed directories", Options.DryRun, verbosity);
        }
    }
}
