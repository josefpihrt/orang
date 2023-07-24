// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

internal class RegexMatchCommand : RegexCommand<RegexMatchCommandOptions>
{
    public RegexMatchCommand(RegexMatchCommandOptions options, Logger logger) : base(options, logger)
    {
    }

    protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
    {
        MatchData matchData = MatchData.Create(
            Options.Input,
            Options.Filter.Regex,
            Options.MaxCount,
            cancellationToken);

        var outputWriter = new OutputWriter(Options.HighlightOptions, _logger);

        int count = outputWriter.WriteMatches(matchData, Options, cancellationToken);

        if (count > 0)
            _logger.WriteLine();

        if (_logger.ShouldWrite(Verbosity.Detailed)
            || Options.IncludeSummary)
        {
            Verbosity verbosity = (Options.IncludeSummary) ? Verbosity.Minimal : Verbosity.Detailed;

            _logger.WriteGroups(matchData.GroupDefinitions, verbosity: verbosity);
            _logger.WriteLine(verbosity);

            if (Options.ContentDisplayStyle == ContentDisplayStyle.Value
                && Options.ModifyOptions.HasAnyFunction)
            {
                _logger.WriteCount("Values", count, Colors.Message_OK, verbosity);
            }
            else
            {
                _logger.WriteCount("Matches", matchData.Count, Colors.Message_OK, verbosity);

                if (count != matchData.Count)
                {
                    _logger.Write("  ", Colors.Message_OK, verbosity);
                    _logger.WriteCount("Captures", count, Colors.Message_OK, verbosity);
                }
            }

            _logger.WriteLine(verbosity);
        }

        return (count > 0) ? CommandResult.Success : CommandResult.NoMatch;
    }
}
