// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

[OptionValueProvider(nameof(Content), OptionValueProviderNames.PatternOptionsWithoutPart)]
[OptionValueProvider(nameof(Highlight), OptionValueProviderNames.FindHighlightOptions)]
internal abstract class CommonFindCommandLineOptions : FileSystemCommandLineOptions
{
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

    public bool TryParse(CommonFindCommandOptions options, ParseContext context)
    {
        var baseOptions = (FileSystemCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (CommonFindCommandOptions)baseOptions;

        if (!context.TryParseFilter(
            Content,
            OptionNames.Content,
            OptionValueProviders.PatternOptionsWithoutPartProvider,
            out Matcher? contentFilter,
            allowNull: true,
            allowEmptyPattern: true))
        {
            return false;
        }

        var displayParts = DisplayParts.None;
        var lineDisplayOptions = LineDisplayOptions.None;
        ContentDisplayStyle? contentDisplayStyle = null;
        PathDisplayStyle? pathDisplayStyle = null;
        LineContext lineContext = default;
#if DEBUG
        string? indent = null;
        string? separator = null;
#else
        const string? indent = null;
        const string? separator = null;
#endif
        if (ContentMode is not null)
        {
            if (context.TryParseAsEnum(
                ContentMode,
                OptionNames.ContentMode,
                out ContentDisplayStyle contentDisplayStyle2,
                provider: OptionValueProviders.ContentDisplayStyleProvider))
            {
                if (!VerifyContentDisplayStyle(contentDisplayStyle2, context))
                    return false;

                contentDisplayStyle = contentDisplayStyle2;
            }
            else
            {
                return false;
            }
        }

        if (PathMode is not null)
        {
            if (context.TryParseAsEnum(
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
        if (ContentIndent is not null)
            indent = RegexEscape.ConvertCharacterEscapes(ContentIndent);

        if (ContentSeparator is not null)
            separator = ContentSeparator;

        if (NoContent)
            contentDisplayStyle = ContentDisplayStyle.Omit;

        if (NoPath)
            pathDisplayStyle = PathDisplayStyle.Omit;
#endif
        if (pathDisplayStyle == PathDisplayStyle.Relative
            && options.Paths.Length > 1
            && options.SortOptions is not null)
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

            context.WriteError($"Option '{OptionNames.GetHelpText(OptionNames.ContentMode)}' cannot have value '{helpValue}' "
                + $"when option '{OptionNames.GetHelpText(OptionNames.Ask)}' has value '{helpValue2}'.");

            return false;
        }

        if (!context.TryParseHighlightOptions(
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
                ?? ((ReferenceEquals(contentFilter, Matcher.EntireInput)) ? ContentDisplayStyle.AllLines : GetDefaultContentDisplayStyle()),
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

        options.MaxMatchingFiles = (contentFilter is null && MaxMatchingFiles == 0) ? MaxCount : MaxMatchingFiles;

        return true;
    }

    public abstract ContentDisplayStyle GetDefaultContentDisplayStyle();

    public virtual bool VerifyContentDisplayStyle(ContentDisplayStyle contentDisplayStyle, ParseContext context)
    {
        return true;
    }
}
