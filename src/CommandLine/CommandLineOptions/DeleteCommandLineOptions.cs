// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommandLine;
using static Orang.CommandLine.ParseHelpers;

namespace Orang.CommandLine
{
    [Verb("delete", HelpText = "Deletes files and directories.")]
    [OptionValueProvider(nameof(Content), OptionValueProviderNames.PatternOptionsWithoutPart)]
    [OptionValueProvider(nameof(Highlight), OptionValueProviderNames.DeleteHighlightOptions)]
    internal class DeleteCommandLineOptions : CommonFindCommandLineOptions
    {
        [Option(longName: OptionNames.Ask,
            HelpText = "Ask for a permission to delete file or directory.")]
        public bool Ask { get; set; }

        [Option(shortName: OptionShortNames.Content, longName: OptionNames.Content,
            HelpText = "Regular expression for files' content. Syntax is <PATTERN> [<PATTERN_OPTIONS>].",
            MetaValue = MetaValues.Regex)]
        public IEnumerable<string> Content { get; set; }

        [Option(longName: OptionNames.ContentOnly,
            HelpText = "Delete content of a file or directory but not the file or directory itself.")]
        public bool ContentOnly { get; set; }

        [Option(shortName: OptionShortNames.DryRun, longName: OptionNames.DryRun,
            HelpText = "Display which files or directories should be deleted but do not actually delete any file or directory.")]
        public bool DryRun { get; set; }

        [Option(longName: OptionNames.IncludingBom,
            HelpText = "Delete byte order mark (BOM) when deleting file's content.")]
        public bool IncludingBom { get; set; }

        [Option(shortName: OptionShortNames.MaxCount, longName: OptionNames.MaxCount,
            HelpText = "Stop renaming after specified number is reached.",
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

        public bool TryParse(ref DeleteCommandOptions options)
        {
            var baseOptions = (CommonFindCommandOptions)options;

            if (!TryParse(ref baseOptions))
                return false;

            options = (DeleteCommandOptions)baseOptions;

            if (!TryParseAsEnumFlags(Highlight, OptionNames.Highlight, out HighlightOptions highlightOptions, defaultValue: HighlightOptions.Match, provider: OptionValueProviders.DeleteHighlightOptionsProvider))
                return false;

            if (!TryParseFilter(Name, OptionNames.Name, out Filter nameFilter))
                return false;

            Filter contentFilter = null;

            if (Content.Any()
                && !TryParseFilter(Content, OptionNames.Content, out contentFilter))
            {
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
            options.Ask = Ask;
            options.DryRun = DryRun;
            options.HighlightOptions = highlightOptions;
            options.SearchTarget = GetSearchTarget();
            options.NameFilter = nameFilter;
            options.ContentFilter = contentFilter;
            options.ContentOnly = ContentOnly;
            options.IncludingBom = IncludingBom;
            options.MaxMatchingFiles = MaxCount;
            options.Output = outputOptions;

            return true;
        }
    }
}
