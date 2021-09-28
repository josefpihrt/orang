// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;
using Orang.CommandLine.Annotations;
using static Orang.CommandLine.ParseHelpers;

namespace Orang.CommandLine
{
    [Verb("regex-match", HelpText = "Searches the input string for occurrences of the regular expression.")]
    [OptionValueProvider(nameof(Content), OptionValueProviderNames.PatternOptions_Match)]
    [OptionValueProvider(nameof(Highlight), OptionValueProviderNames.MatchHighlightOptions)]
    [CommandGroup("Regex", 2)]
    [CommandAlias("regex match")]
    internal sealed class RegexMatchCommandLineOptions : RegexCommandLineOptions
    {
        [Option(
            shortName: OptionShortNames.Content,
            longName: OptionNames.Content,
            Required = true,
            HelpText = "Regular expression for the input string.",
            MetaValue = MetaValues.Regex)]
        public IEnumerable<string> Content { get; set; } = null!;

        [Option(
            shortName: OptionShortNames.MaxCount,
            longName: OptionNames.MaxCount,
            Default = -1,
            HelpText = "Maximum number of matches returned.",
            MetaValue = MetaValues.Num)]
        public int MaxCount { get; set; }

        public bool TryParse(RegexMatchCommandOptions options)
        {
            var baseOptions = (RegexCommandOptions)options;

            if (!TryParse(baseOptions))
                return false;

            options = (RegexMatchCommandOptions)baseOptions;

            if (!FilterParser.TryParse(
                Content,
                OptionNames.Content,
                OptionValueProviders.PatternOptions_Match_Provider,
                out Filter? filter))
            {
                return false;
            }

            if (!TryParseHighlightOptions(
                Highlight,
                out HighlightOptions highlightOptions,
                defaultValue: HighlightOptions.Default,
                provider: OptionValueProviders.MatchHighlightOptionsProvider))
            {
                return false;
            }

            options.Filter = filter!;
            options.HighlightOptions = highlightOptions;
            options.MaxCount = MaxCount;

            return true;
        }
    }
}
