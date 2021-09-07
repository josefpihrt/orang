// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CommandLine;

namespace Orang.CommandLine
{
    [Verb("create-pattern", HelpText = "")]
    [CommandGroup("Regex", 2)]
    internal sealed class CreatePatternCommandLineOptions : AbstractCommandLineOptions
    {
        [Value(
            index: 0,
            Required = true,
            HelpText = "",
            MetaName = "<PATTERN|PATH>")]
        public string Value { get; set; } = null!;

        [Option(
            longName: OptionNames.EndsWith,
            HelpText = "")]
        public bool EndsWith { get; set; }

        [Option(
            longName: OptionNames.Equals_,
            HelpText = "")]
        public bool Equals_ { get; set; }

        [Option(
            shortName: OptionShortNames.ExplicitCapture,
            longName: OptionNames.ExplicitCapture,
            HelpText = "")]
        public bool ExplicitCapture { get; set; }

        [Option(
            shortName: OptionShortNames.FromFile,
            longName: OptionNames.FromFile,
            HelpText = "")]
        public bool FromFile { get; set; }

        [Option(
            shortName: OptionShortNames.IgnoreCase,
            longName: OptionNames.IgnoreCase,
            HelpText = "")]
        public bool IgnoreCase { get; set; }

        [Option(
            shortName: OptionShortNames.IgnorePatternWhitespace,
            longName: OptionNames.IgnorePatternWhitespace,
            HelpText = "")]
        public bool IgnorePatternWhitespace { get; set; }

        [Option(
            longName: OptionNames.List,
            HelpText = "")]
        public bool List { get; set; }

        [Option(
            longName: OptionNames.ListSeparator,
            HelpText = "")]
        public string ListSeparator { get; set; } = null!;

        [Option(
            shortName: OptionShortNames.Literal,
            longName: OptionNames.Literal,
            HelpText = "")]
        public bool Literal { get; set; }

        [Option(
            shortName: OptionShortNames.Multiline,
            longName: OptionNames.Multiline,
            HelpText = "")]
        public bool Multiline { get; set; }

        [Option(
            shortName: OptionShortNames.Singleline,
            longName: OptionNames.Singleline,
            HelpText = "")]
        public bool Singleline { get; set; }

        [Option(
            longName: OptionNames.StartsWith,
            HelpText = "")]
        public bool StartsWith { get; set; }

        [Option(
            longName: OptionNames.WholeLine,
            HelpText = "")]
        public bool WholeLine { get; set; }

        [Option(
            shortName: OptionShortNames.WholeWord,
            longName: OptionNames.WholeWord,
            HelpText = "")]
        public bool WholeWord { get; set; }

        public bool TryParse(CreatePatternCommandOptions options)
        {
            var patternOptions = PatternOptions.None;

            if (ExplicitCapture)
                patternOptions |= PatternOptions.ExplicitCapture;

            if (EndsWith)
                patternOptions |= PatternOptions.EndsWith;

            if (Equals_)
                patternOptions |= PatternOptions.Equals;

            if (FromFile)
                patternOptions |= PatternOptions.FromFile;

            if (IgnoreCase)
                patternOptions |= PatternOptions.IgnoreCase;

            if (IgnorePatternWhitespace)
                patternOptions |= PatternOptions.IgnorePatternWhitespace;

            if (List)
                patternOptions |= PatternOptions.List;

            if (Literal)
                patternOptions |= PatternOptions.Literal;

            if (Multiline)
                patternOptions |= PatternOptions.Multiline;

            if (Singleline)
                patternOptions |= PatternOptions.Singleline;

            if (StartsWith)
                patternOptions |= PatternOptions.StartsWith;

            if (WholeLine)
                patternOptions |= PatternOptions.WholeLine;

            if (WholeWord)
                patternOptions |= PatternOptions.WholeWord;

            options.PatternOptions = patternOptions;
            options.PatternOrPath = Value!;
            options.Separator = ListSeparator;

            return true;
        }
    }
}
