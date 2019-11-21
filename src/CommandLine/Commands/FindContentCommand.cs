// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Orang.FileSystem;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class FindContentCommand : CommonFindContentCommand<FindCommandOptions>
    {
        private AskMode _askMode;
        private IResultStorage _storage;
        private OutputSymbols _symbols;

        public FindContentCommand(FindCommandOptions options) : base(options)
        {
        }

        private OutputSymbols Symbols => _symbols ?? (_symbols = OutputSymbols.Create(Options.HighlightOptions));

        protected override void ExecuteCore(SearchContext context)
        {
            context.Telemetry.MatchingLineCount = -1;

            if (ConsoleOut.Verbosity >= Verbosity.Minimal)
                _askMode = Options.AskMode;

            if (Options.OutputPath != null
                && Options.Output.IncludeContent)
            {
                _storage = new TextWriterResultStorage(context.Output);
            }

            base.ExecuteCore(context);
        }

        protected override void ExecuteFile(string filePath, SearchContext context)
        {
            context.Telemetry.FileCount++;

            FileSystemFinderResult? maybeResult = MatchFile(filePath);

            if (maybeResult != null)
                ProcessResult(maybeResult.Value, context, FileWriterOptions);
        }

        protected override void ExecuteDirectory(string directoryPath, SearchContext context)
        {
            foreach (FileSystemFinderResult result in Find(directoryPath, context))
            {
                ProcessResult(result, context, DirectoryWriterOptions, directoryPath);

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

            Match match = Options.ContentFilter.Match(input, context.CancellationToken);

            if (match == null)
                return;

            ExecuteOrAddResult(result, context, writerOptions, match, input, default(Encoding), baseDirectoryPath);
        }

        protected override void ExecuteResult(
            FileSystemFinderResult result,
            SearchContext context,
            ContentWriterOptions writerOptions,
            Match match,
            string input,
            Encoding encoding,
            string baseDirectoryPath = null,
            ColumnWidths columnWidths = null)
        {
            string indent = (baseDirectoryPath != null && Options.PathDisplayStyle == PathDisplayStyle.Relative)
                ? Options.Indent
                : "";

            if (!Options.OmitPath)
            {
                WritePath(
                    result,
                    baseDirectoryPath,
                    relativePath: baseDirectoryPath != null && Options.PathDisplayStyle == PathDisplayStyle.Relative,
                    colors: Colors.Matched_Path,
                    matchColors: (Options.HighlightMatch) ? Colors.Match_Path : default,
                    indent: indent,
                    fileProperties: Options.Format.FileProperties,
                    columnWidths: columnWidths,
                    verbosity: Verbosity.Minimal);
            }

            context.Output?.WriteLineIf(Options.Output.IncludePath, result.Path);

            if (Options.ContentFilter.IsNegative)
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

                GetGroups(match, writerOptions.GroupNumber, context, isPathWritten: !Options.OmitPath, predicate: Options.ContentFilter.Predicate, groups: groups);

                if (_storage != null
                    || Options.AskMode == AskMode.Value
                    || ShouldLog(Verbosity.Normal))
                {
                    MatchOutputInfo outputInfo = Options.CreateOutputInfo(input, match);

                    contentWriter = ContentWriter.CreateFind(Options.ContentDisplayStyle, input, writerOptions, _storage, outputInfo, ask: _askMode == AskMode.Value);
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

        protected override void WriteSummary(SearchTelemetry telemetry, Verbosity verbosity)
        {
            WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget, verbosity);

            if (!ShouldLog(verbosity))
                return;

            WriteLine(verbosity);
            WriteCount("Matches", telemetry.MatchCount, Colors.Message_OK, verbosity);
            Write("  ", Colors.Message_OK, verbosity);

            if (telemetry.MatchingLineCount > 0)
            {
                WriteCount("Matching lines", telemetry.MatchingLineCount, Colors.Message_OK, verbosity);
                Write("  ", Colors.Message_OK, verbosity);
            }

            if (telemetry.MatchingFileCount > 0)
                WriteCount("Matching files", telemetry.MatchingFileCount, Colors.Message_OK, verbosity);

            WriteLine(verbosity);
        }

        protected override ContentWriterOptions CreateMatchWriteOptions(string indent)
        {
            int groupNumber = Options.ContentFilter.GroupNumber;

            GroupDefinition? groupDefinition = (groupNumber >= 0)
                ? new GroupDefinition(groupNumber, Options.ContentFilter.GroupName)
                : default(GroupDefinition?);

            return new ContentWriterOptions(
                format: Options.Format,
                groupDefinition,
                symbols: Symbols,
                highlightOptions: Options.HighlightOptions,
                indent: indent);
        }
    }
}
