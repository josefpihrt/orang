// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class MatchCommand : RegexCommand<MatchCommandOptions>
    {
        public MatchCommand(MatchCommandOptions options) : base(options)
        {
        }

        protected override CommandResult ExecuteCore(TextWriter output, CancellationToken cancellationToken = default)
        {
            MatchData matchData = MatchData.Create(
                Options.Input,
                Options.Filter.Regex,
                Options.MaxCount,
                cancellationToken);

            var outputWriter = new OutputWriter(Options.HighlightOptions);

            int count = outputWriter.WriteMatches(matchData, Options, output, cancellationToken);

            if (count > 0)
                WriteLine();

            if (ShouldLog(Verbosity.Detailed)
                || Options.IncludeSummary)
            {
                Verbosity verbosity = (Options.IncludeSummary) ? Verbosity.Minimal : Verbosity.Detailed;

                WriteGroups(matchData.GroupDefinitions, verbosity: verbosity);
                WriteLine(verbosity);

                WriteCount("Matches", matchData.Count, Colors.Message_OK, verbosity);

                if (count != matchData.Count)
                {
                    Write("  ", Colors.Message_OK, verbosity);
                    WriteCount("Captures", count, Colors.Message_OK, verbosity);
                }

                WriteLine(verbosity);
            }

            return (count > 0) ? CommandResult.Success : CommandResult.NoSuccess;
        }
    }
}
