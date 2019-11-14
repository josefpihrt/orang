// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;

namespace Orang.CommandLine
{
    internal abstract class CommonRegexCommandLineOptions : AbstractCommandLineOptions
    {
        [Option(shortName: OptionShortNames.Display, longName: OptionNames.Display,
            HelpText = "Display of the results.",
            MetaValue = MetaValues.DisplayOptions)]
        public IEnumerable<string> Display { get; set; }

        public bool TryParse(ref CommonRegexCommandOptions options)
        {
            return true;
        }
    }
}
