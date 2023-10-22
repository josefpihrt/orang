// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.FileSystem;
using System.Text.RegularExpressions;

namespace Orang.CommandLine;

[Verb("find", HelpText = "Searches the file system for files and directories and optionally searches files' content.")]
[CommandGroup("Main", 0)]
internal sealed class FindCommandLineOptions : CommonFindCommandLineOptions
{
    [Option(
        longName: OptionNames.Ask,
        HelpText = "Ask for permission after each file or value.",
        MetaValue = MetaValues.AskMode)]
    public string Ask { get; set; } = null!;

    [Option(
        longName: OptionNames.Pipe,
        HelpText = "Defines how to use redirected/piped input.",
        MetaValue = MetaValues.PipeMode)]
    public string Pipe { get; set; } = null!;

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

    [Option(
        longName: "split",
        HelpText = "Execute regex in a split mode.")]
    public bool Split { get; set; }

    public bool TryParse(FindCommandOptions options, ParseContext context)
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

        if (pipeMode == CommandLine.PipeMode.Paths
            || pipeMode == CommandLine.PipeMode.Text)
        {
            if (!Console.IsInputRedirected)
            {
                context.WriteError("Redirected/piped input is required "
                    + $"when option '{OptionNames.GetHelpText(OptionNames.Pipe)}' is specified.");

                return false;
            }

            PipeMode = pipeMode;
        }

        if (!context.TryParseProperties(Ask, Name, options, allowEmptyPattern: true))
            return false;

        var baseOptions = (CommonFindCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (FindCommandOptions)baseOptions;

        string? input = null;

        if (options.IsDefaultPath()
            && pipeMode != CommandLine.PipeMode.Paths
            && Console.IsInputRedirected)
        {
            if (options.ContentFilter is null)
            {
                context.WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Content)}' is required "
                    + "when redirected/piped input is used as a text to be searched.");

                return false;
            }

            input = ConsoleHelpers.ReadRedirectedInput();

            if (options.Paths.Length == 1
                && options.Paths[0].Origin == PathOrigin.CurrentDirectory
                && options.ExtensionFilter is null
                && options.NameFilter is null)
            {
                options.Paths = ImmutableArray<PathInfo>.Empty;
            }
        }

        if (Modify.Any())
        {
            context.WriteWarning($"Option '{OptionNames.GetHelpText(OptionNames.Modify)}' has been deprecated "
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
            options.AggregateOnly = aggregateOnly;
        }

        if (Function.Any())
        {
            if (Modify.Any())
            {
                context.WriteError($"Options '{OptionNames.GetHelpText(OptionNames.Modify)}' and "
                    + $"'{OptionNames.GetHelpText(OptionNames.Function)}' cannot be used both at the same time.");

                return false;
            }

            if (options.ContentFilter is null)
            {
                context.WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Content)}' is required when "
                    + $"option '{OptionNames.GetHelpText(OptionNames.Function)}' is used.");

                return false;
            }

            if (!context.TryParseFunctions(
                Modify,
                OptionNames.Modify,
                out ModifyFunctions? functions,
                out ValueSortProperty sortProperty))
            {
                return false;
            }

            options.ModifyOptions = new ModifyOptions(
                functions.Value,
                aggregate: false,
                ignoreCase: (options.ContentFilter.Regex.Options & RegexOptions.IgnoreCase) != 0,
                cultureInvariant: (options.ContentFilter.Regex.Options & RegexOptions.CultureInvariant) != 0,
                sortProperty: sortProperty);

            options.AggregateOnly = true;
        }

        OutputDisplayFormat format = options.Format;
        ContentDisplayStyle contentDisplayStyle = format.ContentDisplayStyle;
        PathDisplayStyle pathDisplayStyle = format.PathDisplayStyle;

        options.Input = input;
        options.Split = Split;

        options.Format = new OutputDisplayFormat(
            contentDisplayStyle: contentDisplayStyle,
            pathDisplayStyle: pathDisplayStyle,
            lineOptions: format.LineOptions,
            lineContext: format.LineContext,
            displayParts: format.DisplayParts,
            indent: format.Indent,
            separator: format.Separator);

        return true;
    }

    public override ContentDisplayStyle GetDefaultContentDisplayStyle()
    {
        return (Function.Any()) ? ContentDisplayStyle.Value : ContentDisplayStyle.Line;
    }

    public override bool VerifyContentDisplayStyle(ContentDisplayStyle contentDisplayStyle, ParseContext context)
    {
        if (Function.Any()
            && contentDisplayStyle != ContentDisplayStyle.Value)
        {
            context.WriteError($"When option '{OptionNames.GetHelpText(OptionNames.Function)}' is used "
                + $"option '{OptionNames.GetHelpText(OptionNames.ContentMode)}' must be set to "
                + $"'{OptionValues.ContentDisplayStyle_Value.HelpValue}' (or unspecified).");

            return false;
        }

        return true;
    }
}
