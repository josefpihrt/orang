// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CommandLine;

namespace Orang.CommandLine
{
    [Verb("escape", HelpText = "Escapes special characters by replacing them with their escape codes.")]
    internal class EscapeCommandLineOptions
    {
        [Option(shortName: OptionShortNames.Input, longName: OptionNames.Input,
            Required = true,
            HelpText = "Text to be escaped.",
            MetaValue = MetaValues.Input)]
        public string Input { get; set; }

        [Option(longName: OptionNames.CharGroup,
            HelpText = "Text is part of a character group.")]
        public bool CharGroup { get; set; }

        [Option(shortName: OptionShortNames.Replacement, longName: OptionNames.Replacement,
            HelpText = "Text is a replacement string.")]
        public bool Replacement { get; set; }

        public bool TryParse(ref EscapeCommandOptions options)
        {
            options.Input = Input;
            options.InCharGroup = CharGroup;
            options.Replacement = Replacement;

            return true;
        }
    }
}
