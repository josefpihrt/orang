// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Orang.FileSystem;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class FindCommand : CommonFindCommand<FindCommandOptions>
    {
        private bool _ask;

        public FindCommand(FindCommandOptions options) : base(options)
        {
        }

        protected override void ExecuteCore(SearchContext context)
        {
            if (Options.AskMode == AskMode.File
                && ConsoleOut.Verbosity >= Verbosity.Minimal)
            {
                _ask = true;
            }

            base.ExecuteCore(context);
        }

        protected override void ExecuteFile(string filePath, SearchContext context)
        {
            SearchTelemetry telemetry = context.Telemetry;
            telemetry.FileCount++;

            const string indent = null;

            FileSystemFinderResult? maybeResult = MatchFile(filePath);

            if (maybeResult == null)
                return;

            FileSystemFinderResult result = maybeResult.Value;

            WritePath(result, colors: Colors.Matched_Path, matchColors: (Options.HighlightMatch) ? Colors.Match : default, indent: indent, verbosity: Verbosity.Minimal);
            WriteLine(Verbosity.Minimal);

            if (_ask)
            {
                try
                {
                    if (ConsoleHelpers.Question("Continue without asking?", indent))
                        _ask = false;
                }
                catch (OperationCanceledException)
                {
                    context.State = SearchState.Canceled;
                }
            }

            telemetry.MatchingFileCount++;

            context.Output?.WriteLine(filePath);

            if (Options.MaxMatchingFiles == telemetry.MatchingFileCount + telemetry.MatchingDirectoryCount)
            {
                context.State = SearchState.MaxReached;
            }
        }

        protected override void ExecuteDirectory(string directoryPath, SearchContext context, FileSystemFinderProgressReporter progress)
        {
            SearchTelemetry telemetry = context.Telemetry;
            string indent = (Options.PathDisplayStyle == PathDisplayStyle.Relative) ? Options.Indent : "";

            foreach (FileSystemFinderResult result in Find(directoryPath, progress, context.CancellationToken))
            {
                EndProgress(progress);

                WritePath(
                    result,
                    directoryPath,
                    relativePath: Options.PathDisplayStyle == PathDisplayStyle.Relative,
                    colors: Colors.Matched_Path,
                    matchColors: (Options.HighlightMatch) ? Colors.Match : default,
                    indent: indent,
                    verbosity: Verbosity.Minimal);

                WriteLine(Verbosity.Minimal);

                if (_ask)
                {
                    try
                    {
                        if (ConsoleHelpers.Question("Continue without asking?", indent))
                            _ask = false;
                    }
                    catch (OperationCanceledException)
                    {
                        context.State = SearchState.Canceled;
                        break;
                    }
                }

                if (result.IsDirectory)
                {
                    telemetry.MatchingDirectoryCount++;
                }
                else
                {
                    telemetry.MatchingFileCount++;
                }

                context.Output?.WriteLine(result.Path);

                if (Options.MaxMatchingFiles == telemetry.MatchingFileCount + telemetry.MatchingDirectoryCount)
                {
                    context.State = SearchState.MaxReached;
                    break;
                }
            }
        }

        protected override void WriteSummary(SearchTelemetry telemetry)
        {
            WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget);

            if (!ShouldLog(Verbosity.Minimal))
                return;

            WriteLine(Verbosity.Minimal);

            if (Options.SearchTarget != SearchTarget.Directories)
            {
                WriteCount("Matching files", telemetry.MatchingFileCount, Colors.Message_OK, Verbosity.Minimal);
                WriteLine(Verbosity.Minimal);
            }

            if (Options.SearchTarget != SearchTarget.Files)
            {
                WriteCount("Matching directories", telemetry.MatchingDirectoryCount, Colors.Message_OK, Verbosity.Minimal);
                WriteLine(Verbosity.Minimal);
            }
        }
    }
}
