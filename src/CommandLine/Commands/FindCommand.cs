// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private IResultStorage _storage;
        private List<int> _storageIndexes;
        private IResultStorage _fileStorage;
        private List<string> _fileValues;

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

            bool aggregate = (Options.ModifyOptions.Functions & ModifyFunctions.ExceptIntersect) != 0
                || (Options.ModifyOptions.Aggregate
                    && (Options.ModifyOptions.Method != null
                        || (Options.ModifyOptions.Functions & ModifyFunctions.Enumerable) != 0));

            if (aggregate)
            {
                _storage = new ListResultStorage();

                if ((Options.ModifyOptions.Functions & ModifyFunctions.ExceptIntersect) != 0)
                    _storageIndexes = new List<int>();
            }

            base.ExecuteCore(context);

            if (aggregate)
                WriteAggregatedValues();
        }

        protected override void ExecuteFile(string filePath, SearchContext context)
        {
            context.Telemetry.FileCount++;

            FileSystemFinderResult result = MatchFile(filePath, context.Progress);

            if (result != null)
            {
                if (ContentFilter != null)
                {
                    ProcessResult(result, context, FileWriterOptions);
                }
                else
                {
                    ExecuteOrAddResult(result, context, null);
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

                if (context.TerminationReason == TerminationReason.Canceled)
                    break;

                if (context.TerminationReason == TerminationReason.MaxReached)
                    break;
            }

            if (ContentFilter == null)
                _storageIndexes?.Add(_storage.Count);
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
                context.TerminationReason = TerminationReason.MaxReached;

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

            AskToContinue(context, indent);
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

            if (_storage != null
                && result.Match != null
                && !object.ReferenceEquals(result.Match, Match.Empty))
            {
                _storage.Add(result.Match.Value);
            }

            AskToContinue(context, indent);
        }

        protected void AskToContinue(SearchContext context, string indent)
        {
            if (_askMode == AskMode.File)
            {
                try
                {
                    if (ConsoleHelpers.AskToContinue(indent))
                        _askMode = AskMode.None;
                }
                catch (OperationCanceledException)
                {
                    context.TerminationReason = TerminationReason.Canceled;
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
            List<Capture> captures = null;

            try
            {
                captures = ListCache<Capture>.GetInstance();

                GetCaptures(match, writerOptions.GroupNumber, context, isPathWritten: !Options.OmitPath, predicate: ContentFilter.Predicate, captures: captures);

                bool hasAnyFunction = Options.ModifyOptions.HasAnyFunction;

                if (hasAnyFunction
                    || Options.AskMode == AskMode.Value
                    || ShouldLog(Verbosity.Normal))
                {
                    if (hasAnyFunction)
                    {
                        if (_fileValues == null)
                        {
                            _fileValues = new List<string>();
                        }
                        else
                        {
                            _fileValues.Clear();
                        }

                        if (_fileStorage == null)
                            _fileStorage = new ListResultStorage(_fileValues);
                    }

                    contentWriter = ContentWriter.CreateFind(
                        contentDisplayStyle: Options.ContentDisplayStyle,
                        input: input,
                        options: writerOptions,
                        storage: (hasAnyFunction) ? _fileStorage : _storage,
                        outputInfo: Options.CreateOutputInfo(input, match),
                        writer: (hasAnyFunction) ? null : ContentTextWriter.Default,
                        ask: _askMode == AskMode.Value);
                }
                else
                {
                    contentWriter = new EmptyContentWriter(null, writerOptions);
                }

                WriteMatches(contentWriter, captures, context);

                if (hasAnyFunction)
                {
                    ConsoleColors colors = (Options.HighlightMatch) ? Colors.Match : default;
                    ConsoleColors boundaryColors = (Options.HighlightBoundary) ? Colors.MatchBoundary : default;

                    var valueWriter = new ValueWriter(ContentTextWriter.Default, writerOptions.Indent, includeEndingIndent: false);

                    foreach (string value in _fileValues.Modify(Options.ModifyOptions))
                    {
                        Write(writerOptions.Indent, Verbosity.Normal);
                        valueWriter.Write(value, Symbols, colors, boundaryColors);
                        WriteLine(Verbosity.Normal);

                        _storage?.Add(value);
                        telemetry.MatchCount++;
                    }

                    _storageIndexes?.Add(_storage!.Count);
                }
                else
                {
                    telemetry.MatchCount += contentWriter.MatchCount;

                    if (contentWriter.MatchingLineCount >= 0)
                    {
                        if (telemetry.MatchingLineCount == -1)
                            telemetry.MatchingLineCount = 0;

                        telemetry.MatchingLineCount += contentWriter.MatchingLineCount;
                    }
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

                if (captures != null)
                    ListCache<Capture>.Free(captures);
            }
        }

        protected void WriteMatches(ContentWriter writer, IEnumerable<Capture> captures, SearchContext context)
        {
            try
            {
                writer.WriteMatches(captures, context.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                context.TerminationReason = TerminationReason.Canceled;
            }
        }

        protected MaxReason GetCaptures(
            Match match,
            int groupNumber,
            SearchContext context,
            bool isPathWritten,
            Func<Capture, bool> predicate,
            List<Capture> captures)
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
                maxReason = GetMatches(captures, match, count, predicate, context.CancellationToken);
            }
            else
            {
                maxReason = GetCaptures(captures, match, groupNumber, count, predicate, context.CancellationToken);
            }

            if (captures.Count > 1
                && captures[0].Index > captures[1].Index)
            {
                captures.Reverse();
            }

            if ((maxReason == MaxReason.CountEqualsMax || maxReason == MaxReason.CountExceedsMax)
                && maxMatches > 0
                && (maxMatchesInFile == 0 || maxMatches <= maxMatchesInFile))
            {
                context.TerminationReason = TerminationReason.MaxReached;
            }

            if (isPathWritten)
            {
                Verbosity verbosity = ConsoleOut.Verbosity;

                if (verbosity >= Verbosity.Detailed
                    || Options.IncludeCount)
                {
                    verbosity = (Options.IncludeCount) ? Verbosity.Minimal : Verbosity.Detailed;

                    ConsoleOut.Write("  ", Colors.Message_OK, verbosity);
                    ConsoleOut.Write(captures.Count.ToString("n0"), Colors.Message_OK, verbosity);
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
                        Out.Write(captures.Count.ToString("n0"), verbosity);
                        Out.WriteIf(maxReason == MaxReason.CountExceedsMax, "+", verbosity);
                    }

                    Out.WriteLine(Verbosity.Minimal);
                }
            }

            return maxReason;
        }

        private MaxReason GetMatches(
            List<Capture> matches,
            Match match,
            int count,
            Func<Group, bool> predicate,
            CancellationToken cancellationToken)
        {
            do
            {
                matches.Add(match);

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

                if (matches.Count == count)
                {
                    return (match.Success)
                        ? MaxReason.CountExceedsMax
                        : MaxReason.CountEqualsMax;
                }

                cancellationToken.ThrowIfCancellationRequested();

            } while (match.Success);

            return MaxReason.None;
        }

        private MaxReason GetCaptures(
            List<Capture> captures,
            Match match,
            int groupNumber,
            int count,
            Func<Capture, bool> predicate,
            CancellationToken cancellationToken)
        {
            Group group = match.Groups[groupNumber];

            do
            {
                Capture prevCapture = null;

                foreach (Capture capture in group.Captures)
                {
                    if (prevCapture != null
                        && prevCapture.EndIndex() <= capture.EndIndex()
                        && prevCapture.Index >= capture.Index)
                    {
                        captures[captures.Count - 1] = capture;
                    }
                    else
                    {
                        captures.Add(capture);
                    }

                    if (captures.Count == count)
                        return (group.Success) ? MaxReason.CountExceedsMax : MaxReason.CountEqualsMax;

                    cancellationToken.ThrowIfCancellationRequested();

                    prevCapture = capture;
                }

                group = NextGroup();

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

        private void WriteAggregatedValues()
        {
            int count = 0;
            ModifyOptions modifyOptions = Options.ModifyOptions;
            List<string> allValues = ((ListResultStorage)_storage).Values;

            if (_storageIndexes?.Count > 1)
            {
                Debug.Assert((modifyOptions.Functions & ModifyFunctions.ExceptIntersect) != 0, modifyOptions.Functions.ToString());

                if ((modifyOptions.Functions & ModifyFunctions.Except) != 0)
                {
                    if (_storageIndexes.Count > 2)
                        throw new InvalidOperationException($"'Except' operation cannot be applied on more than two {((ContentFilter != null) ? "files" : "directories")}.");

                    int index = _storageIndexes[0];

                    allValues = allValues
                        .Take(index)
                        .Except(GetRange(index, allValues.Count))
                        .ToList();
                }
                else if ((modifyOptions.Functions & ModifyFunctions.Intersect) != 0)
                {
                    allValues = Intersect();
                }
            }

            using (IEnumerator<string> en = allValues
                .Modify(modifyOptions, filter: ModifyFunctions.Enumerable)
                .GetEnumerator())
            {
                if (en.MoveNext())
                {
                    OutputSymbols symbols = OutputSymbols.Create(Options.HighlightOptions);
                    ConsoleColors colors = ((Options.HighlightOptions & HighlightOptions.Match) != 0) ? Colors.Match : default;
                    ConsoleColors boundaryColors = (Options.HighlightBoundary) ? Colors.MatchBoundary : default;
                    var valueWriter = new ValueWriter(new ContentTextWriter(Verbosity.Minimal), includeEndingIndent: false);

                    ConsoleOut.WriteLineIf(ShouldWriteLine(ConsoleOut.Verbosity));
                    Out?.WriteLineIf(ShouldWriteLine(Out.Verbosity));

                    do
                    {
                        valueWriter.Write(en.Current, symbols, colors, boundaryColors);
                        WriteLine(Verbosity.Minimal);
                        count++;

                    } while (en.MoveNext());

                    if (ShouldLog(Verbosity.Detailed)
                        || Options.IncludeSummary)
                    {
                        WriteLine(Verbosity.Minimal);
                        WriteCount("Values", count, verbosity: Verbosity.Minimal);
                        WriteLine(Verbosity.Minimal);
                    }
                }
            }

            List<string> Intersect()
            {
                var list = new List<string>(GetRange(0, _storageIndexes[0]));

                for (int i = 1; i < _storageIndexes.Count; i++)
                {
                    IEnumerable<string> second = GetRange(_storageIndexes[i - 1], _storageIndexes[i]);

                    list = list.Intersect(second, modifyOptions.StringComparer).ToList();
                }

                return list;
            }

            IEnumerable<string> GetRange(int start, int end)
            {
                for (int i = start; i < end; i++)
                    yield return allValues[i];
            }

            bool ShouldWriteLine(Verbosity verbosity)
            {
                if (verbosity > Verbosity.Minimal)
                    return true;

                return verbosity == Verbosity.Minimal
                    && (Options.PathDisplayStyle != PathDisplayStyle.Omit || Options.IncludeSummary);
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
