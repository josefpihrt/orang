// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Orang.FileSystem;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

#pragma warning disable IDE0068

namespace Orang.CommandLine
{
    internal class ReplaceCommand : CommonFindContentCommand<ReplaceCommandOptions>
    {
        private OutputSymbols _symbols;
        private MatchEvaluator _matchEvaluator;

        public ReplaceCommand(ReplaceCommandOptions options) : base(options)
        {
        }

        private OutputSymbols Symbols => _symbols ?? (_symbols = OutputSymbols.Create(Options.HighlightOptions));

        private MatchEvaluator MatchEvaluator => _matchEvaluator ?? (_matchEvaluator = Options.MatchEvaluator ?? new MatchEvaluator(f => f.Result(Options.Replacement)));

        protected override void ExecuteCore(SearchContext context)
        {
            context.Telemetry.MatchingLineCount = -1;

            if (Options.Input != null)
            {
                ExecuteInput(context);
            }
            else
            {
                base.ExecuteCore(context);
            }
        }

        private void ExecuteInput(SearchContext context)
        {
            string input = Options.Input;
            int count = 0;
            var maxReason = MaxReason.None;
            Match match = Options.ContentFilter.Regex.Match(input);

            if (match.Success)
            {
                ContentWriter contentWriter = null;
                List<Group> groups = null;

                try
                {
                    groups = ListCache<Group>.GetInstance();

                    maxReason = GetGroups(match, FileWriterOptions.GroupNumber, context, isPathWritten: false, groups);

                    if (ShouldLog(Verbosity.Normal))
                    {
                        MatchOutputInfo outputInfo = Options.CreateOutputInfo(input, match);

                        contentWriter = ContentWriter.CreateReplace(Options.ContentDisplayStyle, input, MatchEvaluator, FileWriterOptions, outputInfo: outputInfo);
                    }
                    else
                    {
                        contentWriter = new EmptyContentWriter(null, FileWriterOptions);
                    }

                    WriteMatches(contentWriter, groups, context);
                    count = contentWriter.MatchCount;
                }
                finally
                {
                    contentWriter?.Dispose();

                    if (groups != null)
                        ListCache<Group>.Free(groups);
                }
            }

            WriteLine();

            if (ShouldLog(Verbosity.Detailed)
                || Options.IncludeSummary == true)
            {
                Verbosity verbosity = (Options.IncludeSummary == true) ? Verbosity.Minimal : Verbosity.Detailed;

                WriteCount("Replacements", count, Colors.Message_OK, verbosity);
                WriteIf(maxReason == MaxReason.CountExceedsMax, "+", Colors.Message_OK, verbosity);
                WriteLine(verbosity);
            }
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
            string indent = (baseDirectoryPath != null && Options.PathDisplayStyle == PathDisplayStyle.Relative)
                ? Options.Indent
                : "";

            Encoding encoding = Options.DefaultEncoding;

            string input = ReadFile(result.Path, baseDirectoryPath, ref encoding, context, indent);

            if (input == null)
                return;

            Match match = Options.ContentFilter.Regex.Match(input);

            if (!match.Success)
                return;

            ExecuteOrAddResult(result, context, writerOptions, match, input, encoding, baseDirectoryPath);
        }

        protected override void ExecuteResult(
            FileSystemFinderResult result,
            SearchContext context,
            ContentWriterOptions writerOptions,
            Match match,
            string input,
            Encoding encoding,
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
                    matchColors: (Options.HighlightMatch) ? Colors.Match_Path : default,
                    indent: indent,
                    verbosity: Verbosity.Minimal);
            }

            ReplaceMatches(result.Path, encoding, input, match, indent, writerOptions, context);
        }

        private string ReadFile(
            string filePath,
            string basePath,
            ref Encoding encoding,
            SearchContext context,
            string indent = null)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    Encoding encodingFromBom = EncodingHelpers.DetectEncoding(stream);

                    if (encodingFromBom != null)
                        encoding = encodingFromBom;

                    stream.Position = 0;

                    using (var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex) when (ex is IOException
                || ex is UnauthorizedAccessException)
            {
                EndProgress(context);

                WriteFileError(
                    ex,
                    filePath,
                    basePath,
                    relativePath: Options.PathDisplayStyle == PathDisplayStyle.Relative,
                    indent: indent);

                return null;
            }
        }

        private void ReplaceMatches(
            string filePath,
            Encoding encoding,
            string input,
            Match match,
            string indent,
            ContentWriterOptions writerOptions,
            SearchContext context)
        {
            SearchTelemetry telemetry = context.Telemetry;

            ContentWriter contentWriter = null;
            TextWriter textWriter = null;
            List<Group> groups = null;

            try
            {
                groups = ListCache<Group>.GetInstance();

                GetGroups(match, writerOptions.GroupNumber, context, isPathWritten: !Options.OmitPath, groups);

                int fileMatchCount = 0;
                int fileReplacementCount = 0;

                if (!Options.DryRun)
                {
                    if (Options.AskMode == AskMode.File)
                    {
                        textWriter = new StringWriter();
                    }
                    else if (Options.AskMode == AskMode.None)
                    {
                        textWriter = new StreamWriter(filePath, false, encoding);
                    }
                }

                if (Options.AskMode == AskMode.Value
                    || ShouldLog(Verbosity.Normal))
                {
                    MatchOutputInfo outputInfo = Options.CreateOutputInfo(input, match);

                    if (Options.AskMode == AskMode.Value)
                    {
                        Lazy<TextWriter> lazyWriter = (Options.DryRun)
                            ? null
                            : new Lazy<TextWriter>(() => new StreamWriter(filePath, false, encoding));

                        contentWriter = AskReplacementWriter.Create(Options.ContentDisplayStyle, input, MatchEvaluator, lazyWriter, writerOptions, outputInfo);
                    }
                    else
                    {
                        contentWriter = ContentWriter.CreateReplace(Options.ContentDisplayStyle, input, MatchEvaluator, writerOptions, textWriter, outputInfo);
                    }
                }
                else if (Options.DryRun)
                {
                    contentWriter = new EmptyContentWriter(null, FileWriterOptions);
                }
                else
                {
                    contentWriter = new TextWriterContentWriter(input, MatchEvaluator, textWriter, writerOptions);
                }

                WriteMatches(contentWriter, groups, context);

                fileMatchCount = contentWriter.MatchCount;
                fileReplacementCount = (contentWriter is AskReplacementWriter askReplacementWriter) ? askReplacementWriter.ReplacementCount : fileMatchCount;
                telemetry.MatchCount += fileMatchCount;

                if (contentWriter.MatchingLineCount >= 0)
                {
                    if (telemetry.MatchingLineCount == -1)
                        telemetry.MatchingLineCount = 0;

                    telemetry.MatchingLineCount += contentWriter.MatchingLineCount;
                }

                if (Options.AskMode == AskMode.File)
                {
                    if (context.State == SearchState.Canceled)
                    {
                        fileReplacementCount = 0;
                    }
                    else
                    {
                        try
                        {
                            if (Options.DryRun)
                            {
                                if (ConsoleHelpers.Question("Continue without asking?", indent))
                                    Options.AskMode = AskMode.None;
                            }
                            else if (ConsoleHelpers.Question("Replace content?", indent))
                            {
                                File.WriteAllText(filePath, textWriter.ToString(), encoding);
                            }
                            else
                            {
                                fileReplacementCount = 0;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            context.State = SearchState.Canceled;
                            fileReplacementCount = 0;
                        }
                    }
                }
                else if (Options.AskMode == AskMode.Value)
                {
                    if (((AskReplacementWriter)contentWriter).ContinueWithoutAsking)
                        Options.AskMode = AskMode.None;
                }

                if (!Options.DryRun)
                {
                    telemetry.ProcessedMatchCount += fileReplacementCount;

                    if (fileReplacementCount > 0)
                        telemetry.ProcessedFileCount++;
                }
            }
            catch (Exception ex) when (ex is IOException
                || ex is UnauthorizedAccessException)
            {
                WriteFileError(ex, indent: indent);
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

            string matchCount = telemetry.MatchCount.ToString("n0");
            string matchingFileCount = telemetry.MatchingFileCount.ToString("n0");
            string processedMatchCount = telemetry.ProcessedMatchCount.ToString("n0");
            string processedFileCount = telemetry.ProcessedFileCount.ToString("n0");

            const string matchesTitle = "Matches";
            const string matchingFilesTitle = "Matching files";
            const string replacementsTitle = "Replacements";
            const string replacedFilesTitle = "Replaced files";

            int width1 = Math.Max(matchesTitle.Length, replacementsTitle.Length);
            int width2 = Math.Max(matchCount.Length, processedMatchCount.Length);
            int width3 = Math.Max(matchingFilesTitle.Length, replacedFilesTitle.Length);
            int width4 = Math.Max(matchingFileCount.Length, processedFileCount.Length);

            WriteCount(matchesTitle, matchCount, width1, width2, Colors.Message_OK, verbosity);
            Write("  ", Colors.Message_OK, verbosity);

            int matchingLinesWidth = 0;
            if (telemetry.MatchingLineCount >= 0)
            {
                matchingLinesWidth += WriteCount("Matching lines", telemetry.MatchingLineCount, Colors.Message_OK, verbosity);
                Write("  ", Colors.Message_OK, verbosity);
                matchingLinesWidth += 2;
            }

            WriteCount(matchingFilesTitle, matchingFileCount, width3, width4, Colors.Message_OK, verbosity);
            WriteLine(verbosity);

            WriteCount(replacementsTitle, processedMatchCount, width1, width2, Colors.Message_Change, verbosity);
            Write("  ", Colors.Message_Change, verbosity);
            Write(' ', matchingLinesWidth, Colors.Message_Change, verbosity);
            WriteCount(replacedFilesTitle, processedFileCount, width3, width4, Colors.Message_Change, verbosity);

            WriteLine(verbosity);
        }

        protected override ContentWriterOptions CreateMatchWriteOptions(string indent)
        {
            return new ContentWriterOptions(
                format: Options.Format,
                symbols: Symbols,
                highlightOptions: Options.HighlightOptions,
                indent: indent);
        }
    }
}
