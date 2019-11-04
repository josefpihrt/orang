// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class SplitCommand : RegexCommand<SplitCommandOptions>
    {
        public SplitCommand(SplitCommandOptions options) : base(options)
        {
        }

        protected override CommandResult ExecuteCore(TextWriter output, CancellationToken cancellationToken = default)
        {
            SplitData splitData = SplitData.Create(
                Options.Filter.Regex,
                Options.Input,
                Options.MaxCount,
                omitGroups: Options.OmitGroups,
                cancellationToken);

            var outputWriter = new OutputWriter(Options.HighlightOptions);

            int count = outputWriter.WriteSplits(splitData, Options, output, cancellationToken);

            WriteLine();
            WriteGroups(splitData.GroupDefinitions);
            WriteLine(Verbosity.Minimal);

            WriteCount(
                "Splits",
                count,
                Colors.Message_OK,
                Verbosity.Minimal);

            WriteLine(Verbosity.Minimal);

            return (count > 0) ? CommandResult.Success : CommandResult.NoSuccess;
        }
    }
}
