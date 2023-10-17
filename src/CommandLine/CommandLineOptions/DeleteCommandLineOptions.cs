// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.FileSystem;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

[Verb("delete", HelpText = "Deletes files and directories.")]
[OptionValueProvider(nameof(Content), OptionValueProviderNames.PatternOptionsWithoutPart)]
[OptionValueProvider(nameof(Highlight), OptionValueProviderNames.DeleteHighlightOptions)]
[CommandGroup("File System", 1)]
internal sealed class DeleteCommandLineOptions : DeleteOrRenameCommandLineOptions
{
    [Option(
        longName: OptionNames.Ask,
        HelpText = "Ask for a permission to delete file or directory.")]
    public bool Ask { get; set; }

    [Option(
        shortName: OptionShortNames.Content,
        longName: OptionNames.Content,
        HelpText = "Regular expression for files' content.",
        MetaValue = MetaValues.Regex)]
    public IEnumerable<string> Content { get; set; } = null!;

    [Option(
        longName: OptionNames.ContentOnly,
        HelpText = "Delete content of a file or directory but not the file or directory itself.")]
    public bool ContentOnly { get; set; }

    [Option(
        shortName: OptionShortNames.DryRun,
        longName: OptionNames.DryRun,
        HelpText = "Display which files/directories should be deleted "
            + "but do not actually delete any file/directory.")]
    public bool DryRun { get; set; }

    [Option(
        longName: OptionNames.IncludingBom,
        HelpText = "Delete byte order mark (BOM) when deleting file's content.")]
    public bool IncludingBom { get; set; }

    [Option(
        shortName: OptionShortNames.MaxCount,
        longName: OptionNames.MaxCount,
        HelpText = "Stop deleting after specified number is reached.",
        MetaValue = MetaValues.Num)]
    public int MaxCount { get; set; }

    [Option(
        shortName: OptionShortNames.Name,
        longName: OptionNames.Name,
        HelpText = "Regular expression for file or directory name.",
        MetaValue = MetaValues.Regex)]
    public IEnumerable<string> Name { get; set; } = null!;

    public bool TryParse(DeleteCommandOptions options, ParseContext context)
    {
        var baseOptions = (DeleteOrRenameCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (DeleteCommandOptions)baseOptions;

        if (!context.TryParseHighlightOptions(
            Highlight,
            out HighlightOptions highlightOptions,
            defaultValue: HighlightOptions.Default,
            provider: OptionValueProviders.DeleteHighlightOptionsProvider))
        {
            return false;
        }

        if (!context.TryParseFilter(
            Name,
            OptionNames.Name,
            OptionValueProviders.PatternOptionsProvider,
            out Matcher? nameFilter,
            out FileNamePart namePart,
            allowNull: true))
        {
            return false;
        }

        if (nameFilter is null
            && options.Paths.Length == 1
            && options.Paths[0].Origin == PathOrigin.CurrentDirectory)
        {
            context.WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Name)}' "
                + "is required when no path is specified (i.e. current directory is used).");
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

        var displayParts = DisplayParts.None;
        var lineDisplayOptions = LineDisplayOptions.None;
        ContentDisplayStyle? contentDisplayStyle = null;
        PathDisplayStyle? pathDisplayStyle = null;
        LineContext lineContext = default;
        string? indent = null;
        string? separator = null;

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

        options.Format = new OutputDisplayFormat(
            contentDisplayStyle: contentDisplayStyle
                ?? ((ReferenceEquals(contentFilter, Matcher.EntireInput)) ? ContentDisplayStyle.AllLines : ContentDisplayStyle.Omit),
            pathDisplayStyle: pathDisplayStyle ?? PathDisplayStyle.Full,
            lineOptions: lineDisplayOptions,
            lineContext: lineContext,
            displayParts: displayParts,
            indent: indent,
            separator: separator);

        options.AskMode = (Ask) ? AskMode.File : AskMode.None;
        options.DryRun = DryRun;
        options.HighlightOptions = highlightOptions;
        options.SearchTarget = GetSearchTarget();
        options.NameFilter = nameFilter;
        options.NamePart = namePart;
        options.ContentFilter = contentFilter;
        options.ContentOnly = ContentOnly;
        options.IncludingBom = IncludingBom;
        options.MaxMatchingFiles = MaxCount;

        return true;
    }
}
