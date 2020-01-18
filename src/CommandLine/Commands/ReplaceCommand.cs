// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Orang.FileSystem;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

#pragma warning disable IDE0068

namespace Orang.CommandLine
{
    internal class ReplaceCommand : FindCommand<ReplaceCommandOptions>
    {
        private OutputSymbols _symbols;

        public ReplaceCommand(ReplaceCommandOptions options) : base(options)
        {
            Debug.Assert(!options.ContentFilter.IsNegative);
        }

        protected override bool CanDisplaySummary => Options.Input == null;

        private OutputSymbols Symbols => _symbols ?? (_symbols = OutputSymbols.Create(Options.HighlightOptions));

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
            Match match = Options.ContentFilter.Match(input, context.CancellationToken);

            if (match.Success)
            {
                ContentWriter contentWriter = null;
                List<Capture> groups = null;

                try
                {
                    groups = ListCache<Capture>.GetInstance();

                    maxReason = GetCaptures(match, FileWriterOptions.GroupNumber, context, isPathWritten: false, predicate: Options.ContentFilter.Predicate, captures: groups);

                    if (ShouldLog(Verbosity.Normal))
                    {
                        MatchOutputInfo outputInfo = Options.CreateOutputInfo(input, match);

                        contentWriter = ContentWriter.CreateReplace(Options.ContentDisplayStyle, input, Options.ReplaceOptions, FileWriterOptions, outputInfo: outputInfo);
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
                        ListCache<Capture>.Free(groups);
                }
            }

            if (ShouldLog(Verbosity.Detailed)
                || Options.IncludeSummary)
            {
                Verbosity verbosity = (Options.IncludeSummary) ? Verbosity.Minimal : Verbosity.Detailed;

                WriteLine(verbosity);
                WriteCount("Replacements", count, Colors.Message_OK, verbosity);
                WriteIf(maxReason == MaxReason.CountExceedsMax, "+", Colors.Message_OK, verbosity);
                WriteLine(verbosity);
            }
        }

        protected override void ExecuteFile(string filePath, SearchContext context)
        {
            context.Telemetry.FileCount++;

            FileSystemFinderResult? maybeResult = MatchFile(filePath, context.Progress);

            if (maybeResult != null)
                ProcessResult(maybeResult.Value, context, FileWriterOptions);
        }

        protected override void ExecuteDirectory(string directoryPath, SearchContext context)
        {
            foreach (FileSystemFinderResult result in Find(directoryPath, context))
            {
                ProcessResult(result, context, DirectoryWriterOptions, directoryPath);

                if (context.TerminationReason == TerminationReason.Canceled)
                    break;

                if (context.TerminationReason == TerminationReason.MaxReached)
                    break;
            }
        }

        private void ProcessResult(
            FileSystemFinderResult result,
            SearchContext context,
            ContentWriterOptions writerOptions,
            string baseDirectoryPath = null)
        {
            string indent = GetPathIndent(baseDirectoryPath);

            Encoding encoding = Options.DefaultEncoding;

            string input = ReadFile(result.Path, baseDirectoryPath, ref encoding, context, indent);

            if (input == null)
                return;

            Match match = Options.ContentFilter.Match(input, context.CancellationToken);

            if (match == null)
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
            string baseDirectoryPath = null,
            ColumnWidths columnWidths = null)
        {
            string indent = GetPathIndent(baseDirectoryPath);

            if (!Options.OmitPath)
                WritePath(context, result, baseDirectoryPath, indent, columnWidths);

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
                    relativePath: Options.DisplayRelativePath,
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
            List<Capture> groups = null;

            try
            {
                groups = ListCache<Capture>.GetInstance();

                GetCaptures(match, writerOptions.GroupNumber, context, isPathWritten: !Options.OmitPath, predicate: Options.ContentFilter.Predicate, captures: groups);

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

                        contentWriter = AskReplacementWriter.Create(Options.ContentDisplayStyle, input, Options.ReplaceOptions, lazyWriter, writerOptions, outputInfo);
                    }
                    else
                    {
                        contentWriter = ContentWriter.CreateReplace(Options.ContentDisplayStyle, input, Options.ReplaceOptions, writerOptions, textWriter, outputInfo);
                    }
                }
                else if (Options.DryRun)
                {
                    contentWriter = new EmptyContentWriter(null, FileWriterOptions);
                }
                else
                {
                    contentWriter = new TextWriterContentWriter(input, Options.ReplaceOptions, textWriter, writerOptions);
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
                    if (context.TerminationReason == TerminationReason.Canceled)
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
                                File.WriteAllText(filePath, textWriter!.ToString(), encoding);
                            }
                            else
                            {
                                fileReplacementCount = 0;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            context.TerminationReason = TerminationReason.Canceled;
                            fileReplacementCount = 0;
                        }
                    }
                }
                else if (Options.AskMode == AskMode.Value)
                {
                    if (((AskReplacementWriter)contentWriter).ContinueWithoutAsking)
                        Options.AskMode = AskMode.None;
                }

                telemetry.ProcessedMatchCount += fileReplacementCount;

                if (fileReplacementCount > 0)
                    telemetry.ProcessedFileCount++;
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
                    ListCache<Capture>.Free(groups);
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

            ConsoleColors colors = Colors.Message_OK;

            WriteCount(matchesTitle, matchCount, width1, width2, colors, verbosity);
            Write("  ", colors, verbosity);

            int matchingLinesWidth = 0;
            if (telemetry.MatchingLineCount >= 0)
            {
                matchingLinesWidth += WriteCount("Matching lines", telemetry.MatchingLineCount, colors, verbosity);
                Write("  ", colors, verbosity);
                matchingLinesWidth += 2;
            }

            WriteCount(matchingFilesTitle, matchingFileCount, width3, width4, colors, verbosity);
            WriteLine(verbosity);

            colors = (Options.DryRun) ? Colors.Message_DryRun : Colors.Message_Change;

            WriteCount(replacementsTitle, processedMatchCount, width1, width2, colors, verbosity);
            Write("  ", colors, verbosity);
            Write(' ', matchingLinesWidth, colors, verbosity);
            WriteCount(replacedFilesTitle, processedFileCount, width3, width4, colors, verbosity);

            WriteLine(verbosity);
        }

        protected override ContentWriterOptions CreateContentWriterOptions(string indent)
        {
            return new ContentWriterOptions(
                format: Options.Format,
                symbols: Symbols,
                highlightOptions: Options.HighlightOptions,
                indent: indent);
        }
    }
}
