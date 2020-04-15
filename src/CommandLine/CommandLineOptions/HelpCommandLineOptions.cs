// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CommandLine;

namespace Orang.CommandLine
{
    [Verb("help", HelpText = "Displays help.")]
    internal sealed class HelpCommandLineOptions : AbstractCommandLineOptions
    {
        [Value(index: 0,
            HelpText = "Command name.",
            MetaName = ArgumentMetaNames.Command)]
        public string Command { get; set; }

        [Option(shortName: OptionShortNames.Manual, longName: OptionNames.Manual,
            HelpText = "Display full manual.")]
        public bool Manual { get; set; }

        public bool TryParse(HelpCommandOptions options)
        {
            options.Command = Command;
            options.Manual = Manual;

            return true;
        }
    }
}
