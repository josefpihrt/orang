// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Orang.FileSystem;

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

        public MaxReason WriteMatches(ContentWriter writer, Match match, SearchContext context)
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

            var maxReason = MaxReason.None;

            try
            {
                maxReason = writer.WriteMatches(match, count, context.CancellationToken);

                if ((maxReason == MaxReason.CountEqualsMax || maxReason == MaxReason.CountExceedsMax)
                    && maxMatches > 0
                    && (maxMatchesInFile == 0 || maxMatches <= maxMatchesInFile))
                {
                    context.State = SearchState.MaxReached;
                }
            }
            catch (OperationCanceledException)
            {
                context.State = SearchState.Canceled;
            }

            return maxReason;
        }
    }
}
