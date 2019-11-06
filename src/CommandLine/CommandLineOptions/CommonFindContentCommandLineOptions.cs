// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using CommandLine;
using static Orang.CommandLine.ParseHelpers;

namespace Orang.CommandLine
{
    internal abstract class CommonFindContentCommandLineOptions : CommonFindCommandLineOptions
    {
        [Option(shortName: OptionShortNames.LineNumber, longName: OptionNames.LineNumber,
            HelpText = "Include line number.")]
        public bool LineNumber { get; set; }

        [Option(shortName: OptionShortNames.MaxCount, longName: OptionNames.MaxCount,
            HelpText = "Stop searching after specified number is reached.",
            MetaValue = MetaValues.MaxOptions)]
        public IEnumerable<string> MaxCount { get; set; }

        [Option(shortName: OptionShortNames.Name, longName: OptionNames.Name,
            HelpText = "Regular expression for file or directory name. Syntax is <PATTERN> [<PATTERN_OPTIONS>].",
            MetaValue = MetaValues.Regex)]
        public IEnumerable<string> Name { get; set; }

        internal LineDisplayOptions LineDisplayOptions => (LineNumber) ? LineDisplayOptions.IncludeLineNumber : LineDisplayOptions.None;

        public bool TryParse(ref CommonFindContentCommandOptions options)
        {
            var baseOptions = (CommonFindCommandOptions)options;

            if (!TryParse(ref baseOptions))
                return false;

            options = (CommonFindContentCommandOptions)baseOptions;

            int maxMatches = 0;
            int maxMatchesInFile = 0;
            int maxMatchingFiles = 0;

            if (MaxCount.Any()
                && !TryParseMaxCount(MaxCount, out maxMatches, out maxMatchesInFile, out maxMatchingFiles))
            {
                return false;
            }

            Filter nameFilter = null;

            if (Name.Any()
                && !TryParseFilter(Name, OptionNames.Name, out nameFilter))
            {
                return false;
            }

            options.NameFilter = nameFilter;
            options.MaxMatches = maxMatches;
            options.MaxMatchesInFile = maxMatchesInFile;
            options.MaxMatchingFiles = maxMatchingFiles;

            return true;
        }
    }
}
