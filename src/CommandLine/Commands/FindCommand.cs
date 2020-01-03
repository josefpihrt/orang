// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Orang.FileSystem;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class FindCommand<TOptions> : CommonFindCommand<TOptions> where TOptions : FindCommandOptions
    {
        private AskMode _askMode;
        private OutputSymbols _symbols;
        private ContentWriterOptions _fileWriterOptions;
        private ContentWriterOptions _directoryWriterOptions;

        public FindCommand(TOptions options) : base(options)
        {
        }

        public Filter ContentFilter => Options.ContentFilter;

        private OutputSymbols Symbols => _symbols ?? (_symbols = OutputSymbols.Create(Options.HighlightOptions));

        public virtual bool CanWriteContent => true;

        public override bool CanEndProgress
        {
            get
            {
                return !Options.OmitPath
                    || (ContentFilter != null && ConsoleOut.Verbosity > Verbosity.Minimal);
            }
        }

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

        protected virtual ContentWriterOptions CreateContentWriterOptions(string indent)
        {
            int groupNumber = ContentFilter.GroupNumber;

            GroupDefinition? groupDefinition = (groupNumber >= 0)
                ? new GroupDefinition(groupNumber, ContentFilter.GroupName)
                : default(GroupDefinition?);

            return new ContentWriterOptions(
                format: Options.Format,
                groupDefinition,
                symbols: Symbols,
                highlightOptions: Options.HighlightOptions,
                indent: indent);
        }

        protected override void ExecuteCore(SearchContext context)
        {
            context.Telemetry.MatchingLineCount = -1;

            if (ConsoleOut.Verbosity >= Verbosity.Minimal)
                _askMode = Options.AskMode;

            base.ExecuteCore(context);
        }

        protected override void ExecuteFile(string filePath, SearchContext context)
        {
            context.Telemetry.FileCount++;

            FileSystemFinderResult? maybeResult = MatchFile(filePath);

            if (maybeResult != null)
            {
                if (ContentFilter != null)
                {
                    ProcessResult(maybeResult.Value, context, FileWriterOptions);
                }
                else
                {
                    ExecuteOrAddResult(maybeResult.Value, context, null);
                }
            }
        }

        protected override void ExecuteDirectory(string directoryPath, SearchContext context)
        {
            foreach (FileSystemFinderResult result in Find(directoryPath, context))
            {
                if (ContentFilter != null)
                {
                    ProcessResult(result, context, DirectoryWriterOptions, directoryPath);
                }
                else
                {
                    ExecuteOrAddResult(result, context, directoryPath);
                }

                if (context.State == SearchState.Canceled)
                    break;

                if (context.State == SearchState.MaxReached)
                    break;
            }
        }

        private void ProcessResult(
            FileSystemFinderResult result,
            SearchContext context,
            ContentWriterOptions writerOptions,
            string baseDirectoryPath = null)
        {
            string input = ReadFile(result.Path, baseDirectoryPath, Options.DefaultEncoding, context);

            if (input == null)
                return;

            Match match = ContentFilter.Match(input, context.CancellationToken);

            if (match == null)
                return;

            ExecuteOrAddResult(result, context, writerOptions, match, input, default(Encoding), baseDirectoryPath);
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
                var searchResult = new ContentSearchResult(result, baseDirectoryPath, match, input, encoding, writerOptions);

                context.Results.Add(searchResult);
            }
            else
            {
                EndProgress(context);

                ExecuteResult(result, context, writerOptions, match, input, encoding, baseDirectoryPath);
            }
        }

        protected virtual void ExecuteResult(
            FileSystemFinderResult result,
            SearchContext context,
            ContentWriterOptions writerOptions,
            Match match,
            string input,
            Encoding encoding,
            string baseDirectoryPath = null,
            ColumnWidths columnWidths = null)
        {
            string indent = GetPathIndent(baseDirectoryPath);

            if (!Options.OmitPath)
                WritePath(context, result, baseDirectoryPath, indent, columnWidths);

            if (ContentFilter.IsNegative
                || !CanWriteContent)
            {
                WriteLineIf(!Options.OmitPath, Verbosity.Minimal);
            }
            else
            {
                WriteMatches(input, match, writerOptions, context);
            }

            if (_askMode == AskMode.File)
            {
                try
                {
                    if (ConsoleHelpers.Question("Continue without asking?", indent))
                        _askMode = AskMode.None;
                }
                catch (OperationCanceledException)
                {
                    context.State = SearchState.Canceled;
                }
            }
        }

        protected override void ExecuteResult(SearchResult result, SearchContext context, ColumnWidths columnWidths)
        {
            if (ContentFilter != null)
            {
                ExecuteResult((ContentSearchResult)result, context, columnWidths);
            }
            else
            {
                ExecuteResult(result.Result, context, result.BaseDirectoryPath, columnWidths);
            }
        }

        private void ExecuteResult(ContentSearchResult result, SearchContext context, ColumnWidths columnWidths)
        {
            ExecuteResult(result.Result, context, result.WriterOptions, result.Match, result.Input, result.Encoding, result.BaseDirectoryPath, columnWidths);
        }

        protected override void ExecuteResult(FileSystemFinderResult result, SearchContext context, string baseDirectoryPath = null, ColumnWidths columnWidths = null)
        {
            string indent = GetPathIndent(baseDirectoryPath);

            if (!Options.OmitPath)
                WritePath(context, result, baseDirectoryPath, indent, columnWidths);

            if (_askMode == AskMode.File)
            {
                try
                {
                    if (ConsoleHelpers.Question("Continue without asking?", indent))
                        _askMode = AskMode.None;
                }
                catch (OperationCanceledException)
                {
                    context.State = SearchState.Canceled;
                }
            }
        }

        private void WriteMatches(
            string input,
            Match match,
            ContentWriterOptions writerOptions,
            SearchContext context)
        {
            SearchTelemetry telemetry = context.Telemetry;

            ContentWriter contentWriter = null;
            List<Group> groups = null;

            try
            {
                groups = ListCache<Group>.GetInstance();

                GetGroups(match, writerOptions.GroupNumber, context, isPathWritten: !Options.OmitPath, predicate: ContentFilter.Predicate, groups: groups);

                if (Options.AskMode == AskMode.Value
                    || ShouldLog(Verbosity.Normal))
                {
                    MatchOutputInfo outputInfo = Options.CreateOutputInfo(input, match);

                    contentWriter = ContentWriter.CreateFind(Options.ContentDisplayStyle, input, writerOptions, default(IResultStorage), outputInfo, ask: _askMode == AskMode.Value);
                }
                else
                {
                    contentWriter = new EmptyContentWriter(null, writerOptions);
                }

                WriteMatches(contentWriter, groups, context);

                telemetry.MatchCount += contentWriter.MatchCount;

                if (contentWriter.MatchingLineCount >= 0)
                {
                    if (telemetry.MatchingLineCount == -1)
                        telemetry.MatchingLineCount = 0;

                    telemetry.MatchingLineCount += contentWriter.MatchingLineCount;
                }

                if (_askMode == AskMode.Value)
                {
                    if (contentWriter is AskValueContentWriter askValueContentWriter)
                    {
                        if (!askValueContentWriter.Ask)
                            _askMode = AskMode.None;
                    }
                    else if (contentWriter is AskLineContentWriter askLineContentWriter)
                    {
                        if (!askLineContentWriter.Ask)
                            _askMode = AskMode.None;
                    }
                }
            }
            finally
            {
                contentWriter?.Dispose();

                if (groups != null)
                    ListCache<Group>.Free(groups);
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
            if (ContentFilter != null)
            {
                WritePath(context, result, baseDirectoryPath, indent, columnWidths, Colors.Match_Path);
            }
            else
            {
                base.WritePath(context, result, baseDirectoryPath, indent, columnWidths);
            }
        }

        protected override void WriteSummary(SearchTelemetry telemetry, Verbosity verbosity)
        {
            WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget, verbosity);

            if (!ShouldLog(verbosity))
                return;

            WriteLine(verbosity);

            if (ContentFilter != null)
            {
                WriteCount("Matches", telemetry.MatchCount, Colors.Message_OK, verbosity);
                Write("  ", Colors.Message_OK, verbosity);

                if (telemetry.MatchingLineCount > 0)
                {
                    WriteCount("Matching lines", telemetry.MatchingLineCount, Colors.Message_OK, verbosity);
                    Write("  ", Colors.Message_OK, verbosity);
                }

                if (telemetry.MatchingFileCount > 0)
                    WriteCount("Matching files", telemetry.MatchingFileCount, Colors.Message_OK, verbosity);
            }
            else
            {
                if (Options.SearchTarget != SearchTarget.Directories)
                    WriteCount("Matching files", telemetry.MatchingFileCount, Colors.Message_OK, verbosity);

                if (Options.SearchTarget != SearchTarget.Files)
                {
                    if (Options.SearchTarget != SearchTarget.Directories)
                        Write("  ", Colors.Message_OK, verbosity);

                    WriteCount("Matching directories", telemetry.MatchingDirectoryCount, Colors.Message_OK, verbosity);
                }
            }

            WriteLine(verbosity);
        }
    }
}
