// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Orang.FileSystem;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal abstract class CommonFindContentCommand<TOptions> : CommonFindCommand<TOptions> where TOptions : CommonFindContentCommandOptions
    {
        private ContentWriterOptions _fileWriterOptions;
        private ContentWriterOptions _directoryWriterOptions;

        protected CommonFindContentCommand(TOptions options) : base(options)
        {
        }

        public override bool CanEndProgress => true;

        protected ContentWriterOptions FileWriterOptions
        {
            get
            {
                if (_fileWriterOptions == null)
                {
                    string indent = (Options.OmitPath) ? "" : Options.Indent;

                    _fileWriterOptions = CreateContentWriterOptions(indent);
                }

                return _fileWriterOptions;
            }
        }

        protected ContentWriterOptions DirectoryWriterOptions
        {
            get
            {
                if (_directoryWriterOptions == null)
                {
                    string indent = GetIndent();

                    _directoryWriterOptions = CreateContentWriterOptions(indent);
                }

                return _directoryWriterOptions;

                string GetIndent()
                {
                    return Options.PathDisplayStyle switch
                    {
                        PathDisplayStyle.Full => Options.Indent,
                        PathDisplayStyle.Relative => (Options.IncludeBaseDirectory) ? Options.DoubleIndent : Options.Indent,
                        PathDisplayStyle.Omit => "",
                        _ => throw new InvalidOperationException(),
                    };
                }
            }
        }

        protected abstract ContentWriterOptions CreateContentWriterOptions(string indent);

        protected abstract void ExecuteResult(
            FileSystemFinderResult result,
            SearchContext context,
            ContentWriterOptions writerOptions,
            Match match,
            string input,
            Encoding encoding,
            string baseDirectoryPath = null,
            ColumnWidths columnWidths = null);

        protected override void ExecuteResult(FileSystemFinderResult result, SearchContext context, string baseDirectoryPath, ColumnWidths columnWidths)
        {
            throw new NotSupportedException();
        }

        protected override void ExecuteResult(SearchResult result, SearchContext context, ColumnWidths columnWidths)
        {
            ExecuteResult((ContentSearchResult)result, context, columnWidths);
        }

        private void ExecuteResult(ContentSearchResult result, SearchContext context, ColumnWidths columnWidths)
        {
            ExecuteResult(result.Result, context, result.WriterOptions, result.Match, result.Input, result.Encoding, result.BaseDirectoryPath, columnWidths);
        }

        protected void ExecuteOrAddResult(
            FileSystemFinderResult result,
            SearchContext context,
            ContentWriterOptions writerOptions,
            Match match,
            string input,
            Encoding encoding,
            string baseDirectoryPath = null)
        {
            context.Telemetry.MatchingFileCount++;

            if (Options.MaxMatchingFiles == context.Telemetry.MatchingFileCount)
                context.State = SearchState.MaxReached;

            if (context.Results != null)
            {
                context.AddResult(result, baseDirectoryPath, match, input, encoding: encoding, writerOptions);
            }
            else
            {
                EndProgress(context);

                ExecuteResult(result, context, writerOptions, match, input, encoding, baseDirectoryPath);
            }
        }

        protected void WriteMatches(ContentWriter writer, IEnumerable<Group> groups, SearchContext context)
        {
            try
            {
                writer.WriteMatches(groups, context.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                context.State = SearchState.Canceled;
            }
        }

        protected MaxReason GetGroups(
            Match match,
            int groupNumber,
            SearchContext context,
            bool isPathWritten,
            Func<Group, bool> predicate,
            List<Group> groups)
        {
            int maxMatchesInFile = Options.MaxMatchesInFile;
            int maxMatches = Options.MaxMatches;

            int count = 0;

            if (maxMatchesInFile > 0)
            {
                if (maxMatches > 0)
                {
                    maxMatches -= context.Telemetry.MatchCount;
                    count = Math.Min(maxMatchesInFile, maxMatches);
                }
                else
                {
                    count = maxMatchesInFile;
                }
            }
            else if (maxMatches > 0)
            {
                maxMatches -= context.Telemetry.MatchCount;
                count = maxMatches;
            }

            Debug.Assert(count >= 0, count.ToString());

            MaxReason maxReason;
            if (groupNumber < 1)
            {
                maxReason = GetMatches(groups, match, count, predicate, context.CancellationToken);
            }
            else
            {
                maxReason = GetGroups(groups, match, groupNumber, count, predicate, context.CancellationToken);
            }

            if (groups.Count > 1
                && groups[0].Index > groups[1].Index)
            {
                groups.Reverse();
            }

            if ((maxReason == MaxReason.CountEqualsMax || maxReason == MaxReason.CountExceedsMax)
                && maxMatches > 0
                && (maxMatchesInFile == 0 || maxMatches <= maxMatchesInFile))
            {
                context.State = SearchState.MaxReached;
            }

            if (isPathWritten)
            {
                Verbosity verbosity = ConsoleOut.Verbosity;

                if (verbosity >= Verbosity.Detailed
                    || Options.IncludeCount)
                {
                    verbosity = (Options.IncludeCount) ? Verbosity.Minimal : Verbosity.Detailed;

                    ConsoleOut.Write("  ", Colors.Message_OK, verbosity);
                    ConsoleOut.Write(groups.Count.ToString("n0"), Colors.Message_OK, verbosity);
                    ConsoleOut.WriteIf(maxReason == MaxReason.CountExceedsMax, "+", Colors.Message_OK, verbosity);
                }

                ConsoleOut.WriteLine(Verbosity.Minimal);

                if (Out != null)
                {
                    verbosity = Out.Verbosity;

                    if (verbosity >= Verbosity.Detailed
                        || Options.IncludeCount)
                    {
                        verbosity = (Options.IncludeCount) ? Verbosity.Minimal : Verbosity.Detailed;

                        Out.Write("  ", verbosity);
                        Out.Write(groups.Count.ToString("n0"), verbosity);
                        Out.WriteIf(maxReason == MaxReason.CountExceedsMax, "+", verbosity);
                    }

                    Out.WriteLine(Verbosity.Minimal);
                }
            }

            return maxReason;
        }

        private MaxReason GetMatches(
            List<Group> groups,
            Match match,
            int count,
            Func<Group, bool> predicate,
            CancellationToken cancellationToken)
        {
            do
            {
                groups.Add(match);

                if (predicate != null)
                {
                    Match m = match.NextMatch();

                    match = Match.Empty;

                    while (m.Success)
                    {
                        if (predicate(m))
                        {
                            match = m;
                            break;
                        }

                        m = m.NextMatch();
                    }
                }
                else
                {
                    match = match.NextMatch();
                }

                if (groups.Count == count)
                {
                    return (match.Success)
                        ? MaxReason.CountExceedsMax
                        : MaxReason.CountEqualsMax;
                }

                cancellationToken.ThrowIfCancellationRequested();

            } while (match.Success);

            return MaxReason.None;
        }

        private MaxReason GetGroups(
            List<Group> groups,
            Match match,
            int groupNumber,
            int count,
            Func<Group, bool> predicate,
            CancellationToken cancellationToken)
        {
            Group group = match.Groups[groupNumber];

            do
            {
                groups.Add(group);

                group = NextGroup();

                if (groups.Count == count)
                {
                    return (group.Success)
                        ? MaxReason.CountExceedsMax
                        : MaxReason.CountEqualsMax;
                }

                cancellationToken.ThrowIfCancellationRequested();

            } while (group.Success);

            return MaxReason.None;

            Group NextGroup()
            {
                while (true)
                {
                    match = match.NextMatch();

                    if (!match.Success)
                        break;

                    Group g = match.Groups[groupNumber];

                    if (g.Success
                        && predicate?.Invoke(g) != false)
                    {
                        return g;
                    }
                }

                return Match.Empty;
            }
        }

        protected override void WritePath(SearchContext context, FileSystemFinderResult result, string baseDirectoryPath, string indent, ColumnWidths columnWidths)
        {
            WritePath(context, result, baseDirectoryPath, indent, columnWidths, Colors.Match_Path);
        }
    }
}
