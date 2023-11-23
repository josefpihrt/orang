// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.FileSystem;

namespace Orang.CommandLine;

[Verb("find", HelpText = "Searches the file system for files and directories and optionally searches files' content.")]
[CommandGroup("Main", 0)]
internal sealed class FindCommandLineOptions : CommonFindCommandLineOptions
{
    public override ContentDisplayStyle DefaultContentDisplayStyle => ContentDisplayStyle.Line;

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
        longName: OptionNames.Modify,
        HelpText = "Functions to modify results.",
        MetaValue = MetaValues.ModifyOptions)]
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

        if (!context.TryParseProperties(Ask, Name, options, allowEmptyPattern: true))
            return false;

        var baseOptions = (CommonFindCommandOptions)options;

        if (!TryParse(baseOptions, context))
            return false;

        options = (FindCommandOptions)baseOptions;

        string? input = null;

        if (options.IsDefaultPath()
            && pipeMode != CommandLine.PipeMode.Paths
            && redirectedInput is not null)
        {
            if (options.ContentFilter is null)
            {
                context.WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Content)}' is required "
                    + "when redirected/piped input is used as a text to be searched.");

                return false;
            }

            input = redirectedInput;

            if (options.Paths.Length == 1
                && options.Paths[0].Origin == PathOrigin.CurrentDirectory
                && options.ExtensionFilter is null
                && options.NameFilter is null)
            {
                options.Paths = ImmutableArray<PathInfo>.Empty;
            }
        }

        if (!context.TryParseModifyOptions(
            Modify,
            OptionNames.Modify,
            out ModifyOptions? modifyOptions,
            out bool aggregateOnly))
        {
            return false;
        }

        OutputDisplayFormat format = options.Format;
        ContentDisplayStyle contentDisplayStyle = format.ContentDisplayStyle;
        PathDisplayStyle pathDisplayStyle = format.PathDisplayStyle;

        if (modifyOptions.HasAnyFunction
            && contentDisplayStyle == ContentDisplayStyle.ValueDetail)
        {
            contentDisplayStyle = ContentDisplayStyle.Value;
        }

        options.Input = input;
        options.ModifyOptions = modifyOptions;
        options.AggregateOnly = aggregateOnly;
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
}
