// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.FileSystem;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

[Verb("rename", HelpText = "Renames files and directories.")]
[OptionValueProvider(nameof(Content), OptionValueProviderNames.PatternOptionsWithoutPart)]
[OptionValueProvider(nameof(Display), OptionValueProviderNames.Display_NonContent)]
[OptionValueProvider(nameof(Highlight), OptionValueProviderNames.RenameHighlightOptions)]
[OptionValueProvider(nameof(Name), OptionValueProviderNames.PatternOptionsWithoutGroupAndNegative)]
[CommandGroup("File System", 1)]
internal sealed class RenameCommandLineOptions : DeleteOrRenameCommandLineOptions
{
    [Option(
        longName: OptionNames.Ask,
        HelpText = "Ask for a permission to rename file or directory.")]
    public bool Ask { get; set; }

    [Option(
        longName: OptionNames.Conflict,
        HelpText = "Defines how to resolve conflict when a file/directory already exists.",
        MetaValue = MetaValues.ConflictResolution)]
    public string Conflict { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.Content,
        longName: OptionNames.Content,
        HelpText = "Regular expression for files' content.",
        MetaValue = MetaValues.Regex)]
    public IEnumerable<string> Content { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.DryRun,
        longName: OptionNames.DryRun,
        HelpText = "Display which files/directories should be renamed "
            + "but do not actually rename any file/directory.")]
    public bool DryRun { get; set; }

    [HideFromHelp]
    [Option(
        longName: OptionNames.Evaluator,
        HelpText = "[deprecated] Use option -r, --replacement instead.",
        MetaValue = MetaValues.Evaluator)]
    public string Evaluator { get; set; } = null!;

    [Option(
        longName: OptionNames.Interactive,
        HelpText = "Enable editing of a new name.")]
    public bool Interactive { get; set; }

    [Option(
        shortName: OptionShortNames.MaxCount,
        longName: OptionNames.MaxCount,
        HelpText = "Stop renaming after specified number is reached.",
        MetaValue = MetaValues.Num)]
    public int MaxCount { get; set; }

    [HideFromConsoleHelp]
    [Option(
        longName: OptionNames.MaxMatchesInFile,
        HelpText = "Stop searching in a file name after specified number of matches is found.",
        MetaValue = MetaValues.Num)]
    public int MaxMatchesInFile { get; set; }

    [Option(
        longName: OptionNames.Modify,
        HelpText = "Functions to modify result.",
        MetaValue = MetaValues.ReplaceModify)]
    public IEnumerable<string> Modify { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.Name,
        longName: OptionNames.Name,
        Required = true,
        HelpText = "Regular expression for file or directory name.",
        MetaValue = MetaValues.Regex)]
    public IEnumerable<string> Name { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.Replacement,
        longName: OptionNames.Replacement,
        HelpText = "Replacement pattern.",
        MetaValue = MetaValues.Replacement)]
    public IEnumerable<string> Replacement { get; set; } = null!;

    public bool TryParse(RenameCommandOptions options, ParseContext context)
    {
        var baseOptions = (DeleteOrRenameCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (RenameCommandOptions)baseOptions;

        if (!context.TryParseHighlightOptions(
            Highlight,
            out HighlightOptions highlightOptions,
            defaultValue: HighlightOptions.Replacement,
            provider: OptionValueProviders.RenameHighlightOptionsProvider))
        {
            return false;
        }

        if (!context.TryParseFilter(
            Name,
            OptionNames.Name,
            OptionValueProviders.PatternOptionsWithoutGroupAndNegativeProvider,
            out Matcher? nameFilter,
            out FileNamePart namePart,
            namePartProvider: OptionValueProviders.NamePartKindProvider_WithoutFullName))
        {
            return false;
        }

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

        if (!context.TryParseReplacement(Replacement, out string? replacement, out MatchEvaluator? matchEvaluator))
            return false;

        if (matchEvaluator is null
            && Evaluator is not null)
        {
            context.WriteWarning($"Option '{OptionNames.GetHelpText(OptionNames.Evaluator)}' is has been deprecated "
                + "and will be removed in future version. "
                + $"Use option '{OptionNames.GetHelpText(OptionNames.Replacement)}' instead.");

            if (!DelegateFactory.TryCreateFromAssembly(Evaluator, typeof(string), typeof(Match), context.Logger, out matchEvaluator))
                return false;
        }

        if (!context.TryParseReplaceOptions(
            Modify,
            OptionNames.Modify,
            replacement,
            matchEvaluator,
            out Replacer? replacer))
        {
            return false;
        }

        if (!context.TryParseAsEnum(
            Conflict,
            OptionNames.Conflict,
            out ConflictResolution conflictResolution,
            defaultValue: ConflictResolution.Ask,
            provider: OptionValueProviders.ConflictResolutionProvider_WithoutSuffix))
        {
            return false;
        }

        if (!context.TryParseDisplay(
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
            pathDisplayStyleProvider: OptionValueProviders.PathDisplayStyleProvider_Rename))
        {
            return false;
        }

        if (ContentMode is not null)
        {
            if (context.TryParseAsEnum(
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

        if (PathMode is not null)
        {
            if (context.TryParseAsEnum(
                PathMode,
                OptionNames.PathMode,
                out PathDisplayStyle pathDisplayStyle2,
                provider: OptionValueProviders.PathDisplayStyleProvider_Rename))
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
        if (includeCreationTime)
            options.FilePropertyOptions = options.FilePropertyOptions.WithIncludeCreationTime(true);

        if (includeModifiedTime)
            options.FilePropertyOptions = options.FilePropertyOptions.WithIncludeModifiedTime(true);

        if (includeSize)
            options.FilePropertyOptions = options.FilePropertyOptions.WithIncludeSize(true);

        if (pathDisplayStyle == PathDisplayStyle.Relative
            && options.Paths.Length > 1
            && options.SortOptions is not null)
        {
            pathDisplayStyle = PathDisplayStyle.Full;
        }

        options.Format = new OutputDisplayFormat(
            contentDisplayStyle: contentDisplayStyle
                ?? ((ReferenceEquals(contentFilter, Matcher.EntireInput)) ? ContentDisplayStyle.AllLines : ContentDisplayStyle.Omit),
            pathDisplayStyle: pathDisplayStyle ?? PathDisplayStyle.Full,
            lineOptions: lineDisplayOptions,
            lineContext: lineContext,
            displayParts: displayParts,
            indent: indent,
            separator: separator);

        options.HighlightOptions = highlightOptions;
        options.SearchTarget = GetSearchTarget();
        options.Replacer = replacer;
        options.AskMode = (Ask) ? AskMode.File : AskMode.None;
        options.DryRun = DryRun;
        options.NameFilter = nameFilter;
        options.NamePart = namePart;
        options.ContentFilter = contentFilter;
        options.MaxMatchingFiles = MaxCount;
        options.MaxMatchesInFile = MaxMatchesInFile;
        options.ConflictResolution = conflictResolution;
        options.Interactive = Interactive;

        return true;
    }
}
