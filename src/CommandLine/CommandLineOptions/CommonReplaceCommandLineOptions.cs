// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.FileSystem;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

[OptionValueProvider(nameof(Content), OptionValueProviderNames.PatternOptionsWithoutGroupAndPartAndNegative)]
[OptionValueProvider(nameof(Highlight), OptionValueProviderNames.ReplaceHighlightOptions)]
internal abstract class CommonReplaceCommandLineOptions : FileSystemCommandLineOptions
{
    [Option(
        longName: OptionNames.Ask,
        HelpText = "Ask for permission after each file or value.",
        MetaValue = MetaValues.AskMode)]
    public string Ask { get; set; } = null!;

    public abstract IEnumerable<string> Content { get; set; }

    [Option(
        shortName: OptionShortNames.DryRun,
        longName: OptionNames.DryRun,
        HelpText = "Display which files should be updated but do not actually update any file.")]
    public bool DryRun { get; set; }

    [Option(
        longName: OptionNames.Interactive,
        HelpText = "Enable editing of a replacement.")]
    public bool Interactive { get; set; }

    [Option(
        longName: OptionNames.Pipe,
        HelpText = "Defines how to use redirected/piped input.",
        MetaValue = MetaValues.PipeMode)]
    public string Pipe { get; set; } = null!;

    [Option(
        longName: OptionNames.Input,
        HelpText = "The input string to be searched.",
        MetaValue = MetaValues.Input)]
    public IEnumerable<string> Input { get; set; } = null!;

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

    public bool TryParse(CommonReplaceCommandOptions options, ParseContext context)
    {
        PipeMode? pipeMode = null;

        if (!string.IsNullOrEmpty(Pipe))
        {
            if (!context.TryParseAsEnum(
                Pipe,
                OptionNames.Pipe,
                out PipeMode pipeMode2,
                CommandLine.PipeMode.None,
                OptionValueProviders.PipeMode))
            {
                return false;
            }

            pipeMode = pipeMode2;
        }

        string? redirectedInput = ConsoleHelpers.ReadRedirectedInput();

        if (pipeMode == CommandLine.PipeMode.Paths
            || pipeMode == CommandLine.PipeMode.Text)
        {
            if (redirectedInput is null)
            {
                context.WriteError("Redirected/piped input is required "
                    + $"when option '{OptionNames.GetHelpText(OptionNames.Pipe)}' is specified.");

                return false;
            }

            PipeMode = pipeMode;
        }

        if (!context.TryParseProperties(Ask, Name, options))
            return false;

        var baseOptions = (FileSystemCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (CommonReplaceCommandOptions)baseOptions;

        Matcher? contentFilter = null;
        if (!Content.Any())
        {
            contentFilter = GetDefaultContentFilter();
        }

        if (contentFilter is null
            && !context.TryParseFilter(
                Content,
                OptionNames.Content,
                OptionValueProviders.PatternOptionsWithoutGroupAndPartAndNegativeProvider,
                out contentFilter))
        {
            return false;
        }

        string? input = null;

        if (Input.Any()
            && !context.TryParseInput(Input, out input))
        {
            return false;
        }

        if (options.IsDefaultPath()
            && pipeMode != CommandLine.PipeMode.Paths
            && redirectedInput is not null)
        {
            if (input is not null)
            {
                context.WriteError("Cannot use both redirected/piped input and "
                    + $"option '{OptionNames.GetHelpText(OptionNames.Input)}'.");

                return false;
            }

            if (contentFilter is null)
            {
                context.WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Content)}' is required "
                    + "when redirected/piped input is used as a text to be searched.");

                return false;
            }

            input = redirectedInput;
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
                out ContentDisplayStyle contentDisplayStyle3,
                provider: OptionValueProviders.ContentDisplayStyleProvider_WithoutUnmatchedLines))
            {
                contentDisplayStyle = contentDisplayStyle3;
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
                out PathDisplayStyle pathDisplayStyle3,
                provider: OptionValueProviders.PathDisplayStyleProvider))
            {
                pathDisplayStyle = pathDisplayStyle3;
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
        if (contentDisplayStyle is not null)
        {
            if (options.AskMode == AskMode.Value
                && contentDisplayStyle == ContentDisplayStyle.AllLines)
            {
                string helpValue = OptionValueProviders.ContentDisplayStyleProvider
                    .GetValue(nameof(ContentDisplayStyle.AllLines))
                    .HelpValue;

                string helpValue2 = OptionValueProviders.AskModeProvider.GetValue(nameof(AskMode.Value)).HelpValue;

                context.WriteError($"Option '{OptionNames.GetHelpText(OptionNames.ContentMode)}' cannot have value "
                    + $"'{helpValue}' when option '{OptionNames.GetHelpText(OptionNames.Ask)}' has value '{helpValue2}'.");

                return false;
            }

            contentDisplayStyle = contentDisplayStyle.Value;
        }
        else if (Input.Any())
        {
            contentDisplayStyle = ContentDisplayStyle.AllLines;
        }

        if (pathDisplayStyle == PathDisplayStyle.Relative
            && options.Paths.Length > 1
            && options.SortOptions is not null)
        {
            pathDisplayStyle = PathDisplayStyle.Full;
        }

        if (!context.TryParseHighlightOptions(
            Highlight,
            out HighlightOptions highlightOptions,
            defaultValue: HighlightOptions.Replacement,
            contentDisplayStyle: contentDisplayStyle,
            provider: OptionValueProviders.ReplaceHighlightOptionsProvider))
        {
            return false;
        }

        options.Format = new OutputDisplayFormat(
            contentDisplayStyle: contentDisplayStyle ?? ContentDisplayStyle.Line,
            pathDisplayStyle: pathDisplayStyle ?? PathDisplayStyle.Full,
            lineOptions: lineDisplayOptions,
            lineContext: lineContext,
            displayParts: displayParts,
            indent: indent,
            separator: separator);

        options.HighlightOptions = highlightOptions;
        options.ContentFilter = contentFilter;
        options.Input = input;
        options.DryRun = DryRun;
        options.MaxMatchesInFile = MaxMatchesInFile;
        options.MaxMatchingFiles = MaxMatchingFiles;
        options.MaxTotalMatches = MaxCount;
        options.Interactive = Interactive;

        return true;
    }

    protected virtual Matcher? GetDefaultContentFilter()
    {
        return null;
    }

    protected override bool TryParsePaths(out ImmutableArray<PathInfo> paths, ParseContext context)
    {
        if (Path.Any()
            && Input.Any())
        {
            context.WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Input)}' and "
                + $"argument '{ArgumentMetaNames.Path}' cannot be set both at the same time.");

            paths = default;
            return false;
        }

        return base.TryParsePaths(out paths, context);
    }
}
