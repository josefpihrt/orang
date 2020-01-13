// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine
{
    internal static class OptionValues
    {
        public static readonly SimpleOptionValue PathDisplayStyle_Full = SimpleOptionValue.Create(PathDisplayStyle.Full, description: "Display full path.");
        public static readonly SimpleOptionValue PathDisplayStyle_Relative = SimpleOptionValue.Create(PathDisplayStyle.Relative, description: "Display path relatively to the base directory.");
        public static readonly SimpleOptionValue PathDisplayStyle_Match = SimpleOptionValue.Create(PathDisplayStyle.Match, description: "Display only match.");
        public static readonly SimpleOptionValue PathDisplayStyle_Omit = SimpleOptionValue.Create(PathDisplayStyle.Omit, description: "Do not display path.");
        public static readonly SimpleOptionValue PatternOptions_CaseSensitive = SimpleOptionValue.Create(PatternOptions.CaseSensitive, shortValue: "cs", helpValue: "c[ase-]s[ensitive]", description: "Use case-sensitive matching.");
        public static readonly SimpleOptionValue PatternOptions_Compiled = SimpleOptionValue.Create(PatternOptions.Compiled, shortValue: "", description: "Compile the regular expression to an assembly.");
        public static readonly SimpleOptionValue PatternOptions_CultureInvariant = SimpleOptionValue.Create(PatternOptions.CultureInvariant, shortValue: "ci", helpValue: "c[ulture-]i[nvariant]", description: "Ignore cultural differences between languages.");
        public static readonly SimpleOptionValue PatternOptions_ECMAScript = SimpleOptionValue.Create(PatternOptions.ECMAScript, value: "ecma-script", shortValue: "es", helpValue: "e[cma-]s[cript]", description: "Enable ECMAScript-compliant behavior for the expression.");
        public static readonly SimpleOptionValue PatternOptions_EndsWith = SimpleOptionValue.Create(PatternOptions.EndsWith, shortValue: "ew", helpValue: "e[nds-]w[ith]", description: "Pattern should match from the end of the input string.");
        public static readonly SimpleOptionValue PatternOptions_Equals = SimpleOptionValue.Create(PatternOptions.Equals, description: "Pattern should match whole input string.");
        public static readonly SimpleOptionValue PatternOptions_ExplicitCapture = SimpleOptionValue.Create(PatternOptions.ExplicitCapture, shortValue: "n", description: "Do not capture unnamed groups.");
        public static readonly SimpleOptionValue PatternOptions_FromFile = SimpleOptionValue.Create(PatternOptions.FromFile, description: "Load pattern from a file.");
        public static readonly SimpleOptionValue PatternOptions_IgnoreCase = SimpleOptionValue.Create(PatternOptions.IgnoreCase, description: "Use case-insensitive matching.");
        public static readonly SimpleOptionValue PatternOptions_IgnorePatternWhitespace = SimpleOptionValue.Create(PatternOptions.IgnorePatternWhitespace, shortValue: "x", description: "Exclude unescaped white-space from the pattern and enable comments after a number sign (#).");
        public static readonly SimpleOptionValue PatternOptions_List = SimpleOptionValue.Create(PatternOptions.List, shortValue: "li", helpValue: "li[st]", description: "Interpret pattern as a list of patterns any of which is to be matched. Separator is either comma (,) or newline if the list is loaded from a file.");
        public static readonly SimpleOptionValue PatternOptions_Literal = SimpleOptionValue.Create(PatternOptions.Literal, description: "Pattern should be treated as a literal expression and not as a regular expression.");
        public static readonly SimpleOptionValue PatternOptions_Multiline = SimpleOptionValue.Create(PatternOptions.Multiline, description: "^ and $ match the beginning and end of each line (instead of the beginning and end of the input string).");
        public static readonly SimpleOptionValue PatternOptions_Negative = SimpleOptionValue.Create(PatternOptions.Negative, shortValue: "ne", description: "Search succeeds if the regular expression does not match.");
        public static readonly SimpleOptionValue PatternOptions_RightToLeft = SimpleOptionValue.Create(PatternOptions.RightToLeft, description: "Specifies that the search will be from right to left.");
        public static readonly SimpleOptionValue PatternOptions_Singleline = SimpleOptionValue.Create(PatternOptions.Singleline, description: "The period (.) matches every character (instead of every character except \\n).");
        public static readonly SimpleOptionValue PatternOptions_StartsWith = SimpleOptionValue.Create(PatternOptions.StartsWith, shortValue: "sw", helpValue: "s[tarts-]w[ith]", description: "Pattern should match from the start of the input string.");
        public static readonly SimpleOptionValue PatternOptions_WholeLine = SimpleOptionValue.Create(PatternOptions.WholeLine, shortValue: "wl", helpValue: "w[hole-]l[ine]", description: "Pattern should match whole line.");
        public static readonly SimpleOptionValue PatternOptions_WholeWord = SimpleOptionValue.Create(PatternOptions.WholeWord, description: "Pattern should match whole word.");
        public static readonly SimpleOptionValue Display_Count = SimpleOptionValue.Create("Count", description: "Include number of matches in file.");
        public static readonly SimpleOptionValue Display_CreationTime = SimpleOptionValue.Create("CreationTime", shortValue: "ct", helpValue: "c[reation-]t[ime]", description: "Include file creation time.");
        public static readonly SimpleOptionValue Display_LineNumber = SimpleOptionValue.Create("LineNumber", description: "Include line number.");
        public static readonly SimpleOptionValue Display_ModifiedTime = SimpleOptionValue.Create("ModifiedTime", shortValue: "mt", helpValue: "m[odified-]t[ime]", description: "Include file last modified time.");
        public static readonly SimpleOptionValue Display_Size = SimpleOptionValue.Create("Size", description: "Include file size.");
        public static readonly SimpleOptionValue Display_Summary = SimpleOptionValue.Create("Summary", shortValue: "su", description: "Include summary.");
        public static readonly SimpleOptionValue Display_TrimLine = SimpleOptionValue.Create("TrimLine", description: "Trim leading and trailing white-space from a line.");
        public static readonly SimpleOptionValue Output_Append = SimpleOptionValue.Create("Append", description: "If the file exists output will be appended to the end of the file.");

        public static readonly KeyValuePairOptionValue Display_Content = KeyValuePairOptionValue.Create("content", MetaValues.ContentDisplay, shortKey: "c");
        public static readonly KeyValuePairOptionValue Display_Indent = KeyValuePairOptionValue.Create("indent", "<INDENT>", shortKey: "", description: "Indentation for a list of results. Default indentation are 2 spaces.");
        public static readonly KeyValuePairOptionValue Display_Path = KeyValuePairOptionValue.Create("path", MetaValues.PathDisplay, shortKey: "p");
        public static readonly KeyValuePairOptionValue Display_Separator = KeyValuePairOptionValue.Create("separator", "<SEPARATOR>", description: "String that separate each value.");
        public static readonly KeyValuePairOptionValue Encoding = KeyValuePairOptionValue.Create("encoding", MetaValues.Encoding, shortKey: "e");
        public static readonly KeyValuePairOptionValue FileProperty_CreationTime = KeyValuePairOptionValue.Create("creation-time", "<DATE>", shortKey: "ct", helpValue: "c[reation-]t[ime]", description: "Filter files by creation time (See 'Expression syntax' for other expressions).");
        public static readonly KeyValuePairOptionValue FileProperty_ModifiedTime = KeyValuePairOptionValue.Create("modified-time", "<DATE>", shortKey: "mt", helpValue: "m[odified-]t[ime]", description: "Filter files by modified time (See 'Expression syntax' for other expressions).");
        public static readonly KeyValuePairOptionValue FileProperty_Size = KeyValuePairOptionValue.Create("size", "<NUM>", description: "Filter files by size (See 'Expression syntax' for other expressions).");
        public static readonly KeyValuePairOptionValue Group = KeyValuePairOptionValue.Create("group", "<GROUP_NAME>", shortKey: "g");
        public static readonly KeyValuePairOptionValue ListSeparator = KeyValuePairOptionValue.Create("list-separator", "<SEPARATOR>", shortKey: "ls", helpValue: "l[ist-]s[eparator]", description: "String that separate each value in a list. Default value is comma (,) or newline if the list is loaded from a file.");
        public static readonly KeyValuePairOptionValue Length = KeyValuePairOptionValue.Create("length", "<NUM>", shortKey: "", description: "Include matches whose length matches the expression (See 'Expression syntax' for other expressions).");
        public static readonly KeyValuePairOptionValue MaxCount = KeyValuePairOptionValue.Create("max-count", "<NUM>", description: "Show only <NUM> items.");
        public static readonly KeyValuePairOptionValue MaxMatches = KeyValuePairOptionValue.Create("matches", "<NUM>", description: "Stop searching after <NUM> matches.");
        public static readonly KeyValuePairOptionValue MaxMatchingFiles = KeyValuePairOptionValue.Create("matching-files", "<NUM>", shortKey: "mf", helpValue: "m[atching-]f[iles]", description: "Stop searching after <NUM> matching files.");
        public static readonly KeyValuePairOptionValue Part = KeyValuePairOptionValue.Create("part", MetaValues.NamePart, shortKey: "p", description: "The part of a file or a directory name that should be matched.");
        public static readonly KeyValuePairOptionValue Timeout = KeyValuePairOptionValue.Create("timeout", "<NUM>", shortKey: "", description: "Match time-out interval in seconds.");
        public static readonly KeyValuePairOptionValue Verbosity = KeyValuePairOptionValue.Create("verbosity", MetaValues.Verbosity, shortKey: "v");
    }
}
