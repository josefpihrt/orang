// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using Orang.FileSystem;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class FindContentCommand : CommonFindContentCommand<FindCommandOptions>
    {
        private AskMode _askMode;
        private OutputSymbols _symbols;
        private MatchWriterOptions _fileWriterOptions;
        private MatchWriterOptions _directoryWriterOptions;

        public FindContentCommand(FindCommandOptions options) : base(options)
        {
        }

        protected override bool CanExecuteFile => true;

        private OutputSymbols Symbols => _symbols ?? (_symbols = OutputSymbols.Create(Options.HighlightOptions));

        private MatchWriterOptions FileWriterOptions => _fileWriterOptions ?? (_fileWriterOptions = CreateMatchWriteOptions(Options.Indent));

        private MatchWriterOptions DirectoryWriterOptions => _directoryWriterOptions ?? (_directoryWriterOptions = CreateMatchWriteOptions(Options.DoubleIndent));

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

            string input = ReadFile(filePath, null, Options.DefaultEncoding);

            if (input != null)
            {
                Match match = Options.ContentFilter.Regex.Match(input);

                if (Options.ContentFilter.IsMatch(match))
                {
                    WritePath(filePath, colors: Colors.Matched_Path, verbosity: Verbosity.Minimal);
                    WriteMatches(input, match, FileWriterOptions, context);

                    if (Options.MaxMatchingFiles == context.Telemetry.MatchingFileCount)
                        context.State = SearchState.MaxReached;
                }
            }
        }

        protected override void ExecuteDirectory(string directoryPath, SearchContext context, FileSystemFinderProgressReporter progress)
        {
            Regex regex = Options.ContentFilter.Regex;

            string basePath = (Options.Format.Includes(MiscellaneousDisplayOptions.IncludeFullPath)) ? null : directoryPath;

            foreach (FileSystemFinderResult result in FileSystemHelpers.Find(directoryPath, Options, progress, context.CancellationToken))
            {
                string input = ReadFile(result.Path, directoryPath, Options.DefaultEncoding, progress, Options.Indent);

                if (input == null)
                    continue;

                Match match = regex.Match(input);

                if (Options.ContentFilter.IsMatch(match))
                {
                    EndProgress(progress);
                    WritePath(result, basePath, colors: Colors.Matched_Path, matchColors: (Options.HighlightMatch) ? Colors.Match_Path : default, indent: Options.Indent, verbosity: Verbosity.Minimal);

                    if (Options.ContentFilter.IsNegative)
                    {
                        WriteLine(Verbosity.Minimal);
                    }
                    else
                    {
                        WriteMatches(input, match, DirectoryWriterOptions, context);
                    }

                    if (Options.MaxMatchingFiles == context.Telemetry.MatchingFileCount)
                        context.State = SearchState.MaxReached;

                    if (context.State == SearchState.MaxReached)
                        break;

                    if (_askMode == AskMode.File
                        && ConsoleHelpers.Question("Continue without asking?", Options.Indent))
                    {
                        _askMode = AskMode.None;
                    }
                }
            }

            context.Telemetry.SearchedDirectoryCount = progress.SearchedDirectoryCount;
            context.Telemetry.DirectoryCount = progress.DirectoryCount;
            context.Telemetry.FileCount = progress.FileCount;
        }

        private void WriteMatches(
            string input,
            Match match,
            MatchWriterOptions writerOptions,
            SearchContext context)
        {
            SearchTelemetry telemetry = context.Telemetry;

            telemetry.MatchingFileCount++;

            if (ShouldLog(Verbosity.Normal))
            {
                WriteLine(Verbosity.Minimal);

                MatchOutputInfo outputInfo = Options.CreateOutputInfo(input, match);

                using (MatchWriter matchWriter = MatchWriter.CreateFind(Options.ContentDisplayStyle, input, writerOptions, values: null, outputInfo, ask: _askMode == AskMode.Value))
                {
                    WriteMatches(matchWriter, match, context);
                    telemetry.MatchCount += matchWriter.MatchCount;

                    if (matchWriter.MatchingLineCount >= 0)
                    {
                        if (telemetry.MatchingLineCount == -1)
                            telemetry.MatchingLineCount = 0;

                        telemetry.MatchingLineCount += matchWriter.MatchingLineCount;
                    }

                    if (_askMode == AskMode.Value)
                    {
                        if (matchWriter is AskValueMatchWriter askValueMatchWriter)
                        {
                            if (!askValueMatchWriter.Ask)
                                _askMode = AskMode.None;
                        }
                        else if (matchWriter is AskLineMatchWriter askLineMatchWriter)
                        {
                            if (!askLineMatchWriter.Ask)
                                _askMode = AskMode.None;
                        }
                    }
                }
            }
            else
            {
                int fileMatchCount = EnumerateValues();

                Write(" ", Colors.Message_OK, Verbosity.Minimal);
                WriteCount("", fileMatchCount, Colors.Message_OK, Verbosity.Minimal);
                WriteLine(Verbosity.Minimal);

                telemetry.MatchCount += fileMatchCount;
            }

            int EnumerateValues()
            {
                using (var matchWriter = new EmptyMatchWriter(null, writerOptions))
                {
                    WriteMatches(matchWriter, match, context);

                    if (matchWriter.MatchingLineCount >= 0)
                    {
                        if (telemetry.MatchingLineCount == -1)
                            telemetry.MatchingLineCount = 0;

                        telemetry.MatchingLineCount += matchWriter.MatchingLineCount;
                    }

                    return matchWriter.MatchCount;
                }
            }
        }

        protected override void WriteSummary(SearchTelemetry telemetry)
        {
            WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget);

            if (!ShouldLog(Verbosity.Minimal))
                return;

            WriteLine(Verbosity.Minimal);
            WriteCount("Matches", telemetry.MatchCount, Colors.Message_OK, Verbosity.Minimal);

            if (telemetry.MatchCount > 0)
            {
                Write("  ", Colors.Message_OK, Verbosity.Minimal);

                if (telemetry.MatchingLineCount > 0)
                {
                    WriteCount("Matching lines", telemetry.MatchingLineCount, Colors.Message_OK, Verbosity.Minimal);
                    Write("  ", Colors.Message_OK, Verbosity.Minimal);
                }

                WriteCount("Matching files", telemetry.MatchingFileCount, Colors.Message_OK, Verbosity.Minimal);
            }

            WriteLine(Verbosity.Minimal);
        }

        private MatchWriterOptions CreateMatchWriteOptions(string indent)
        {
            int groupNumber = Options.ContentFilter.GroupNumber;

            GroupDefinition? groupDefinition = (groupNumber >= 0)
                ? new GroupDefinition(groupNumber, Options.ContentFilter.GroupName)
                : default(GroupDefinition?);

            return new MatchWriterOptions(
                format: Options.Format,
                groupDefinition,
                symbols: Symbols,
                highlightOptions: Options.HighlightOptions,
                indent: indent);
        }
    }
}
