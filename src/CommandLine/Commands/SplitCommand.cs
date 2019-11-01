// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class SplitCommand : AbstractCommand
    {
        public SplitCommand(SplitCommandOptions options)
        {
            Options = options;
        }

        public SplitCommandOptions Options { get; }

        protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            SplitData splitData = SplitData.Create(
                Options.Filter.Regex,
                Options.Input,
                Options.MaxCount,
                omitGroups: Options.OmitGroups,
                cancellationToken);

            StringWriter writer = null;
            int count = 0;

            try
            {
                if (!string.IsNullOrEmpty(Options.OutputPath))
                    writer = new StringWriter();

                var outputWriter = new OutputWriter(Options.HighlightOptions, writer);

                count = outputWriter.WriteSplits(splitData, Options, cancellationToken);

                if (writer != null)
                    outputWriter.WriteTo(Options.OutputPath);
            }
            finally
            {
                writer?.Dispose();
            }

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
