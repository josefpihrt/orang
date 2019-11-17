// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using static Orang.CommandLine.ParseHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    [Verb("replace", HelpText = "Searches the file system for files and replaces its content.")]
    internal class ReplaceCommandLineOptions : CommonFindContentCommandLineOptions
    {
        [Option(shortName: OptionShortNames.Content, longName: OptionNames.Content,
            Required = true,
            HelpText = "Regular expression for files' content. Syntax is <PATTERN> [<PATTERN_OPTIONS>].",
            MetaValue = MetaValues.Regex)]
        [OptionValueProvider(OptionValueProviderNames.PatternOptionsWithoutGroupAndPartAndNegative)]
        public IEnumerable<string> Content { get; set; }

        [Option(shortName: OptionShortNames.DryRun, longName: OptionNames.DryRun,
            HelpText = "Display which files should be updated but do not actually update any file.")]
        public bool DryRun { get; set; }

        [Option(longName: OptionNames.Evaluator,
            HelpText = "Path to the evaluator method to compute replacements. The format is \"LibraryPath,FullTypeName.MethodName\".",
            MetaValue = MetaValues.Evaluator)]
        public string Evaluator { get; set; }

        [Option(shortName: OptionShortNames.Highlight, longName: OptionNames.Highlight,
            HelpText = "Parts of the output to highlight.",
            MetaValue = MetaValues.Highlight)]
        [OptionValueProvider(OptionValueProviderNames.ReplaceHighlightOptions)]
        public IEnumerable<string> Highlight { get; set; }

        [Option(longName: OptionNames.Input,
            HelpText = "Text to search.",
            MetaValue = MetaValues.Input)]
        public string Input { get; set; }

        [Option(shortName: OptionShortNames.Replacement, longName: OptionNames.Replacement,
            HelpText = "Replacement pattern. Syntax is <REPLACEMENT> [<REPLACEMENT_OPTIONS>].",
            MetaValue = MetaValues.Replacement)]
        public IEnumerable<string> Replacement { get; set; }

        public bool TryParse(ref ReplaceCommandOptions options)
        {
            var baseOptions = (CommonFindContentCommandOptions)options;

            if (!TryParse(ref baseOptions))
                return false;

            options = (ReplaceCommandOptions)baseOptions;

            if (!TryParseAsEnumFlags(Highlight, OptionNames.Highlight, out HighlightOptions highlightOptions, defaultValue: HighlightOptions.Replacement, provider: OptionValueProviders.ReplaceHighlightOptionsProvider))
                return false;

            if (!TryParseFilter(Content, OptionNames.Content, out Filter contentFilter, provider: OptionValueProviders.PatternOptionsWithoutGroupAndPartAndNegativeProvider))
                return false;

            if (!TryParseReplacement(Replacement, out string replacement))
                return false;

            if (!DelegateFactory.TryCreateMatchEvaluator(Evaluator, out MatchEvaluator matchEvaluator))
                return false;

            if (replacement != null && matchEvaluator != null)
            {
                WriteError($"Options '{OptionNames.GetHelpText(OptionNames.Replacement)}' and '{OptionNames.GetHelpText(OptionNames.Evaluator)}' cannot be set both at the same time.");
                return false;
            }

            if (!TryParseMaxCount(MaxCount, out int maxMatchesInFile, out int maxMatches, out int maxMatchingFiles))
                return false;

            ContentDisplayStyle contentDisplayStyle;
            PathDisplayStyle pathDisplayStyle;

            if (!TryParseDisplay(
                values: Display,
                optionName: OptionNames.Display,
                contentDisplayStyle: out ContentDisplayStyle? contentDisplayStyle2,
                pathDisplayStyle: out PathDisplayStyle? pathDisplayStyle2,
                includeSummary: out bool includeSummary,
                includeCount: out bool includeCount,
                contentDisplayStyleProvider: OptionValueProviders.ContentDisplayStyleProvider_WithoutUnmatchedLines,
                pathDisplayStyleProvider: OptionValueProviders.PathDisplayStyleProvider))
            {
                return false;
            }

            if (contentDisplayStyle2 != null)
            {
                if (options.AskMode == AskMode.Value
                    && contentDisplayStyle2 == ContentDisplayStyle.AllLines)
                {
                    WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Format)}' cannot have value '{OptionValueProviders.ContentDisplayStyleProvider.GetValue(nameof(ContentDisplayStyle.AllLines)).HelpValue}' when option '{OptionNames.GetHelpText(OptionNames.Ask)}' has value '{OptionValueProviders.AskModeProvider.GetValue(nameof(AskMode.Value)).HelpValue}'.");
                    return false;
                }

                contentDisplayStyle = contentDisplayStyle2.Value;
            }
            else if (Input != null)
            {
                contentDisplayStyle = ContentDisplayStyle.AllLines;
            }
            else
            {
                contentDisplayStyle = ContentDisplayStyle.Line;
            }

            pathDisplayStyle = pathDisplayStyle2 ?? PathDisplayStyle.Full;

            options.Format = new OutputDisplayFormat(contentDisplayStyle: contentDisplayStyle, pathDisplayStyle: pathDisplayStyle, lineOptions: LineDisplayOptions, includeSummary: includeSummary, includeCount: includeCount);
            options.HighlightOptions = highlightOptions;
            options.ContentFilter = contentFilter;
            options.Replacement = replacement ?? "";
            options.MatchEvaluator = matchEvaluator;
            options.Input = Input;
            options.DryRun = DryRun;
            options.MaxMatchesInFile = maxMatchesInFile;
            options.MaxMatches = maxMatches;
            options.MaxMatchingFiles = maxMatchingFiles;

            return true;
        }

        protected override bool TryParsePaths(out ImmutableArray<string> paths)
        {
            if (Path.Any()
                && Input != null)
            {
                WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Input)}' and argument '{ArgumentMetaNames.Path}' cannot be set both at the same time.");
                return false;
            }

            return base.TryParsePaths(out paths);
        }
    }
}
