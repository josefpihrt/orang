// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine
{
    internal static class OptionValues
    {
        public static readonly SimpleOptionValue PatternOptions_Compiled = SimpleOptionValue.Create(PatternOptions.Compiled, shortValue: "", description: "Compile the regular expression to an assembly.");
        public static readonly SimpleOptionValue PatternOptions_CultureInvariant = SimpleOptionValue.Create(PatternOptions.CultureInvariant, shortValue: "ci", helpValue: "[c]ulture-[i]nvariant", description: "Ignore cultural differences between languages.");
        public static readonly SimpleOptionValue PatternOptions_ECMAScript = SimpleOptionValue.Create(PatternOptions.ECMAScript, value: "ecma-script", shortValue: "es", helpValue: "[e]cma-[s]cript", description: "Enable ECMAScript-compliant behavior for the expression.");
        public static readonly SimpleOptionValue PatternOptions_ExplicitCapture = SimpleOptionValue.Create(PatternOptions.ExplicitCapture, shortValue: "n", description: "Do not capture unnamed groups.");
        public static readonly SimpleOptionValue PatternOptions_FromFile = SimpleOptionValue.Create(PatternOptions.FromFile, description: "Load pattern from a file.");
        public static readonly SimpleOptionValue PatternOptions_IgnoreCase = SimpleOptionValue.Create(PatternOptions.IgnoreCase, description: "Use case-insensitive matching.");
        public static readonly SimpleOptionValue PatternOptions_IgnorePatternWhitespace = SimpleOptionValue.Create(PatternOptions.IgnorePatternWhitespace, shortValue: "x", description: "Exclude unescaped white-space from the pattern and enable comments after a number sign (#).");
        public static readonly SimpleOptionValue PatternOptions_List = SimpleOptionValue.Create(PatternOptions.List, shortValue: "li", helpValue: "[li]st", description: "Interpret pattern as a list of patterns separated by newlines, any of which is to be matched.");
        public static readonly SimpleOptionValue PatternOptions_Literal = SimpleOptionValue.Create(PatternOptions.Literal, description: "Pattern should be treated as a literal expression and not as a regular expression.");
        public static readonly SimpleOptionValue PatternOptions_Multiline = SimpleOptionValue.Create(PatternOptions.Multiline, description: "^ and $ match the beginning and end of each line (instead of the beginning and end of the input string).");
        public static readonly SimpleOptionValue PatternOptions_Negative = SimpleOptionValue.Create(PatternOptions.Negative, shortValue: "neg", description: "Search succeeds if the regular expression does not match.");
        public static readonly SimpleOptionValue PatternOptions_RightToLeft = SimpleOptionValue.Create(PatternOptions.RightToLeft, description: "Specifies that the search will be from right to left.");
        public static readonly SimpleOptionValue PatternOptions_Singleline = SimpleOptionValue.Create(PatternOptions.Singleline, description: "The period (.) matches every character (instead of every character except \\n).");
        public static readonly SimpleOptionValue PatternOptions_WholeInput = SimpleOptionValue.Create(PatternOptions.WholeInput, shortValue: "wi", helpValue: "[w]hole-[i]nput", description: "Pattern should match whole input string.");
        public static readonly SimpleOptionValue PatternOptions_WholeLine = SimpleOptionValue.Create(PatternOptions.WholeLine, shortValue: "wl", helpValue: "[w]hole-[l]ine", description: "Pattern should match whole line.");
        public static readonly SimpleOptionValue PatternOptions_WholeWord = SimpleOptionValue.Create(PatternOptions.WholeWord, description: "Pattern should match whole word.");
        public static readonly SimpleOptionValue OutputOptions_Content = SimpleOptionValue.Create("Content", description: "Include content in the output.");
        public static readonly SimpleOptionValue OutputOptions_Path = SimpleOptionValue.Create("Path", description: "Include path in the output.");
        public static readonly SimpleOptionValue Display_Summary = SimpleOptionValue.Create("Summary", description: "Include summary.");

        public static readonly KeyValuePairOptionValue Display_Content = KeyValuePairOptionValue.Create("content", MetaValues.ContentDisplay, shortKey: "c");
        public static readonly KeyValuePairOptionValue Display_Path = KeyValuePairOptionValue.Create("path", MetaValues.PathDisplay, shortKey: "p");
        public static readonly KeyValuePairOptionValue Encoding = KeyValuePairOptionValue.Create("encoding", MetaValues.Encoding, shortKey: "e");
        public static readonly KeyValuePairOptionValue Group = KeyValuePairOptionValue.Create("group", "<GROUP_NAME>", shortKey: "g");
        public static readonly KeyValuePairOptionValue ListSeparator = KeyValuePairOptionValue.Create("list-separator", "<SEPARATOR>", shortKey: "ls", helpValue: "[li]st-[s]eparator", description: "String that separate each value in a list. Default value is comma (,) or newline if the list is loaded from a file.");
        public static readonly KeyValuePairOptionValue MaxMatches = KeyValuePairOptionValue.Create("matches", "<NUM>", description: "Stop searching after <NUM> matches.");
        public static readonly KeyValuePairOptionValue MaxMatchingFiles = KeyValuePairOptionValue.Create("matching-files", "<NUM>", shortKey: "mf", helpValue: "[m]atching-[f]iles", description: "Stop searching after <NUM> matching files.");
        public static readonly KeyValuePairOptionValue Part = KeyValuePairOptionValue.Create("part", MetaValues.NamePart, shortKey: "p", description: "The part of a file or a directory name that should be matched.");
        public static readonly KeyValuePairOptionValue Timeout = KeyValuePairOptionValue.Create("timeout", "<NUM>", shortKey: "", description: "Match time-out interval in seconds.");
        public static readonly KeyValuePairOptionValue Verbosity = KeyValuePairOptionValue.Create("verbosity", MetaValues.Verbosity, shortKey: "v");
    }
}
