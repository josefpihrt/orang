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
        private MatchWriterOptions _fileWriterOptions;
        private MatchWriterOptions _directoryWriterOptions;

        public ReplaceCommand(ReplaceCommandOptions options) : base(options)
        {
        }

        protected override bool CanExecuteFile => true;

        private OutputSymbols Symbols => _symbols ?? (_symbols = OutputSymbols.Create(Options.HighlightOptions));

        private MatchEvaluator MatchEvaluator => _matchEvaluator ?? (_matchEvaluator = Options.MatchEvaluator ?? new MatchEvaluator(f => f.Result(Options.Replacement)));

        private MatchWriterOptions FileWriterOptions => _fileWriterOptions ?? (_fileWriterOptions = CreateMatchWriteOptions(Options.Indent));

        private MatchWriterOptions DirectoryWriterOptions => _directoryWriterOptions ?? (_directoryWriterOptions = CreateMatchWriteOptions(Options.DoubleIndent));

        protected override void ExecuteCore(SearchContext context)
        {
            context.Telemetry.MatchingLineCount = -1;
            string input = Options.Input;

            if (input != null)
            {
                int count = 0;
                Match match = Options.ContentFilter.Regex.Match(input);

                if (match.Success)
                {
                    if (!ShouldLog(Verbosity.Normal))
                    {
                        count = match.CountMatches();
                    }
                    else
                    {
                        MatchOutputInfo outputInfo = Options.CreateOutputInfo(input, match);

                        using (MatchWriter matchWriter = MatchWriter.CreateReplace(Options.ContentDisplayStyle, input, MatchEvaluator, FileWriterOptions, outputInfo: outputInfo))
                        {
                            WriteMatches(matchWriter, match, context);
                            count = matchWriter.MatchCount;
                        }
                    }
                }

                WriteLine();
                WriteCount("Replacements", count, Colors.Message_OK, Verbosity.Minimal);
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

            string basePath = (Options.Format.Includes(MiscellaneousDisplayOptions.IncludeFullPath)) ? null : directoryPath;

            foreach (FileSystemFinderResult result in FileSystemHelpers.Find(directoryPath, Options, progress, context.CancellationToken))
            {
                Encoding encoding = Options.DefaultEncoding;

                string input = ReadFile(result.Path, directoryPath, ref encoding, progress);

                if (input == null)
                    continue;

                Match match = regex.Match(input);

                if (match.Success)
                {
                    EndProgress(progress);
                    WritePath(result, basePath, colors: Colors.Matched_Path, matchColors: (Options.HighlightMatch) ? Colors.Match_Path : default, indent: Options.Indent, verbosity: Verbosity.Minimal);

                    ReplaceMatches(result.Path, encoding, input, match, Options.Indent, DirectoryWriterOptions, context);

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

        private string ReadFile(string filePath, string basePath, ref Encoding encoding, FileSystemFinderProgressReporter progress = null)
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

                WriteFileError(ex, filePath, basePath, indent: Options.Indent);
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

            bool consoleNewlineWritten = false;
            bool logNewLineWritten = false;

            try
            {
                int fileMatchCount = 0;
                int fileReplacementCount = 0;

                if (Options.SaveMode == SaveMode.FileByFile)
                {
                    textWriter = new StringWriter();
                }
                else if (Options.SaveMode == SaveMode.NoAsk)
                {
                    textWriter = new StreamWriter(filePath, false, encoding);
                }

                if (Options.SaveMode != SaveMode.ValueByValue
                    && !ShouldLog(Verbosity.Normal))
                {
                    if (Options.SaveMode == SaveMode.DryRun)
                    {
                        fileMatchCount = match.CountMatches();
                    }
                    else
                    {
                        matchWriter = new TextWriterMatchWriter(input, MatchEvaluator, textWriter, writerOptions);
                    }
                }
                else
                {
                    if (Options.SaveMode == SaveMode.ValueByValue)
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

                    matchWriter = CreateWriter();
                }

                if (matchWriter != null)
                {
                    WriteMatches(matchWriter, match, context);
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
                }

                if (Options.SaveMode != SaveMode.ValueByValue)
                {
                    if (ConsoleOut.Verbosity == Verbosity.Minimal)
                    {
                        ConsoleOut.WriteLine($" {fileMatchCount.ToString("n0")}", Colors.Message_OK);
                        consoleNewlineWritten = true;
                    }

                    if (Out?.Verbosity == Verbosity.Minimal)
                    {
                        Out.WriteLine($" {fileMatchCount.ToString("n0")}", Colors.Message_OK);
                        logNewLineWritten = true;
                    }
                }

                telemetry.MatchCount += fileMatchCount;

                if (Options.SaveMode == SaveMode.FileByFile)
                {
                    consoleNewlineWritten = true;

                    if (ConsoleHelpers.Question("Replace content?", indent))
                    {
                        File.WriteAllText(filePath, textWriter.ToString(), encoding);
                    }
                    else
                    {
                        fileReplacementCount = 0;
                    }
                }

                if (Options.SaveMode != SaveMode.DryRun)
                {
                    telemetry.ProcessedMatchCount += fileReplacementCount;

                    if (fileReplacementCount > 0)
                        telemetry.ProcessedFileCount++;
                }
            }
            catch (Exception ex)
            {
                if (!consoleNewlineWritten)
                    ConsoleOut.WriteLine();

                if (!logNewLineWritten)
                    Out?.WriteLine();

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

            MatchWriter CreateWriter()
            {
                MatchOutputInfo outputInfo = Options.CreateOutputInfo(input, match);

                if (Options.SaveMode == SaveMode.ValueByValue)
                {
                    return AskReplacementWriter.Create(Options.ContentDisplayStyle, input, MatchEvaluator, new Lazy<TextWriter>(() => new StreamWriter(filePath, false, encoding)), writerOptions, outputInfo);
                }
                else
                {
                    return MatchWriter.CreateReplace(Options.ContentDisplayStyle, input, MatchEvaluator, writerOptions, textWriter, outputInfo);
                }
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

        private MatchWriterOptions CreateMatchWriteOptions(string indent)
        {
            return new MatchWriterOptions(
                format: Options.Format,
                symbols: Symbols,
                highlightOptions: Options.HighlightOptions,
                indent: indent);
        }
    }
}
