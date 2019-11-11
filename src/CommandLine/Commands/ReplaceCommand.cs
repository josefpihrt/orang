// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
            string input = Options.Input;

            if (input != null)
            {
                int count = 0;
                var maxReason = MaxReason.None;
                Match match = Options.ContentFilter.Regex.Match(input);

                if (match.Success)
                {
                    if (!ShouldLog(Verbosity.Normal))
                    {
                        using (var matchWriter = new EmptyMatchWriter(null, FileWriterOptions))
                        {
                            maxReason = WriteMatches(matchWriter, match, context);

                            count = matchWriter.MatchCount;
                        }
                    }
                    else
                    {
                        MatchOutputInfo outputInfo = Options.CreateOutputInfo(input, match);

                        using (MatchWriter matchWriter = MatchWriter.CreateReplace(Options.ContentDisplayStyle, input, MatchEvaluator, FileWriterOptions, outputInfo: outputInfo))
                        {
                            maxReason = WriteMatches(matchWriter, match, context);
                            count = matchWriter.MatchCount;
                        }
                    }
                }

                WriteLine();
                WriteCount("Replacements", count, Colors.Message_OK, Verbosity.Minimal);
                WriteIf(maxReason == MaxReason.CountExceedsMax, "+", Colors.Message_OK, Verbosity.Minimal);
                WriteLine(Verbosity.Minimal);
            }
            else
            {
                base.ExecuteCore(context);
            }
        }

        protected override void ExecuteFile(string filePath, SearchContext context)
        {
            context.Telemetry.FileCount++;

            Encoding encoding = Options.DefaultEncoding;

            string input = ReadFile(filePath, null, ref encoding);

            if (input != null)
            {
                Match match = Options.ContentFilter.Regex.Match(input);

                if (match.Success)
                {
                    if (!Options.OmitPath)
                        WritePath(filePath, colors: Colors.Matched_Path, verbosity: Verbosity.Minimal);

                    ReplaceMatches(filePath, encoding, input, match, "", FileWriterOptions, context);

                    if (Options.MaxMatchingFiles == context.Telemetry.MatchingFileCount)
                        context.State = SearchState.MaxReached;
                }
            }
        }

        protected override void ExecuteDirectory(string directoryPath, SearchContext context, FileSystemFinderProgressReporter progress)
        {
            Regex regex = Options.ContentFilter.Regex;
            string basePath = (Options.PathDisplayStyle == PathDisplayStyle.Full) ? null : directoryPath;
            string indent = (Options.PathDisplayStyle == PathDisplayStyle.Relative) ? Options.Indent : "";

            foreach (FileSystemFinderResult result in Find(directoryPath, progress, context.CancellationToken))
            {
                Encoding encoding = Options.DefaultEncoding;

                string input = ReadFile(result.Path, directoryPath, ref encoding, progress, indent);

                if (input == null)
                    continue;

                Match match = regex.Match(input);

                if (match.Success)
                {
                    EndProgress(progress);

                    if (!Options.OmitPath)
                        WritePath(result, basePath, colors: Colors.Matched_Path, matchColors: (Options.HighlightMatch) ? Colors.Match_Path : default, indent: indent, verbosity: Verbosity.Minimal);

                    ReplaceMatches(result.Path, encoding, input, match, indent, DirectoryWriterOptions, context);

                    if (context.State == SearchState.Canceled)
                        break;

                    if (Options.MaxMatchingFiles == context.Telemetry.MatchingFileCount)
                        context.State = SearchState.MaxReached;

                    if (context.State == SearchState.MaxReached)
                        break;
                }
            }

            context.Telemetry.SearchedDirectoryCount = progress.SearchedDirectoryCount;
            context.Telemetry.DirectoryCount = progress.DirectoryCount;
            context.Telemetry.FileCount = progress.FileCount;
        }

        private string ReadFile(string filePath, string basePath, ref Encoding encoding, FileSystemFinderProgressReporter progress = null, string indent = null)
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
                EndProgress(progress);

                WriteFileError(ex, filePath, basePath, indent: indent);
                return null;
            }
        }

        private void ReplaceMatches(
            string filePath,
            Encoding encoding,
            string input,
            Match match,
            string indent,
            MatchWriterOptions writerOptions,
            SearchContext context)
        {
            SearchTelemetry telemetry = context.Telemetry;
            telemetry.MatchingFileCount++;

            MatchWriter matchWriter = null;
            TextWriter textWriter = null;

            var maxReason = MaxReason.None;
            bool consoleNewlineWritten = Options.OmitPath;
            bool logNewLineWritten = Options.OmitPath;

            try
            {
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

                if (Options.AskMode != AskMode.Value
                    && !ShouldLog(Verbosity.Normal))
                {
                    if (Options.DryRun)
                    {
                        matchWriter = new EmptyMatchWriter(null, FileWriterOptions);
                    }
                    else
                    {
                        matchWriter = new TextWriterMatchWriter(input, MatchEvaluator, textWriter, writerOptions);
                    }
                }
                else
                {
                    if (!Options.OmitPath)
                    {
                        if (Options.AskMode == AskMode.Value)
                        {
                            ConsoleOut.WriteLine();
                            consoleNewlineWritten = true;
                        }
                        else if (ConsoleOut.Verbosity >= Verbosity.Normal)
                        {
                            ConsoleOut.WriteLine(Verbosity.Normal);
                            consoleNewlineWritten = true;
                        }

                        if (Out?.Verbosity >= Verbosity.Normal)
                        {
                            Out.WriteLine(Verbosity.Normal);
                            logNewLineWritten = true;
                        }
                    }

                    MatchOutputInfo outputInfo = Options.CreateOutputInfo(input, match);

                    if (Options.AskMode == AskMode.Value)
                    {
                        Lazy<TextWriter> lazyWriter = (Options.DryRun)
                            ? null
                            : new Lazy<TextWriter>(() => new StreamWriter(filePath, false, encoding));

                        matchWriter = AskReplacementWriter.Create(Options.ContentDisplayStyle, input, MatchEvaluator, lazyWriter, writerOptions, outputInfo);
                    }
                    else
                    {
                        matchWriter = MatchWriter.CreateReplace(Options.ContentDisplayStyle, input, MatchEvaluator, writerOptions, textWriter, outputInfo);
                    }
                }

                maxReason = WriteMatches(matchWriter, match, context);
                fileMatchCount = matchWriter.MatchCount;
                fileReplacementCount = (matchWriter is AskReplacementWriter askReplacementWriter) ? askReplacementWriter.ReplacementCount : fileMatchCount;

                if (matchWriter.MatchingLineCount >= 0)
                {
                    if (telemetry.MatchingLineCount == -1)
                        telemetry.MatchingLineCount = 0;

                    telemetry.MatchingLineCount += matchWriter.MatchingLineCount;
                }

                if (!consoleNewlineWritten
                    && ConsoleOut.Verbosity >= Verbosity.Normal)
                {
                    consoleNewlineWritten = fileMatchCount > 0;
                }

                if (!logNewLineWritten
                    && Out?.Verbosity >= Verbosity.Normal)
                {
                    logNewLineWritten = fileMatchCount > 0;
                }

                if (Options.AskMode != AskMode.Value
                    && !Options.OmitPath)
                {
                    if (ConsoleOut.Verbosity == Verbosity.Minimal)
                    {
                        ConsoleOut.Write($" {fileMatchCount.ToString("n0")}", Colors.Message_OK);
                        ConsoleOut.WriteIf(maxReason == MaxReason.CountExceedsMax, "+", Colors.Message_OK);
                        ConsoleOut.WriteLine();
                        consoleNewlineWritten = true;
                    }

                    if (Out?.Verbosity == Verbosity.Minimal)
                    {
                        Out.Write($" {fileMatchCount.ToString("n0")}");
                        Out.WriteIf(maxReason == MaxReason.CountExceedsMax, "+");
                        Out.WriteLine();
                        logNewLineWritten = true;
                    }
                }

                telemetry.MatchCount += fileMatchCount;

                if (Options.AskMode == AskMode.File)
                {
                    if (context.State == SearchState.Canceled)
                    {
                        fileReplacementCount = 0;
                    }
                    else
                    {
                        consoleNewlineWritten = true;

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
                    if (((AskReplacementWriter)matchWriter).ContinueWithoutAsking)
                        Options.AskMode = AskMode.None;
                }

                if (!Options.DryRun)
                {
                    telemetry.ProcessedMatchCount += fileReplacementCount;

                    if (fileReplacementCount > 0)
                        telemetry.ProcessedFileCount++;
                }
            }
            catch (Exception ex)
            {
                ConsoleOut.WriteLineIf(!consoleNewlineWritten);
                Out?.WriteLineIf(!logNewLineWritten);

                if (ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    WriteFileError(ex, indent: indent);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                matchWriter?.Dispose();
            }
        }

        protected override void WriteSummary(SearchTelemetry telemetry)
        {
            WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget);

            if (!ShouldLog(Verbosity.Minimal))
                return;

            WriteLine(Verbosity.Minimal);

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

            WriteCount(matchesTitle, matchCount, width1, width2, Colors.Message_OK, Verbosity.Minimal);
            Write("  ", Colors.Message_OK, Verbosity.Minimal);

            int matchingLinesWidth = 0;
            if (telemetry.MatchingLineCount >= 0)
            {
                matchingLinesWidth += WriteCount("Matching lines", telemetry.MatchingLineCount, Colors.Message_OK, Verbosity.Minimal);
                Write("  ", Colors.Message_OK, Verbosity.Minimal);
                matchingLinesWidth += 2;
            }

            WriteCount(matchingFilesTitle, matchingFileCount, width3, width4, Colors.Message_OK, Verbosity.Minimal);
            WriteLine(Verbosity.Minimal);

            WriteCount(replacementsTitle, processedMatchCount, width1, width2, Colors.Message_Change, Verbosity.Minimal);
            Write("  ", Colors.Message_Change, Verbosity.Minimal);
            Write(' ', matchingLinesWidth, Colors.Message_Change, Verbosity.Minimal);
            WriteCount(replacedFilesTitle, processedFileCount, width3, width4, Colors.Message_Change, Verbosity.Minimal);

            WriteLine(Verbosity.Minimal);
        }

        protected override MatchWriterOptions CreateMatchWriteOptions(string indent)
        {
            return new MatchWriterOptions(
                format: Options.Format,
                symbols: Symbols,
                highlightOptions: Options.HighlightOptions,
                indent: indent);
        }
    }
}
