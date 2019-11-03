// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using CommandLine;
using static Orang.CommandLine.ParseHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    [Verb("find", HelpText = "Searches the file system for files and directories and optionally searches files' content.")]
    internal class FindCommandLineOptions : CommonFindContentCommandLineOptions
    {
        [Option(longName: OptionNames.Ask,
            HelpText = "Ask for continuation after each file or value.",
            MetaValue = MetaValues.AskMode)]
        public string Ask { get; set; }

        [Option(shortName: OptionShortNames.Content, longName: OptionNames.Content,
            HelpText = "Regular expression for files' content. Syntax is <PATTERN> [<PATTERN_OPTIONS>].",
            MetaValue = MetaValues.Regex)]
        [OptionValueProvider(OptionValueProviderNames.PatternOptionsWithoutPart)]
        public IEnumerable<string> Content { get; set; }

        [Option(shortName: OptionShortNames.Highlight, longName: OptionNames.Highlight,
            HelpText = "Parts of the output to highlight.",
            MetaValue = MetaValues.Highlight)]
        [OptionValueProvider(OptionValueProviderNames.FindHighlightOptions)]
        public IEnumerable<string> Highlight { get; set; }

        public bool TryParse(ref FindCommandOptions options)
        {
            var baseOptions = (CommonFindContentCommandOptions)options;

            if (!TryParse(ref baseOptions))
                return false;

            options = (FindCommandOptions)baseOptions;

            if (!TryParseAsEnum(Ask, OptionNames.Ask, out AskMode askMode, defaultValue: AskMode.None, OptionValueProviders.AskModeProvider))
                return false;

            if (askMode == AskMode.Value
                && ConsoleOut.Verbosity < Orang.Verbosity.Normal)
            {
                WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Ask)}' cannot have value '{OptionValueProviders.AskModeProvider.GetValue(nameof(AskMode.Value)).HelpValue}' when '{OptionNames.GetHelpText(OptionNames.Verbosity)}' is set to '{OptionValueProviders.VerbosityProvider.GetValue(ConsoleOut.Verbosity.ToString()).HelpValue}'.");
                return false;
            }

            if (!TryParseAsEnumFlags(Highlight, OptionNames.Highlight, out HighlightOptions highlightOptions, defaultValue: HighlightOptions.Match, provider: OptionValueProviders.FindHighlightOptionsProvider))
                return false;

            Filter contentFilter = null;

            if (Content.Any()
                && !TryParseFilter(Content, OptionNames.Content, out contentFilter, provider: OptionValueProviders.PatternOptionsWithoutPartProvider))
            {
                return false;
            }

            if (!TryParseDisplay(
                values: Display,
                optionName: OptionNames.Display,
                contentDisplayStyle: out ContentDisplayStyle contentDisplayStyle,
                pathDisplayStyle: out PathDisplayStyle pathDisplayStyle,
                defaultContentDisplayStyle: (askMode == AskMode.Value) ? ContentDisplayStyle.Value : ContentDisplayStyle.Line,
                defaultPathDisplayStyle: PathDisplayStyle.Relative,
                contentDisplayStyleProvider: OptionValueProviders.ContentDisplayStyleProvider,
                pathDisplayStyleProvider: (contentFilter != null) ? OptionValueProviders.PathDisplayStyleProvider : OptionValueProviders.PathDisplayStyleProvider_WithoutOmit))
            {
                return false;
            }

            if (askMode == AskMode.Value
                && (contentDisplayStyle == ContentDisplayStyle.AllLines || contentDisplayStyle == ContentDisplayStyle.UnmatchedLines))
            {
                WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Display)}' cannot have value '{OptionValueProviders.ContentDisplayStyleProvider.GetValue(contentDisplayStyle.ToString()).HelpValue}' when option '{OptionNames.GetHelpText(OptionNames.Ask)}' has value '{OptionValueProviders.AskModeProvider.GetValue(nameof(AskMode.Value)).HelpValue}'.");
                return false;
            }

            options.Format = new OutputDisplayFormat(contentDisplayStyle: contentDisplayStyle, pathDisplayStyle: pathDisplayStyle, lineOptions: LineDisplayOptions);

            options.AskMode = askMode;
            options.HighlightOptions = highlightOptions;
            options.SearchTarget = GetSearchTarget();
            options.ContentFilter = contentFilter;

            return true;
        }
    }
}
