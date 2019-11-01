// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CommandLine;

namespace Orang.CommandLine
{
    internal abstract class CommonListCommandLineOptions
    {
        [Option(shortName: OptionShortNames.Filter, longName: OptionNames.Filter,
            HelpText = "Filter string that should be used to filter results.",
            MetaValue = "<FILTER>")]
        public string Filter { get; set; }

        public bool TryParse(ref CommonListCommandOptions options)
        {
            options.Filter = Filter;

            return true;
        }
    }
}
