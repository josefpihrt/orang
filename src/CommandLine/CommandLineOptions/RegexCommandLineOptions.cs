// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

internal abstract class RegexCommandLineOptions : CommonRegexCommandLineOptions
{
    [Value(
        index: 0,
        HelpText = "Path to a file that should be analyzed.",
        MetaName = ArgumentMetaNames.Path)]
    public string Path { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.Content,
        longName: OptionNames.Content,
        Required = true,
        HelpText = "Regular expression for the input string.",
        MetaValue = MetaValues.Regex)]
    public IEnumerable<string> Content { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.Input,
        longName: OptionNames.Input,
        HelpText = "The input string to be searched.",
        MetaValue = MetaValues.Input)]
    public IEnumerable<string> Input { get; set; } = null!;

    [Option(
        longName: OptionNames.Function,
        HelpText = "Space separated list of functions to modify a list of matches.",
        MetaValue = MetaValues.Function)]
    [AdditionalDescription(" All matches from all files are evaluated at once.")]
    public IEnumerable<string> Function { get; set; } = null!;

    [Option(
        longName: OptionNames.Modify,
        HelpText = "[deprecated] Modify results according to specified values.",
        MetaValue = MetaValues.ModifyOptions)]
    [HideFromHelp]
    public IEnumerable<string> Modify { get; set; } = null!;

    public bool TryParse(RegexCommandOptions options, ParseContext context)
    {
        var baseOptions = (CommonRegexCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (RegexCommandOptions)baseOptions;

        string? input = Input.FirstOrDefault();

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

        var displayParts = DisplayParts.None;
        const LineDisplayOptions lineDisplayOptions = LineDisplayOptions.None;
        ContentDisplayStyle? contentDisplayStyle = null;
#if DEBUG
        string? indent = null;
        string? separator = null;
#else
        const string? indent = null;
#endif
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

        options.ModifyOptions = ModifyOptions.Default;

        if (Modify.Any())
        {
            context.WriteDeprecatedWarning($"Option '{OptionNames.GetHelpText(OptionNames.Modify)}' has been deprecated "
                + "and will be removed in future versions. "
                + $"Use option '{OptionNames.GetHelpText(OptionNames.Function)}' instead.");

            if (!context.TryParseModifyOptions(
                Modify,
                OptionNames.Modify,
                out ModifyOptions? modifyOptions,
                out bool aggregateOnly))
            {
                return false;
            }

            options.ModifyOptions = modifyOptions;

            if (aggregateOnly)
                context.Logger.ConsoleOut.Verbosity = Orang.Verbosity.Minimal;

            if (modifyOptions.HasAnyFunction
                && contentDisplayStyle == ContentDisplayStyle.ValueDetail)
            {
                contentDisplayStyle = ContentDisplayStyle.Value;
            }
        }

        if (Function.Any())
        {
            if (Modify.Any())
            {
                context.WriteError($"Options '{OptionNames.GetHelpText(OptionNames.Modify)}' and "
                    + $"'{OptionNames.GetHelpText(OptionNames.Function)}' cannot be used both at the same time.");

                return false;
            }

            if (!context.TryParseFunctions(
                Function,
                OptionNames.Function,
                out ModifyFunctions? functions,
                out ValueSortProperty sortProperty))
            {
                return false;
            }

            options.ModifyOptions = new ModifyOptions(
                functions.Value,
                aggregate: true,
                ignoreCase: (options.Matcher.Regex.Options & RegexOptions.IgnoreCase) != 0,
                cultureInvariant: (options.Matcher.Regex.Options & RegexOptions.CultureInvariant) != 0,
                sortProperty: sortProperty);

            context.Logger.ConsoleOut.Verbosity = Orang.Verbosity.Minimal;
        }

        options.Format = new OutputDisplayFormat(
            contentDisplayStyle: contentDisplayStyle ?? ContentDisplayStyle.Value,
            pathDisplayStyle: PathDisplayStyle.Full,
            lineOptions: lineDisplayOptions,
            displayParts: displayParts,
            indent: indent,
#if DEBUG
            separator: separator ?? Environment.NewLine);
#else
            separator: Environment.NewLine);
#endif
        options.Input = input;

        return true;
    }
}
