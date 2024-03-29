﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;
using Orang.CommandLine.Annotations;

namespace Orang.CommandLine;

[Verb(
    "regex-split",
    HelpText = "Splits the input string into an list of substrings at the positions defined by a regular expression.")]
[OptionValueProvider(nameof(Content), OptionValueProviderNames.PatternOptionsWithoutGroupAndPartAndNegative)]
[OptionValueProvider(nameof(Highlight), OptionValueProviderNames.SplitHighlightOptions)]
[CommandGroup("Regex", 2)]
[CommandAlias("regex split")]
internal sealed class RegexSplitCommandLineOptions : RegexCommandLineOptions
{
    [Option(
        shortName: OptionShortNames.MaxCount,
        longName: OptionNames.MaxCount,
        Default = 0,
        HelpText = "Maximum number of times the split can occur.",
        MetaValue = MetaValues.Num)]
    public int MaxCount { get; set; }

    [Option(
        longName: OptionNames.NoGroups,
        HelpText = "Do not include groups in the results.")]
    public bool NoGroups { get; set; }

    public bool TryParse(RegexSplitCommandOptions options, ParseContext context)
    {
        var baseOptions = (RegexCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (RegexSplitCommandOptions)baseOptions;

        if (!context.TryParseFilter(
            Content,
            OptionNames.Content,
            OptionValueProviders.PatternOptionsWithoutGroupAndPartAndNegativeProvider,
            out Matcher? matcher))
        {
            return false;
        }

        if (!context.TryParseHighlightOptions(
            Highlight,
            out HighlightOptions highlightOptions,
            defaultValue: HighlightOptions.Split,
            provider: OptionValueProviders.SplitHighlightOptionsProvider))
        {
            return false;
        }

        options.Matcher = matcher!;
        options.HighlightOptions = highlightOptions;
        options.OmitGroups = NoGroups;
        options.MaxCount = MaxCount;

        return true;
    }
}
