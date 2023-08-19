// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

[OptionValueProvider(nameof(Display), OptionValueProviderNames.Display_MatchAndSplit)]
internal abstract class RegexCommandLineOptions : CommonRegexCommandLineOptions
{
    [Value(
        index: 0,
        HelpText = "Path to a file that should be analyzed.",
        MetaName = ArgumentMetaNames.Path)]
    public string Path { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.Input,
        longName: OptionNames.Input,
        HelpText = "The input string to be searched.",
        MetaValue = MetaValues.Input)]
    public IEnumerable<string> Input { get; set; } = null!;
#if DEBUG
    [HideFromConsoleHelp]
    [Option(
        longName: OptionNames.Modifier,
        HelpText = OptionHelpText.Modifier,
        MetaValue = MetaValues.Modifier)]
    public IEnumerable<string> Modifier { get; set; } = null!;
#endif
    [Option(
        longName: OptionNames.Modify,
        HelpText = "Functions to modify results.",
        MetaValue = MetaValues.ModifyOptions)]
    public IEnumerable<string> Modify { get; set; } = null!;

    public bool TryParse(RegexCommandOptions options, ParseContext context)
    {
        var baseOptions = (CommonRegexCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (RegexCommandOptions)baseOptions;

        string input = Input.FirstOrDefault();

        if (!string.IsNullOrEmpty(Path))
        {
            if (input is not null)
            {
                context.WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Input)}' and "
                    + $"argument '{ArgumentMetaNames.Path}' cannot be set both at the same time.");

                return false;
            }

            try
            {
                input = File.ReadAllText(Path);
            }
            catch (Exception ex) when (ex is ArgumentException
                || ex is IOException
                || ex is UnauthorizedAccessException)
            {
                context.WriteError(ex);
                return false;
            }
        }
        else if (string.IsNullOrEmpty(input))
        {
            string? redirectedInput = ConsoleHelpers.ReadRedirectedInput();

            if (redirectedInput is null)
            {
                context.WriteError("Input is missing.");
                return false;
            }

            input = redirectedInput;
        }
        else if (!context.TryParseInput(Input, out input))
        {
            return false;
        }

        if (Display.Any())
        {
            context.WriteWarning($"Option '{OptionNames.GetHelpText(OptionNames.Display)}' has been deprecated "
                + "and will be removed in future version. Use following options instead:"
                + $"{Environment.NewLine}  {OptionNames.GetHelpText(OptionNames.ContentMode)}"
                + $"{Environment.NewLine}  {OptionNames.GetHelpText(OptionNames.Summary)}"
#if DEBUG
                + $"{Environment.NewLine}  {OptionNames.GetHelpText(OptionNames.ContentIndent)}"
                + $"{Environment.NewLine}  {OptionNames.GetHelpText(OptionNames.ContentSeparator)}"
#endif
                );
        }

        if (!context.TryParseDisplay(
            values: Display,
            optionName: OptionNames.Display,
            contentDisplayStyle: out ContentDisplayStyle? contentDisplayStyle,
            pathDisplayStyle: out PathDisplayStyle? _,
            lineDisplayOptions: out LineDisplayOptions lineDisplayOptions,
            lineContext: out LineContext _,
            displayParts: out DisplayParts displayParts,
            includeCreationTime: out bool includeCreationTime,
            includeModifiedTime: out bool includeModifiedTime,
            includeSize: out bool includeSize,
            indent: out string? indent,
            separator: out string? separator,
            noAlign: out bool _,
            contentDisplayStyleProvider: OptionValueProviders.ContentDisplayStyleProvider_WithoutLineAndUnmatchedLines,
            pathDisplayStyleProvider: OptionValueProviders.PathDisplayStyleProvider))
        {
            return false;
        }

        if (Summary)
            displayParts |= DisplayParts.Summary;

        if (ContentMode is not null)
        {
            if (context.TryParseAsEnum(
                ContentMode,
                OptionNames.ContentMode,
                out ContentDisplayStyle contentDisplayStyle2,
                provider: OptionValueProviders.ContentDisplayStyleProvider_WithoutLineAndUnmatchedLines))
            {
                contentDisplayStyle = contentDisplayStyle2;
            }
            else
            {
                return false;
            }
        }
#if DEBUG
        if (ContentIndent is not null)
            indent = RegexEscape.ConvertCharacterEscapes(ContentIndent);

        if (ContentSeparator is not null)
            separator = ContentSeparator;
#endif
        EnumerableModifier<string>? modifier = null;
#if DEBUG // --modifier
        if (Modifier.Any()
            && !context.TryParseModifier(Modifier, OptionNames.Modifier, out modifier))
        {
            return false;
        }
#endif
        if (!context.TryParseModifyOptions(
            Modify,
            OptionNames.Modify,
            modifier,
            out ModifyOptions? modifyOptions,
                out bool aggregateOnly,
                out bool _))
        {
            return false;
        }

        if (modifyOptions.HasAnyFunction
            && contentDisplayStyle == ContentDisplayStyle.ValueDetail)
        {
            contentDisplayStyle = ContentDisplayStyle.Value;
        }

        if (aggregateOnly)
            context.Logger.ConsoleOut.Verbosity = Orang.Verbosity.Minimal;

        options.Format = new OutputDisplayFormat(
            contentDisplayStyle: contentDisplayStyle ?? ContentDisplayStyle.Value,
            pathDisplayStyle: PathDisplayStyle.Full,
            lineOptions: lineDisplayOptions,
            displayParts: displayParts,
            indent: indent,
            separator: separator ?? Environment.NewLine);

        options.ModifyOptions = modifyOptions;
        options.Input = input;

        return true;
    }
}
