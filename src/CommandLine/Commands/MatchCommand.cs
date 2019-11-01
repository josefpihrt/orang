// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class MatchCommand : AbstractCommand
    {
        public MatchCommand(MatchCommandOptions options)
        {
            Options = options;
        }

        public MatchCommandOptions Options { get; }

        protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            MatchData matchData = MatchData.Create(
                Options.Input,
                Options.Filter.Regex,
                Options.MaxCount,
                cancellationToken);

            OutputWriter outputWriter = null;
            StringWriter writer = null;
            int count = 0;

            try
            {
                if (!string.IsNullOrEmpty(Options.OutputPath))
                    writer = new StringWriter();

                outputWriter = new OutputWriter(Options.HighlightOptions, writer);

                count = outputWriter.WriteMatches(matchData, Options, cancellationToken);

                if (writer != null)
                    outputWriter.WriteTo(Options.OutputPath);
            }
            finally
            {
                writer?.Dispose();
            }

            if (count > 0)
                WriteLine();

            WriteGroups(matchData.GroupDefinitions);
            WriteLine(Verbosity.Minimal);

            WriteCount("Matches", matchData.Count, Colors.Message_OK, Verbosity.Minimal);

            if (count != matchData.Count)
            {
                Write("  ", Colors.Message_OK, Verbosity.Minimal);
                WriteCount("Captures", count, Colors.Message_OK, Verbosity.Minimal);
            }

            WriteLine(Verbosity.Minimal);

            return (count > 0) ? CommandResult.Success : CommandResult.NoSuccess;
        }
    }
}
