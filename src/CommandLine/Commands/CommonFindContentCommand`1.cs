// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Orang.FileSystem;

namespace Orang.CommandLine
{
    internal abstract class CommonFindContentCommand<TOptions> : CommonFindCommand<TOptions> where TOptions : CommonFindContentCommandOptions
    {
        protected CommonFindContentCommand(TOptions options) : base(options)
        {
        }

        public void WriteMatches(MatchWriter writer, Match match, SearchContext context)
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

            WriteResult result = writer.WriteMatches(match, count, context.CancellationToken);

            if (result == WriteResult.MaxReached
                && maxMatches > 0
                && (maxMatchesInFile == 0 || maxMatches <= maxMatchesInFile))
            {
                context.State = SearchState.MaxReached;
            }
        }
    }
}
