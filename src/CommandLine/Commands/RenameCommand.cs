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
    internal class RenameCommand : DeleteOrRenameCommand<RenameCommandOptions>
    {
        public RenameCommand(RenameCommandOptions options) : base(options)
        {
            Debug.Assert(options.ContentFilter?.IsNegative != true);
        }

        protected override FileSystemFinderOptions CreateFinderOptions()
        {
            return new FileSystemFinderOptions(
                searchTarget: Options.SearchTarget,
                recurseSubdirectories: Options.RecurseSubdirectories,
                attributes: Options.Attributes,
                attributesToSkip: Options.AttributesToSkip,
                empty: Options.Empty,
                canEnumerate: Options.DryRun,
                partOnly: true,
                encoding: Options.DefaultEncoding);
        }

        protected override void ProcessResult(
            FileSystemFinderResult result,
            SearchContext context,
            string baseDirectoryPath = null)
        {
            ExecuteOrAddResult(result, context, baseDirectoryPath);
        }

        protected override void ExecuteResult(
            FileSystemFinderResult result,
            SearchContext context,
            string baseDirectoryPath,
            ColumnWidths columnWidths)
        {
            string indent = GetPathIndent(baseDirectoryPath);

            List<ReplaceItem> replaceItems = GetReplaceItems(result, context.CancellationToken);

            string path = result.Path;
            string newPath = GetNewPath(result, replaceItems);
            bool changed = !string.Equals(path, newPath, StringComparison.Ordinal);

            if (!Options.OmitPath
                && changed)
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

            ListCache<ReplaceItem>.Free(replaceItems);

            if (!changed)
                return;

            bool renamed = false;

            if (!Options.Ask || AskToExecute(context, "Rename?", indent))
            {
                try
                {
                    if (!Options.DryRun)
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

                    if (result.IsDirectory)
                    {
                        context.Telemetry.ProcessedDirectoryCount++;
                    }
                    else
                    {
                        context.Telemetry.ProcessedFileCount++;
                    }
                }
                catch (Exception ex) when (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    WriteFileError(ex, indent: indent);
                }
            }

            if (result.IsDirectory
                && renamed)
            {
                OnDirectoryChanged(new DirectoryChangedEventArgs(path, newPath));
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

        protected override void WriteSummary(SearchTelemetry telemetry, Verbosity verbosity)
        {
            WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget, verbosity);
            WriteProcessedFilesAndDirectories(telemetry, Options.SearchTarget, "Renamed files", "Renamed directories", Options.DryRun, verbosity);
        }
    }
}
