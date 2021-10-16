// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandLine;
using Orang.CommandLine.Annotations;

namespace Orang.CommandLine
{
    internal abstract class CommonRegexCommandLineOptions : BaseCommandLineOptions
    {
        [Option(
            longName: OptionNames.ContentMode,
            HelpText = "Defines which parts of a content should be included in the results.",
            MetaValue = MetaValues.ContentMode)]
        public string ContentMode { get; set; } = null!;

        [Option(
#if DEBUG
            shortName: OptionShortNames.Summary,
#endif
            longName: OptionNames.Summary,
            HelpText = "Show summary at the end of search.")]
        public bool Summary { get; set; }
#if DEBUG
        [Option(
            longName: OptionNames.ContentIndent,
            HelpText = "Indentation for each line of a content.",
            MetaValue = MetaValues.Indent)]
        public string ContentIndent { get; set; } = null!;

        [Option(
            longName: OptionNames.ContentSeparator,
            HelpText = "Value that separates each match or matching line.",
            MetaValue = MetaValues.Separator)]
        public string ContentSeparator { get; set; } = null!;
#endif
        [HideFromHelp]
        [Option(
            shortName: OptionShortNames.Display,
            longName: OptionNames.Display,
            HelpText = "Display of the results.",
            MetaValue = MetaValues.DisplayOptions)]
        public IEnumerable<string> Display { get; set; } = null!;

        [Option(
            shortName: OptionShortNames.Highlight,
            longName: OptionNames.Highlight,
            HelpText = "Parts of the output to highlight.",
            MetaValue = MetaValues.Highlight)]
        public IEnumerable<string> Highlight { get; set; } = null!;

        [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.")]
        public bool TryParse(CommonRegexCommandOptions options, ParseContext context)
        {
            return true;
        }
    }
}
