// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using Orang.FileSystem;
using static Orang.CommandLine.ParseHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    [Verb("rename", HelpText = "Renames files and directories.")]
    [OptionValueProvider(nameof(Content), OptionValueProviderNames.PatternOptionsWithoutPart)]
    [OptionValueProvider(nameof(Highlight), OptionValueProviderNames.RenameHighlightOptions)]
    [OptionValueProvider(nameof(Name), OptionValueProviderNames.PatternOptionsWithoutGroupAndNegative)]
    internal class RenameCommandLineOptions : CommonFindCommandLineOptions
    {
        [Option(longName: OptionNames.Ask,
            HelpText = "Ask for a permission to rename file or directory.")]
        public bool Ask { get; set; }

        [Option(shortName: OptionShortNames.Content, longName: OptionNames.Content,
            HelpText = "Regular expression for files' content. Syntax is <PATTERN> [<PATTERN_OPTIONS>].",
            MetaValue = MetaValues.Regex)]
        public IEnumerable<string> Content { get; set; }

        [Option(shortName: OptionShortNames.DryRun, longName: OptionNames.DryRun,
            HelpText = "Display which files or directories should be renamed but do not actually rename any file or directory.")]
        public bool DryRun { get; set; }

        [Option(longName: OptionNames.Evaluator,
            HelpText = "Path to the evaluator method to compute replacements. The format is \"LibraryPath,FullTypeName.MethodName\".",
            MetaValue = MetaValues.Evaluator)]
        public string Evaluator { get; set; }

        [Option(shortName: OptionShortNames.MaxCount, longName: OptionNames.MaxCount,
            HelpText = "Stop deleting after specified number is reached.",
            MetaValue = MetaValues.Number)]
        public int MaxCount { get; set; }

        [Option(shortName: OptionShortNames.Name, longName: OptionNames.Name,
            Required = true,
            HelpText = "Regular expression for file or directory name. Syntax is <PATTERN> [<PATTERN_OPTIONS>].",
            MetaValue = MetaValues.Regex)]
        public IEnumerable<string> Name { get; set; }

        [Option(shortName: OptionShortNames.Output, longName: OptionNames.Output,
            HelpText = "Path to a file that should store results. Syntax is <PATH> [<OUTPUT_OPTIONS>].",
            MetaValue = MetaValues.OutputOptions)]
        public IEnumerable<string> Output { get; set; }

        [Option(shortName: OptionShortNames.Replacement, longName: OptionNames.Replacement,
            HelpText = "Replacement pattern. Syntax is <REPLACEMENT> [<REPLACEMENT_OPTIONS>].",
            MetaValue = MetaValues.Replacement)]
        public IEnumerable<string> Replacement { get; set; }

        public bool TryParse(ref RenameCommandOptions options)
        {
            var baseOptions = (CommonFindCommandOptions)options;

            if (!TryParse(ref baseOptions))
                return false;

            options = (RenameCommandOptions)baseOptions;

            if (!TryParseAsEnumFlags(Highlight, OptionNames.Highlight, out HighlightOptions highlightOptions, defaultValue: HighlightOptions.Match | HighlightOptions.Replacement, provider: OptionValueProviders.RenameHighlightOptionsProvider))
                return false;

            if (!TryParseFilter(Name, OptionNames.Name, out Filter nameFilter, provider: OptionValueProviders.PatternOptionsWithoutGroupAndNegativeProvider))
                return false;

            if (nameFilter.NamePart == NamePartKind.FullName)
            {
                WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Options)}' has invalid value '{OptionValueProviders.NamePartKindProvider.GetValue(nameof(NamePartKind.FullName)).HelpValue}'.");
                return false;
            }

            Filter contentFilter = null;

            if (Content.Any()
                && !TryParseFilter(Content, OptionNames.Content, out contentFilter))
            {
                return false;
            }

            if (!TryParseReplacement(Replacement, out string replacement))
                return false;

            if (!DelegateFactory.TryCreateMatchEvaluator(Evaluator, out MatchEvaluator matchEvaluator))
                return false;

            if (replacement != null && matchEvaluator != null)
            {
                WriteError($"Options '{OptionNames.GetHelpText(OptionNames.Replacement)}' and '{OptionNames.GetHelpText(OptionNames.Evaluator)}' cannot be set both at the same time.");
                return false;
            }

            if (!TryParseOutputOptions(Output, OptionNames.Output, out OutputOptions outputOptions))
                return false;

            if (!TryParseDisplay(
                values: Display,
                optionName: OptionNames.Display,
                contentDisplayStyle: out ContentDisplayStyle _,
                pathDisplayStyle: out PathDisplayStyle pathDisplayStyle,
                displayParts: out DisplayParts displayParts,
                fileProperties: out ImmutableArray<FileProperty> fileProperties,
                defaultContentDisplayStyle: 0,
                defaultPathDisplayStyle: PathDisplayStyle.Full,
                contentDisplayStyleProvider: OptionValueProviders.ContentDisplayStyleProvider,
                pathDisplayStyleProvider: OptionValueProviders.PathDisplayStyleProvider))
            {
                return false;
            }

            if (pathDisplayStyle == PathDisplayStyle.Relative
                && options.SortOptions != null)
            {
                pathDisplayStyle = PathDisplayStyle.Full;
            }

            options.Format = new OutputDisplayFormat(ContentDisplayStyle.None, pathDisplayStyle, displayParts: displayParts, fileProperties: fileProperties);
            options.HighlightOptions = highlightOptions;
            options.SearchTarget = GetSearchTarget();
            options.Replacement = replacement ?? "";
            options.Ask = Ask;
            options.DryRun = DryRun;
            options.NameFilter = nameFilter;
            options.ContentFilter = contentFilter;
            options.MatchEvaluator = matchEvaluator;
            options.MaxMatchingFiles = MaxCount;
            options.Output = outputOptions;

            return true;
        }
    }
}
