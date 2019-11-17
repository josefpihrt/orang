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

        [Option(shortName: OptionShortNames.Output, longName: OptionNames.Output,
            HelpText = "Path to a file that should store results. Syntax is <PATH> [<OUTPUT_OPTIONS>].",
            MetaValue = MetaValues.OutputOptions)]
        public IEnumerable<string> Output { get; set; }

        public bool TryParse(ref FindCommandOptions options)
        {
            var baseOptions = (CommonFindContentCommandOptions)options;

            if (!TryParse(ref baseOptions))
                return false;

            options = (FindCommandOptions)baseOptions;

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
                includeSummary: out bool includeSummary,
                includeCount: out bool includeCount,
                defaultContentDisplayStyle: ContentDisplayStyle.Line,
                defaultPathDisplayStyle: PathDisplayStyle.Full,
                contentDisplayStyleProvider: OptionValueProviders.ContentDisplayStyleProvider,
                pathDisplayStyleProvider: (contentFilter != null) ? OptionValueProviders.PathDisplayStyleProvider : OptionValueProviders.PathDisplayStyleProvider_WithoutOmit))
            {
                return false;
            }

            if (options.AskMode == AskMode.Value
                && (contentDisplayStyle == ContentDisplayStyle.AllLines || contentDisplayStyle == ContentDisplayStyle.UnmatchedLines))
            {
                WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Display)}' cannot have value '{OptionValueProviders.ContentDisplayStyleProvider.GetValue(contentDisplayStyle.ToString()).HelpValue}' when option '{OptionNames.GetHelpText(OptionNames.Ask)}' has value '{OptionValueProviders.AskModeProvider.GetValue(nameof(AskMode.Value)).HelpValue}'.");
                return false;
            }

            if (!TryParseOutputOptions(Output, OptionNames.Output, out OutputOptions outputOptions))
                return false;

            if (!TryParseMaxCount(MaxCount, out int maxCount, out int maxMatches, out int maxMatchingFiles))
                return false;

            int maxMatchesInFile;

            if (contentFilter != null)
            {
                maxMatchesInFile = maxCount;
            }
            else
            {
                maxMatchesInFile = 0;
                maxMatches = 0;
                maxMatchingFiles = (maxCount > 0) ? maxCount : maxMatchingFiles;
            }

            options.Format = new OutputDisplayFormat(contentDisplayStyle: contentDisplayStyle, pathDisplayStyle: pathDisplayStyle, lineOptions: LineDisplayOptions, includeSummary: includeSummary, includeCount: includeCount);
            options.HighlightOptions = highlightOptions;
            options.SearchTarget = GetSearchTarget();
            options.ContentFilter = contentFilter;
            options.Output = outputOptions;

            options.MaxMatchesInFile = maxMatchesInFile;
            options.MaxMatches = maxMatches;
            options.MaxMatchingFiles = maxMatchingFiles;

            return true;
        }
    }
}
