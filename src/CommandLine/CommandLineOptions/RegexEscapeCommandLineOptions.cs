// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CommandLine;
using Orang.CommandLine.Annotations;

namespace Orang.CommandLine;

[Verb("regex-escape", HelpText = "Escapes special characters by replacing them with their escape codes.")]
[CommandGroup("Regex", 2)]
[CommandAlias("regex escape")]
internal sealed class RegexEscapeCommandLineOptions : BaseCommandLineOptions
{
    [Option(
        shortName: OptionShortNames.Input,
        longName: OptionNames.Input,
        HelpText = "Text to be escaped.",
        MetaValue = MetaValues.Input)]
    public string Input { get; set; } = null!;

    [Option(
        longName: OptionNames.CharGroup,
        HelpText = "Text is part of a character group.")]
    public bool CharGroup { get; set; }

    [Option(
        shortName: OptionShortNames.Replacement,
        longName: OptionNames.Replacement,
        HelpText = "Text is a replacement string.")]
    public bool Replacement { get; set; }

    public bool TryParse(RegexEscapeCommandOptions options, ParseContext context)
    {
        string input = Input;

        if (input == null)
        {
            string? redirectedInput = ConsoleHelpers.ReadRedirectedInput();

            if (redirectedInput == null)
            {
                context.WriteError("Input is missing.");
                return false;
            }

            input = redirectedInput;
        }

        options.Input = input;
        options.InCharGroup = CharGroup;
        options.Replacement = Replacement;

        return true;
    }
}
