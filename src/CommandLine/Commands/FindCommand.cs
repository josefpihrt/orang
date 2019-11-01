// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        protected override void ExecuteDirectory(string directoryPath, SearchContext context, FileSystemFinderProgressReporter progress)
        {
            SearchTelemetry telemetry = context.Telemetry;

            string basePath = (Options.Format.Includes(MiscellaneousDisplayOptions.IncludeFullPath)) ? null : directoryPath;

            foreach (FileSystemFinderResult result in FileSystemHelpers.Find(directoryPath, Options, progress, context.CancellationToken))
            {
                EndProgress(progress);
                WritePath(result, basePath, colors: Colors.Matched_Path, matchColors: (Options.HighlightMatch) ? Colors.Match : default, indent: Options.Indent, Verbosity.Minimal);
                WriteLine(Verbosity.Minimal);

                if (ConsoleHelpers.QuestionIf(_ask, "Continue without asking?", Options.Indent))
                    _ask = false;

                if (result.IsDirectory)
                {
                    telemetry.MatchingDirectoryCount++;
                }
                else
                {
                    telemetry.MatchingFileCount++;
                }

                if (Options.MaxMatchingFiles == telemetry.MatchingFileCount + telemetry.MatchingDirectoryCount)
                {
                    context.State = SearchState.MaxReached;
                    break;
                }
            }

            telemetry.SearchedDirectoryCount = progress.SearchedDirectoryCount;
            telemetry.FileCount = progress.FileCount;
            telemetry.DirectoryCount = progress.DirectoryCount;
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
