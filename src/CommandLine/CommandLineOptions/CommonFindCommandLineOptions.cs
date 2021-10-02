// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.Text.RegularExpressions;
using static Orang.CommandLine.ParseHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    [OptionValueProvider(nameof(Content), OptionValueProviderNames.PatternOptionsWithoutPart)]
    [OptionValueProvider(nameof(Highlight), OptionValueProviderNames.FindHighlightOptions)]
    internal abstract class CommonFindCommandLineOptions : FileSystemCommandLineOptions
    {
        public abstract ContentDisplayStyle DefaultContentDisplayStyle { get; }

        [Option(
            shortName: OptionShortNames.Content,
            longName: OptionNames.Content,
            HelpText = "Regular expression for files' content.",
            MetaValue = MetaValues.Regex)]
        public IEnumerable<string> Content { get; set; } = null!;

        [Option(
            shortName: OptionShortNames.MaxCount,
            longName: OptionNames.MaxCount,
            HelpText = "Stop searching after specified number is reached.",
            MetaValue = MetaValues.Num)]
        public int MaxCount { get; set; }

        [HideFromConsoleHelp]
        [Option(
            longName: OptionNames.MaxMatchingFiles,
            HelpText = "Stop searching after specified number of files is found.",
            MetaValue = MetaValues.Num)]
        public int MaxMatchingFiles { get; set; }

        [HideFromConsoleHelp]
        [Option(
            longName: OptionNames.MaxMatchesInFile,
            HelpText = "Stop searching in a file after specified number of matches is found.",
            MetaValue = MetaValues.Num)]
        public int MaxMatchesInFile { get; set; }

        [Option(
            shortName: OptionShortNames.Name,
            longName: OptionNames.Name,
            HelpText = "Regular expression for file or directory name.",
            MetaValue = MetaValues.Regex)]
        public IEnumerable<string> Name { get; set; } = null!;

        public bool TryParse(CommonFindCommandOptions options)
        {
            var baseOptions = (FileSystemCommandOptions)options;

            if (!TryParse(baseOptions))
                return false;

            options = (CommonFindCommandOptions)baseOptions;

            if (!FilterParser.TryParse(
                Content,
                OptionNames.Content,
                OptionValueProviders.PatternOptionsWithoutPartProvider,
                out Filter? contentFilter,
                allowNull: true,
                allowEmptyPattern: true))
            {
                return false;
            }

            if (!TryParseDisplay(
                values: Display,
                optionName: OptionNames.Display,
                contentDisplayStyle: out ContentDisplayStyle? contentDisplayStyle,
                pathDisplayStyle: out PathDisplayStyle? pathDisplayStyle,
                lineDisplayOptions: out LineDisplayOptions lineDisplayOptions,
                lineContext: out LineContext lineContext,
                displayParts: out DisplayParts displayParts,
                includeCreationTime: out bool includeCreationTime,
                includeModifiedTime: out bool includeModifiedTime,
                includeSize: out bool includeSize,
                indent: out string? indent,
                separator: out string? separator,
                noAlign: out bool _,
                contentDisplayStyleProvider: OptionValueProviders.ContentDisplayStyleProvider,
                pathDisplayStyleProvider: OptionValueProviders.PathDisplayStyleProvider))
            {
                return false;
            }

            if (ContentMode != null)
            {
                if (TryParseAsEnum(
                    ContentMode,
                    OptionNames.ContentMode,
                    out ContentDisplayStyle contentDisplayStyle2,
                    provider: OptionValueProviders.ContentDisplayStyleProvider))
                {
                    contentDisplayStyle = contentDisplayStyle2;
                }
                else
                {
                    return false;
                }
            }

            if (PathMode != null)
            {
                if (TryParseAsEnum(
                    PathMode,
                    OptionNames.PathMode,
                    out PathDisplayStyle pathDisplayStyle2,
                    provider: OptionValueProviders.PathDisplayStyleProvider))
                {
                    pathDisplayStyle = pathDisplayStyle2;
                }
                else
                {
                    return false;
                }
            }

            if (Count)
                displayParts |= DisplayParts.Count;

            if (Summary)
                displayParts |= DisplayParts.Summary;

            if (Context >= 0)
                lineContext = new LineContext(Context);

            if (BeforeContext >= 0)
                lineContext = lineContext.WithBefore(BeforeContext);

            if (AfterContext >= 0)
                lineContext = lineContext.WithAfter(AfterContext);

            if (LineNumber)
                lineDisplayOptions |= LineDisplayOptions.IncludeLineNumber;
#if DEBUG
            if (ContentIndent != null)
                indent = RegexEscape.ConvertCharacterEscapes(ContentIndent);

            if (ContentSeparator != null)
                separator = ContentSeparator;

            if (NoContent)
                contentDisplayStyle = ContentDisplayStyle.Omit;

            if (NoPath)
                pathDisplayStyle = PathDisplayStyle.Omit;
#endif
            if (includeCreationTime)
                options.FilePropertyOptions = options.FilePropertyOptions.WithIncludeCreationTime(true);

            if (includeModifiedTime)
                options.FilePropertyOptions = options.FilePropertyOptions.WithIncludeModifiedTime(true);

            if (includeSize)
                options.FilePropertyOptions = options.FilePropertyOptions.WithIncludeSize(true);

            if (pathDisplayStyle == PathDisplayStyle.Relative
                && options.Paths.Length > 1
                && options.SortOptions != null)
            {
                pathDisplayStyle = PathDisplayStyle.Full;
            }

            if (options.AskMode == AskMode.Value
                && (contentDisplayStyle == ContentDisplayStyle.AllLines
                    || contentDisplayStyle == ContentDisplayStyle.UnmatchedLines
                    || contentDisplayStyle == ContentDisplayStyle.Omit))
            {
                string helpValue = OptionValueProviders.ContentDisplayStyleProvider
                    .GetValue(contentDisplayStyle.Value.ToString())
                    .HelpValue;

                string helpValue2 = OptionValueProviders.AskModeProvider.GetValue(nameof(AskMode.Value)).HelpValue;

                WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Display)}' cannot have value '{helpValue}' "
                    + $"when option '{OptionNames.GetHelpText(OptionNames.Ask)}' has value '{helpValue2}'.");

                return false;
            }

            if (!TryParseHighlightOptions(
                Highlight,
                out HighlightOptions highlightOptions,
                defaultValue: HighlightOptions.Default,
                contentDisplayStyle: contentDisplayStyle,
                provider: OptionValueProviders.FindHighlightOptionsProvider))
            {
                return false;
            }

            options.Format = new OutputDisplayFormat(
                contentDisplayStyle: contentDisplayStyle
                    ?? ((ReferenceEquals(contentFilter, Filter.EntireInput)) ? ContentDisplayStyle.AllLines : DefaultContentDisplayStyle),
                pathDisplayStyle: pathDisplayStyle ?? PathDisplayStyle.Full,
                lineOptions: lineDisplayOptions,
                lineContext: lineContext,
                displayParts: displayParts,
                indent: indent,
                separator: separator);

            options.HighlightOptions = highlightOptions;
            options.SearchTarget = GetSearchTarget();
            options.ContentFilter = contentFilter;
            options.MaxTotalMatches = MaxCount;
            options.MaxMatchesInFile = MaxMatchesInFile;
            options.MaxMatchingFiles = MaxMatchingFiles;

            return true;
        }
    }
}
