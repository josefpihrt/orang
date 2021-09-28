// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.FileSystem;
using Orang.Text.RegularExpressions;
using static Orang.CommandLine.ParseHelpers;

namespace Orang.CommandLine
{
    [Verb("delete", HelpText = "Deletes files and directories.")]
    [OptionValueProvider(nameof(Content), OptionValueProviderNames.PatternOptionsWithoutPart)]
    [OptionValueProvider(nameof(Display), OptionValueProviderNames.Display_NonContent)]
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

        public bool TryParse(DeleteCommandOptions options)
        {
            var baseOptions = (DeleteOrRenameCommandOptions)options;

            if (!TryParse(baseOptions))
                return false;

            options = (DeleteCommandOptions)baseOptions;

            if (!TryParseHighlightOptions(
                Highlight,
                out HighlightOptions highlightOptions,
                defaultValue: HighlightOptions.Default,
                provider: OptionValueProviders.DeleteHighlightOptionsProvider))
            {
                return false;
            }

            if (!FilterParser.TryParse(
                Name,
                OptionNames.Name,
                OptionValueProviders.PatternOptionsProvider,
                out Filter? nameFilter,
                out FileNamePart namePart,
                allowNull: true))
            {
                return false;
            }

            if (nameFilter == null
                && options.Paths.Length == 1
                && options.Paths[0].Origin == PathOrigin.CurrentDirectory)
            {
                Logger.WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Name)}' "
                    + "is required when no path is specified (i.e. current directory is used).");
                return false;
            }

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

            options.Format = new OutputDisplayFormat(
                contentDisplayStyle: contentDisplayStyle
                    ?? ((ReferenceEquals(contentFilter, Filter.EntireInput)) ? ContentDisplayStyle.AllLines : ContentDisplayStyle.Omit),
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
}
