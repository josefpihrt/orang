// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Orang.FileSystem;

namespace Orang.CommandLine
{
    internal class CreatePatternCommand : AbstractCommand<CreatePatternCommandOptions>
    {
        public CreatePatternCommand(CreatePatternCommandOptions options) : base(options)
        {
        }

        protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            PatternOptions patternOptions = Options.PatternOptions;

            string pattern = Options.PatternOrPath;

            if ((patternOptions & PatternOptions.FromFile) != 0
                && !FileSystemHelpers.TryReadAllText(pattern, out pattern!, ex => Logger.WriteError(ex)))
            {
                return CommandResult.Fail;
            }

            pattern = FilterParser.BuildPattern(pattern, patternOptions, Options.Separator);

            var inlineOptions = "";

            if ((patternOptions & PatternOptions.IgnoreCase) != 0)
                inlineOptions += "i";

            if ((patternOptions & PatternOptions.Multiline) != 0)
                inlineOptions += "m";

            if ((patternOptions & PatternOptions.ExplicitCapture) != 0)
                inlineOptions += "n";

            if ((patternOptions & PatternOptions.Singleline) != 0)
                inlineOptions += "s";

            if ((patternOptions & PatternOptions.IgnorePatternWhitespace) != 0)
                inlineOptions += "x";

            if (!string.IsNullOrEmpty(inlineOptions))
                inlineOptions = $"(?{inlineOptions})";

            pattern = inlineOptions + pattern;

            Logger.WriteLine(pattern, Verbosity.Minimal);

            return CommandResult.Success;
        }
    }
}
