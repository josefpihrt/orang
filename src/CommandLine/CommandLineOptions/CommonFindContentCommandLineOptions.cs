// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using CommandLine;
using static Orang.CommandLine.ParseHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal abstract class CommonFindContentCommandLineOptions : CommonFindCommandLineOptions
    {
        [Option(longName: OptionNames.Ask,
            HelpText = "Ask for permission after each file or value.",
            MetaValue = MetaValues.AskMode)]
        public string Ask { get; set; }

        [Option(shortName: OptionShortNames.MaxCount, longName: OptionNames.MaxCount,
            HelpText = "Stop searching after specified number is reached.",
            MetaValue = MetaValues.MaxOptions)]
        public IEnumerable<string> MaxCount { get; set; }

        [Option(shortName: OptionShortNames.Name, longName: OptionNames.Name,
            HelpText = "Regular expression for file or directory name. Syntax is <PATTERN> [<PATTERN_OPTIONS>].",
            MetaValue = MetaValues.Regex)]
        public IEnumerable<string> Name { get; set; }

        public bool TryParse(ref CommonFindContentCommandOptions options)
        {
            var baseOptions = (CommonFindCommandOptions)options;

            if (!TryParse(ref baseOptions))
                return false;

            options = (CommonFindContentCommandOptions)baseOptions;

            if (!TryParseAsEnum(Ask, OptionNames.Ask, out AskMode askMode, defaultValue: AskMode.None, OptionValueProviders.AskModeProvider))
                return false;

            if (askMode == AskMode.Value
                && ConsoleOut.Verbosity < Orang.Verbosity.Normal)
            {
                WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Ask)}' cannot have value '{OptionValueProviders.AskModeProvider.GetValue(nameof(AskMode.Value)).HelpValue}' when '{OptionNames.GetHelpText(OptionNames.Verbosity)}' is set to '{OptionValueProviders.VerbosityProvider.GetValue(ConsoleOut.Verbosity.ToString()).HelpValue}'.");
                return false;
            }

            Filter nameFilter = null;

            if (Name.Any()
                && !TryParseFilter(Name, OptionNames.Name, out nameFilter))
            {
                return false;
            }

            options.AskMode = askMode;
            options.NameFilter = nameFilter;

            return true;
        }
    }
}
