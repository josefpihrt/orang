// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;

namespace Orang.CommandLine
{
    internal abstract class AbstractCommandLineOptions
    {
        [Option(longName: OptionNames.FileLog,
            HelpText = "Syntax is <LOG_PATH> [<LOG_OPTIONS>].",
            MetaValue = "<FILE_LOG>")]
        public IEnumerable<string> FileLog { get; set; }

        [Option(shortName: OptionShortNames.Help, longName: OptionNames.Help,
            HelpText = "Show command line help.")]
        public bool Help { get; set; }

        [Option(shortName: OptionShortNames.Verbosity, longName: OptionNames.Verbosity,
            HelpText = "The amount of information to display in the log.",
            MetaValue = MetaValues.Verbosity)]
        public string Verbosity { get; set; }
    }
}
