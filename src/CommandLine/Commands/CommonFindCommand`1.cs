﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Orang.FileSystem;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

internal abstract class CommonFindCommand<TOptions> :
    FileSystemCommand<TOptions> where TOptions : CommonFindCommandOptions
{
    private OutputSymbols? _symbols;
    private ContentWriterOptions? _fileWriterOptions;
    private ContentWriterOptions? _directoryWriterOptions;

    protected CommonFindCommand(TOptions options, Logger logger) : base(options, logger)
    {
        if (_logger.ShouldWrite(Verbosity.Minimal))
        {
            PathWriter = new PathWriter(
                _logger,
                pathColors: Colors.Matched_Path,
                matchColors: (Options.HighlightMatch) ? Colors.Match_Path : default,
                relativePath: Options.DisplayRelativePath);
        }
    }

    public Matcher? ContentFilter => Options.ContentFilter;

    private OutputSymbols Symbols => _symbols ??= OutputSymbols.Create(Options.HighlightOptions);

    protected AskMode AskMode { get; set; }

    private PathWriter? PathWriter { get; }

    public override bool CanEndProgress
    {
        get
        {
            return !Options.OmitPath
                || (ContentFilter is not null
                    && !Options.OmitContent
                    && _logger.ConsoleOut.Verbosity > Verbosity.Minimal);
        }
    }

    protected ContentWriterOptions FileWriterOptions
    {
        get
        {
            if (_fileWriterOptions is null)
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
            if (_directoryWriterOptions is null)
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
                    PathDisplayStyle.Relative => Options.Indent,
                    PathDisplayStyle.Match => Options.Indent,
                    PathDisplayStyle.Omit => "",
                    _ => throw new InvalidOperationException($"Unknown enum value '{Options.PathDisplayStyle}'."),
                };
            }
        }
    }

    protected virtual ContentWriterOptions CreateContentWriterOptions(string indent)
    {
        int groupNumber = ContentFilter!.GroupNumber;

        GroupDefinition? groupDefinition = (groupNumber >= 0)
            ? new GroupDefinition(groupNumber, ContentFilter.GroupName)
            : default(GroupDefinition?);

        HighlightOptions highlightOptions = Options.HighlightOptions;

        if (ReferenceEquals(ContentFilter, Matcher.EntireInput))
            highlightOptions &= ~HighlightOptions.DefaultOrMatch;

        return new ContentWriterOptions(
            format: Options.Format,
            groupDefinition,
            symbols: Symbols,
            highlightOptions: highlightOptions,
            indent: indent);
    }

    protected override void ExecuteCore(SearchContext context)
    {
        context.Telemetry.MatchingLineCount = -1;

        if (_logger.ConsoleOut.Verbosity >= Verbosity.Minimal)
            AskMode = Options.AskMode;

        base.ExecuteCore(context);
    }

    protected override void ExecuteFile(string filePath, SearchContext context)
    {
        FileMatch? fileMatch = MatchFile(filePath);

        if (fileMatch is not null)
        {
            if (ContentFilter is not null)
            {
                ExecuteMatchWithContent(fileMatch, context, FileWriterOptions);
            }
            else
            {
                ExecuteMatch(fileMatch, context);
            }
        }
    }

    protected override void ExecuteDirectory(string directoryPath, SearchContext context)
    {
        foreach (FileMatch fileMatch in GetMatches(directoryPath, context, this as INotifyDirectoryChanged))
        {
            if (ContentFilter is not null)
            {
                ExecuteMatchWithContent(fileMatch, context, DirectoryWriterOptions, directoryPath);
            }
            else
            {
                ExecuteMatch(fileMatch, context, directoryPath);
            }

            if (context.TerminationReason == TerminationReason.Canceled)
                break;

            if (context.TerminationReason == TerminationReason.MaxReached)
                break;
        }
    }

    protected void ExecuteMatchWithContent(
        FileMatch fileMatch,
        SearchContext context,
        ContentWriterOptions writerOptions,
        string? baseDirectoryPath = null)
    {
        if (fileMatch.IsDirectory)
        {
            context.Telemetry.MatchingDirectoryCount++;
        }
        else
        {
            context.Telemetry.MatchingFileCount++;
        }

        if (Options.MaxMatchingFiles == context.Telemetry.MatchingFileDirectoryCount)
            context.TerminationReason = TerminationReason.MaxReached;

        if (context.Results is not null)
        {
            var searchResult = new SearchResult(fileMatch, baseDirectoryPath, writerOptions);

            context.Results.Add(searchResult);
        }
        else
        {
            if (CanEndProgress)
                EndProgress(context);

            ExecuteMatchWithContentCore(fileMatch, context, writerOptions, baseDirectoryPath);
        }
    }

    protected virtual void ExecuteMatchWithContentCore(
        FileMatch fileMatch,
        SearchContext context,
        ContentWriterOptions writerOptions,
        string? baseDirectoryPath = null,
        ColumnWidths? columnWidths = null)
    {
        if (!fileMatch.IsDirectory
            && ContentFilter?.Invert == false
            && !Options.OmitContent
            && _logger.ShouldWrite(Verbosity.Normal))
        {
            string indent = GetPathIndent(baseDirectoryPath);

            bool isPathDisplayed = !Options.OmitPath;

            if (isPathDisplayed)
                WritePath(context, fileMatch, baseDirectoryPath, indent, columnWidths, includeNewline: false);

            if (ContentFilter!.Invert
                || fileMatch.IsDirectory)
            {
                _logger.WriteLineIf(isPathDisplayed, Verbosity.Minimal);
            }
            else
            {
                WriteContent(
                    context,
                    fileMatch,
                    writerOptions,
                    isPathDisplayed: isPathDisplayed);
            }
        }

        ExecuteMatchCore(fileMatch, context, baseDirectoryPath, columnWidths);
    }

    protected override void ExecuteResult(SearchResult result, SearchContext context, ColumnWidths? columnWidths)
    {
        if (ContentFilter is not null)
        {
            ExecuteMatchWithContentCore(
                result.FileMatch,
                context,
                result.WriterOptions!,
                result.BaseDirectoryPath,
                columnWidths);
        }
        else
        {
            ExecuteMatchCore(result.FileMatch, context, result.BaseDirectoryPath, columnWidths);
        }
    }

    protected void AskToContinue(SearchContext context, string indent)
    {
        if (AskMode == AskMode.File)
        {
            try
            {
                if (ConsoleHelpers.AskToContinue(indent) == DialogResult.YesToAll)
                    AskMode = AskMode.None;
            }
            catch (OperationCanceledException)
            {
                context.TerminationReason = TerminationReason.Canceled;
            }
        }
    }

    protected virtual void WriteMatches(ContentWriter writer, IEnumerable<Capture> captures, SearchContext context)
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
        bool isPathDisplayed,
        Func<string, bool>? predicate,
        List<Capture> captures)
    {
        (int maxMatchesInFile, int maxTotalMatches, int count) = CalculateMaxCount(context);

        MaxReason maxReason = CaptureFactory.GetCaptures(
            ref captures,
            match,
            groupNumber,
            count,
            predicate,
            context.CancellationToken);

        if ((maxReason == MaxReason.CountEqualsMax || maxReason == MaxReason.CountExceedsMax)
            && maxTotalMatches > 0
            && (maxMatchesInFile == 0 || maxTotalMatches <= maxMatchesInFile))
        {
            context.TerminationReason = TerminationReason.MaxReached;
        }

        if (isPathDisplayed)
            _logger.WriteFilePathEnd(captures.Count, maxReason, Options.IncludeCount);

        return maxReason;
    }

    protected (int maxMatchesInFile, int maxTotalMatches, int count) CalculateMaxCount(
        SearchContext context)
    {
        int maxMatchesInFile = Options.MaxMatchesInFile;
        int maxTotalMatches = Options.MaxTotalMatches;
        int count = 0;

        if (maxMatchesInFile > 0)
        {
            if (maxTotalMatches > 0)
            {
                maxTotalMatches -= context.Telemetry.MatchCount;
                count = Math.Min(maxMatchesInFile, maxTotalMatches);
            }
            else
            {
                count = maxMatchesInFile;
            }
        }
        else if (maxTotalMatches > 0)
        {
            maxTotalMatches -= context.Telemetry.MatchCount;
            count = maxTotalMatches;
        }

        Debug.Assert(count >= 0, count.ToString());

        return (maxMatchesInFile, maxTotalMatches, count);
    }

    protected void WritePath(
        SearchContext context,
        FileMatch fileMatch,
        string? baseDirectoryPath,
        string indent,
        ColumnWidths? columnWidths,
        bool includeNewline)
    {
        WritePath(context, fileMatch, baseDirectoryPath, indent, columnWidths);

        if (includeNewline)
            _logger.WriteLine(Verbosity.Minimal);
    }

    private void WritePath(
        SearchContext context,
        FileMatch fileMatch,
        string? baseDirectoryPath,
        string indent,
        ColumnWidths? columnWidths)
    {
        if (Options.PathDisplayStyle == PathDisplayStyle.Match
            && fileMatch.Name is not null
            && !object.ReferenceEquals(fileMatch.Name, Match.Empty))
        {
            if (_logger.ShouldWrite(Verbosity.Minimal))
            {
                _logger.Write(indent, Verbosity.Minimal);
                _logger.Write(fileMatch.Name.Value, (Options.HighlightMatch) ? Colors.Match_Path : default, Verbosity.Minimal);
            }
        }
        else
        {
            PathWriter?.WritePath(fileMatch, baseDirectoryPath, indent);
        }

        WriteProperties(context, fileMatch, columnWidths);
    }

    private void WriteContent(
        SearchContext context,
        FileMatch fileMatch,
        ContentWriterOptions writerOptions,
        bool isPathDisplayed)
    {
        ContentWriter? contentWriter = null;

        try
        {
            Match match = fileMatch.Content!;

            contentWriter = CommandLine.ContentWriter.CreateFind(
                contentDisplayStyle: Options.ContentDisplayStyle,
                input: fileMatch.ContentText,
                options: writerOptions,
                storage: null,
                outputInfo: Options.CreateOutputInfo(fileMatch.ContentText, match, ContentFilter!),
                writer: ContentWriter,
                ask: AskMode == AskMode.Value);

            WriteMatches(context, match, writerOptions, contentWriter, isPathDisplayed);

            SearchTelemetry telemetry = context.Telemetry;

            telemetry.MatchCount += contentWriter.MatchCount;

            if (contentWriter.MatchingLineCount >= 0)
            {
                if (telemetry.MatchingLineCount == -1)
                    telemetry.MatchingLineCount = 0;

                telemetry.MatchingLineCount += contentWriter.MatchingLineCount;
            }

            if (AskMode == AskMode.Value)
            {
                if (contentWriter is AskValueContentWriter askValueContentWriter)
                {
                    if (!askValueContentWriter.Ask)
                        AskMode = AskMode.None;
                }
                else if (contentWriter is AskLineContentWriter askLineContentWriter)
                {
                    if (!askLineContentWriter.Ask)
                        AskMode = AskMode.None;
                }
            }
        }
        finally
        {
            contentWriter?.Dispose();
        }
    }

    protected void WriteMatches(
        SearchContext context,
        Match match,
        ContentWriterOptions writerOptions,
        ContentWriter contentWriter,
        bool isPathDisplayed)
    {
        List<Capture>? captures = null;

        try
        {
            captures = ListCache<Capture>.GetInstance();

            GetCaptures(
                match,
                writerOptions.GroupNumber,
                context,
                isPathDisplayed: isPathDisplayed,
                predicate: ContentFilter!.Predicate,
                captures: captures);

            WriteMatches(contentWriter, captures, context);
        }
        finally
        {
            if (captures is not null)
                ListCache<Capture>.Free(captures);
        }
    }

    protected bool AskToExecute(SearchContext context, string question, string indent)
    {
        DialogResult result = ConsoleHelpers.Ask(question, indent);

        switch (result)
        {
            case DialogResult.Yes:
                {
                    return true;
                }
            case DialogResult.YesToAll:
                {
                    Options.AskMode = AskMode.None;
                    return true;
                }
            case DialogResult.No:
            case DialogResult.None:
                {
                    return false;
                }
            case DialogResult.NoToAll:
            case DialogResult.Cancel:
                {
                    context.TerminationReason = TerminationReason.Canceled;
                    return false;
                }
            default:
                {
                    throw new InvalidOperationException($"Unknown enum value '{result}'.");
                }
        }
    }

    protected override void WriteSummary(SearchTelemetry telemetry, Verbosity verbosity)
    {
        _logger.WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget, verbosity);

        if (!_logger.ShouldWrite(verbosity))
            return;

        _logger.WriteLine(verbosity);

        if (ContentFilter is not null)
        {
            WriteContentSummary(telemetry, verbosity);
        }
        else
        {
            if (Options.SearchTarget != SearchTarget.Directories)
                _logger.WriteCount("Matching files", telemetry.MatchingFileCount, Colors.Message_OK, verbosity);

            if (Options.SearchTarget != SearchTarget.Files)
            {
                if (Options.SearchTarget != SearchTarget.Directories)
                    _logger.Write("  ", Colors.Message_OK, verbosity);

                _logger.WriteCount("Matching directories", telemetry.MatchingDirectoryCount, Colors.Message_OK, verbosity);
            }
        }

        _logger.WriteLine(verbosity);
    }

    protected void WriteContentSummary(SearchTelemetry telemetry, Verbosity verbosity)
    {
        _logger.WriteCount("Matches", telemetry.MatchCount, Colors.Message_OK, verbosity);
        _logger.Write("  ", Colors.Message_OK, verbosity);

        if (telemetry.MatchingLineCount > 0)
        {
            _logger.WriteCount("Matching lines", telemetry.MatchingLineCount, Colors.Message_OK, verbosity);
            _logger.Write("  ", Colors.Message_OK, verbosity);
        }

        if (telemetry.MatchingFileCount > 0)
            _logger.WriteCount("Matching files", telemetry.MatchingFileCount, Colors.Message_OK, verbosity);
    }
}
