// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using Orang.FileSystem;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal abstract class CommonFindContentCommand<TOptions> : CommonFindCommand<TOptions> where TOptions : CommonFindContentCommandOptions
    {
        private ContentWriterOptions _fileWriterOptions;
        private ContentWriterOptions _directoryWriterOptions;

        protected CommonFindContentCommand(TOptions options) : base(options)
        {
        }

        protected ContentWriterOptions FileWriterOptions
        {
            get
            {
                if (_fileWriterOptions == null)
                {
                    string indent = (Options.OmitPath) ? "" : Options.Indent;

                    _fileWriterOptions = CreateMatchWriteOptions(indent);
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

                    _directoryWriterOptions = CreateMatchWriteOptions(indent);
                }

                return _directoryWriterOptions;

                string GetIndent()
                {
                    switch (Options.PathDisplayStyle)
                    {
                        case PathDisplayStyle.Full:
                            return Options.Indent;
                        case PathDisplayStyle.Relative:
                            return Options.DoubleIndent;
                        case PathDisplayStyle.Omit:
                            return "";
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }
        }

        protected abstract ContentWriterOptions CreateMatchWriteOptions(string indent);

        protected void WriteMatches(ContentWriter writer, IEnumerable<Group> groups, SearchContext context)
        {
            try
            {
                writer.WriteMatches(groups, context.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                context.State = SearchState.Canceled;
            }
        }

        protected MaxReason GetGroups(
            Match match,
            int groupNumber,
            SearchContext context,
            bool pathExists,
            List<Group> groups)
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

            MaxReason maxReason = GetGroups(match, groupNumber, count, groups, context.CancellationToken);

            if ((maxReason == MaxReason.CountEqualsMax || maxReason == MaxReason.CountExceedsMax)
                && maxMatches > 0
                && (maxMatchesInFile == 0 || maxMatches <= maxMatchesInFile))
            {
                context.State = SearchState.MaxReached;
            }

            if (pathExists)
            {
                Verbosity verbosity = ConsoleOut.Verbosity;

                if (verbosity >= Verbosity.Detailed
                    || Options.IncludeCount == true)
                {
                    verbosity = (Options.IncludeCount == true) ? Verbosity.Minimal : Verbosity.Detailed;

                    ConsoleOut.Write(" ", Colors.Message_OK, verbosity);
                    ConsoleOut.Write(groups.Count.ToString("n0"), Colors.Message_OK, verbosity);
                    ConsoleOut.WriteIf(maxReason == MaxReason.CountExceedsMax, "+", Colors.Message_OK, verbosity);
                }

                ConsoleOut.WriteLine(Verbosity.Minimal);

                if (Out != null)
                {
                    verbosity = Out.Verbosity;

                    if (verbosity >= Verbosity.Detailed
                        || Options.IncludeCount == true)
                    {
                        verbosity = (Options.IncludeCount == true) ? Verbosity.Minimal : Verbosity.Detailed;

                        Out.Write(" ", verbosity);
                        Out.Write(groups.Count.ToString("n0"), verbosity);
                        Out.WriteIf(maxReason == MaxReason.CountExceedsMax, "+", verbosity);
                    }

                    Out.WriteLine(Verbosity.Minimal);
                }
            }

            return maxReason;
        }

        private MaxReason GetGroups(
            Match match,
            int groupNumber,
            int count,
            List<Group> groups,
            CancellationToken cancellationToken)
        {
            if (groupNumber < 1)
            {
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    groups.Add(match);

                    match = match.NextMatch();

                    if (groups.Count == count)
                    {
                        return (match.Success)
                            ? MaxReason.CountExceedsMax
                            : MaxReason.CountEqualsMax;
                    }

                } while (match.Success);
            }
            else
            {
                Group group = NextGroup();

                while (group != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    groups.Add(group);

                    group = NextGroup();

                    if (groups.Count == count)
                    {
                        return (group != null)
                            ? MaxReason.CountExceedsMax
                            : MaxReason.CountEqualsMax;
                    }
                }
            }

            return MaxReason.None;

            Group NextGroup()
            {
                while (match.Success)
                {
                    Group group = match.Groups[groupNumber];

                    match = match.NextMatch();

                    if (group.Success)
                        return group;
                }

                return null;
            }
        }
    }
}
