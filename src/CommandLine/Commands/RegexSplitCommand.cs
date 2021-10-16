// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal class RegexSplitCommand : RegexCommand<RegexSplitCommandOptions>
    {
        public RegexSplitCommand(RegexSplitCommandOptions options, Logger logger) : base(options, logger)
        {
        }

        protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            SplitData splitData = SplitData.Create(
                Options.Filter.Regex,
                Options.Input,
                Options.MaxCount,
                omitGroups: Options.OmitGroups,
                cancellationToken);

            var outputWriter = new OutputWriter(Options.HighlightOptions, _logger);

            int count = outputWriter.WriteSplits(splitData, Options, cancellationToken);

            _logger.WriteLine();

            if (_logger.ShouldWrite(Verbosity.Detailed)
                || Options.IncludeSummary)
            {
                Verbosity verbosity = (Options.IncludeSummary) ? Verbosity.Minimal : Verbosity.Detailed;

                _logger.WriteGroups(splitData.GroupDefinitions, verbosity: verbosity);
                _logger.WriteLine(verbosity);

                _logger.WriteCount(
                    (Options.ContentDisplayStyle == ContentDisplayStyle.Value && Options.ModifyOptions.HasAnyFunction)
                        ? "Values"
                        : "Splits",
                    count,
                    Colors.Message_OK,
                    verbosity);

                _logger.WriteLine(verbosity);
            }

            return (count > 0) ? CommandResult.Success : CommandResult.NoMatch;
        }
    }
}
