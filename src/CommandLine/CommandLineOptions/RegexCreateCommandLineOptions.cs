// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CommandLine;
using Orang.CommandLine.Annotations;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

[Verb("regex-create", HelpText = "Creates regular expression pattern.")]
[CommandGroup("Regex", 2)]
[CommandAlias("regex create")]
internal sealed class RegexCreateCommandLineOptions : AbstractCommandLineOptions
{
    [Value(
        index: 0,
        HelpText = "",
        MetaName = "<PATTERN|FILE_PATH>")]
    public string Input { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.EndsWith,
        longName: OptionNames.EndsWith,
        HelpText = "Pattern should match from the end of the input string.")]
    public bool EndsWith { get; set; }

    [Option(
        shortName: OptionShortNames.Equals,
        longName: OptionNames.Equals,
        HelpText = "Pattern should match whole input string.")]
    public bool Equals_ { get; set; }

    [Option(
        shortName: OptionShortNames.ExplicitCapture,
        longName: OptionNames.ExplicitCapture,
        HelpText = "Do not capture unnamed groups.")]
    public bool ExplicitCapture { get; set; }

    [Option(
        shortName: OptionShortNames.FromFile,
        longName: OptionNames.FromFile,
        HelpText = "Interpret argument value as path to a file whose content will be used as an input.")]
    public bool FromFile { get; set; }

    [Option(
        shortName: OptionShortNames.IgnoreCase,
        longName: OptionNames.IgnoreCase,
        HelpText = "Use case-insensitive matching.")]
    public bool IgnoreCase { get; set; }

    [Option(
        shortName: OptionShortNames.IgnorePatternWhitespace,
        longName: OptionNames.IgnorePatternWhitespace,
        HelpText = "Exclude unescaped white-space from the pattern and enable comments after '#' sign.")]
    public bool IgnorePatternWhitespace { get; set; }

    [Option(
        shortName: OptionShortNames.List,
        longName: OptionNames.List,
        HelpText = "Interpret pattern as a list of pattern any of which has to be matched. "
            + "Separator is either comma or newline if the patter is loaded from a file.")]
    public bool List { get; set; }

    [Option(
        longName: OptionNames.Separator,
        HelpText = "String that separates each value in a list. Default value is comma or newline if the list is loaded from a file.")]
    public string Separator { get; set; } = null!;

    [Option(
        shortName: OptionShortNames.Literal,
        longName: OptionNames.Literal,
        HelpText = "Text should be treated as a literal expression not as a regular expression.")]
    public bool Literal { get; set; }

    [Option(
        shortName: OptionShortNames.Multiline,
        longName: OptionNames.Multiline,
        HelpText = "^ and $ match beginning and end of each line (instead of beginning and end  of the input string).")]
    public bool Multiline { get; set; }

    [Option(
        shortName: OptionShortNames.Singleline,
        longName: OptionNames.Singleline,
        HelpText = "Period matches every character (instead of every character except \\n.")]
    public bool Singleline { get; set; }

    [Option(
        shortName: OptionShortNames.StartsWith,
        longName: OptionNames.StartsWith,
        HelpText = "Pattern should match from the beginning of the input string.")]
    public bool StartsWith { get; set; }

    [Option(
        shortName: OptionShortNames.WholeLine,
        longName: OptionNames.WholeLine,
        HelpText = "Pattern should match whole line.")]
    public bool WholeLine { get; set; }

    [Option(
        shortName: OptionShortNames.WholeWord,
        longName: OptionNames.WholeWord,
        HelpText = "Pattern should match whole word.")]
    public bool WholeWord { get; set; }

    public bool TryParse(RegexCreateCommandOptions options, ParseContext context)
    {
        var patternOptions = PatternOptions.None;

        string input = Input;

        if (input is null)
        {
            string? redirectedInput = ConsoleHelpers.ReadRedirectedInput();

            if (redirectedInput is null)
            {
                context.WriteError("Input is missing.");
                return false;
            }

            input = redirectedInput;
        }

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

        string separator = Separator;

        if (separator is not null)
            separator = RegexEscape.ConvertCharacterEscapes(separator);

        options.PatternOptions = patternOptions;
        options.Input = input;
        options.Separator = separator;

        return true;
    }
}
