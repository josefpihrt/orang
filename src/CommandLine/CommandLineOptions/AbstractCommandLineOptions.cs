// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;

namespace Orang.CommandLine
{
    internal abstract class AbstractCommandLineOptions
    {
        [Option(shortName: OptionShortNames.Output, longName: OptionNames.Output,
            HelpText = "Path to a file that should store output. Syntax is <PATH> [<OUTPUT_OPTIONS>].",
            MetaValue = MetaValues.OutputOptions)]
        public IEnumerable<string> Output { get; set; }

        [Option(shortName: OptionShortNames.Verbosity, longName: OptionNames.Verbosity,
            HelpText = "The amount of information to display in the log.",
            MetaValue = MetaValues.Verbosity)]
        public string Verbosity { get; set; }
    }
}
