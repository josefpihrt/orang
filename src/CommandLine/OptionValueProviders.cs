// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Orang.FileSystem;
using Orang.Syntax;

namespace Orang.CommandLine
{
    internal static class OptionValueProviders
    {
        private static ImmutableDictionary<string, OptionValueProvider> _providersByName;

        public static OptionValueProvider PatternOptionsProvider { get; } = new OptionValueProvider(MetaValues.PatternOptions,
            OptionValues.PatternOptions_Compiled,
            OptionValues.PatternOptions_CultureInvariant,
            OptionValues.PatternOptions_ECMAScript,
            OptionValues.PatternOptions_ExplicitCapture,
            OptionValues.PatternOptions_FromFile,
            OptionValues.Group,
            OptionValues.PatternOptions_IgnoreCase,
            OptionValues.PatternOptions_IgnorePatternWhitespace,
            OptionValues.PatternOptions_List,
            OptionValues.Length,
            OptionValues.ListSeparator,
            OptionValues.PatternOptions_Literal,
            OptionValues.PatternOptions_Multiline,
            OptionValues.PatternOptions_Negative,
            OptionValues.Part,
            OptionValues.PatternOptions_RightToLeft,
            OptionValues.PatternOptions_Singleline,
            OptionValues.Timeout,
            OptionValues.PatternOptions_WholeInput,
            OptionValues.PatternOptions_WholeLine,
            OptionValues.PatternOptions_WholeWord
        );

        public static OptionValueProvider PatternOptionsWithoutPartProvider { get; } = new OptionValueProvider(OptionValueProviderNames.PatternOptionsWithoutPart,
            PatternOptionsProvider.Values.Where(f => f.Name != OptionValues.Part.Name));

        public static OptionValueProvider PatternOptionsWithoutGroupAndNegativeProvider { get; } = new OptionValueProvider(OptionValueProviderNames.PatternOptionsWithoutGroupAndNegative,
            PatternOptionsProvider.Values.Where(f => f.Name != OptionValues.Group.Name && f.Name != nameof(PatternOptions.Negative))
        );

        public static OptionValueProvider PatternOptionsWithoutPartAndNegativeProvider { get; } = new OptionValueProvider(OptionValueProviderNames.PatternOptionsWithoutPartAndNegative,
            PatternOptionsProvider.Values.Where(f => f.Name != OptionValues.Part.Name && f.Name != nameof(PatternOptions.Negative))
        );

        public static OptionValueProvider PatternOptionsWithoutGroupAndPartAndNegativeProvider { get; } = new OptionValueProvider(OptionValueProviderNames.PatternOptionsWithoutGroupAndPartAndNegative,
            PatternOptionsProvider.Values.Where(f => f.Name != OptionValues.Part.Name && f.Name != OptionValues.Group.Name && f.Name != nameof(PatternOptions.Negative))
        );

        public static OptionValueProvider ExtensionOptionsProvider { get; } = new OptionValueProvider(MetaValues.ExtensionOptions,
            OptionValues.PatternOptions_CultureInvariant,
            OptionValues.PatternOptions_FromFile,
            OptionValues.PatternOptions_IgnoreCase,
            OptionValues.ListSeparator,
            OptionValues.PatternOptions_Literal,
            OptionValues.PatternOptions_Negative,
            OptionValues.Timeout
        );

        public static OptionValueProvider RegexOptionsProvider { get; } = new OptionValueProvider(MetaValues.RegexOptions,
            SimpleOptionValue.Create(RegexOptions.Compiled, description: "Compile the regular expression to an assembly."),
            SimpleOptionValue.Create(RegexOptions.CultureInvariant, shortValue: "ci", helpValue: "[c]ulture-[i]nvariant", description: "Ignore cultural differences between languages."),
            SimpleOptionValue.Create(RegexOptions.ECMAScript, value: "ecma-script", shortValue: "es", helpValue: "[e]cma-[s]cript", description: "Enable ECMAScript-compliant behavior for the expression."),
            SimpleOptionValue.Create(RegexOptions.ExplicitCapture, shortValue: "n", description: "Do not capture unnamed groups."),
            SimpleOptionValue.Create(RegexOptions.IgnoreCase, description: "Use case-insensitive matching."),
            SimpleOptionValue.Create(RegexOptions.IgnorePatternWhitespace, shortValue: "x", description: "Exclude unescaped white-space from the pattern and enable comments after a number sign (#)."),
            SimpleOptionValue.Create(RegexOptions.Multiline, description: "^ and $ match the beginning and end of each line (instead of the beginning and end of the input string)."),
            SimpleOptionValue.Create(RegexOptions.RightToLeft, description: "Specifies that the search will be from right to left."),
            SimpleOptionValue.Create(RegexOptions.Singleline, description: "The period (.) matches every character (instead of every character except \\n).")
        );

        public static OptionValueProvider ReplacementOptionsProvider { get; } = new OptionValueProvider(MetaValues.ReplacementOptions,
            SimpleOptionValue.Create(ReplacementOptions.FromFile, description: "Load replacement string from a file whose path is specified in <REPLACEMENT> value."),
            SimpleOptionValue.Create(ReplacementOptions.Literal, description: "Replacement should be treated as a literal expression and not as a replacement expression."),
            SimpleOptionValue.Create(ReplacementOptions.Multiline, description: @"Literals '\r' and '\n' should be treated as carriage return and linefeed respectively.")
        );

        public static OptionValueProvider VerbosityProvider { get; } = new OptionValueProvider(MetaValues.Verbosity,
            SimpleOptionValue.Create(Verbosity.Quiet),
            SimpleOptionValue.Create(Verbosity.Minimal),
            SimpleOptionValue.Create(Verbosity.Normal),
            SimpleOptionValue.Create(Verbosity.Detailed),
            SimpleOptionValue.Create(Verbosity.Diagnostic, shortValue: "diag")
        );

        public static OptionValueProvider FileSystemAttributesProvider { get; } = new OptionValueProvider(MetaValues.Attributes,
            SimpleOptionValue.Create(FileSystemAttributes.Archive, shortValue: ""),
            SimpleOptionValue.Create(FileSystemAttributes.Compressed, shortValue: ""),
            SimpleOptionValue.Create(FileSystemAttributes.Directory),
            SimpleOptionValue.Create(FileSystemAttributes.Empty),
            SimpleOptionValue.Create(FileSystemAttributes.Encrypted, shortValue: ""),
            SimpleOptionValue.Create(FileSystemAttributes.File),
            SimpleOptionValue.Create(FileSystemAttributes.Hidden),
            SimpleOptionValue.Create(FileSystemAttributes.IntegrityStream, shortValue: "", hidden: true),
            SimpleOptionValue.Create(FileSystemAttributes.Normal, shortValue: ""),
            SimpleOptionValue.Create(FileSystemAttributes.NoScrubData, shortValue: "", hidden: true),
            SimpleOptionValue.Create(FileSystemAttributes.NotContentIndexed, shortValue: "", hidden: true),
            SimpleOptionValue.Create(FileSystemAttributes.Offline, shortValue: ""),
            SimpleOptionValue.Create(FileSystemAttributes.ReadOnly),
            SimpleOptionValue.Create(FileSystemAttributes.ReparsePoint, shortValue: "rp", helpValue: "[r]eparse-[p]oint"),
            SimpleOptionValue.Create(FileSystemAttributes.SparseFile, shortValue: "", hidden: true),
            SimpleOptionValue.Create(FileSystemAttributes.System),
            SimpleOptionValue.Create(FileSystemAttributes.Temporary, shortValue: "")
        );

        public static OptionValueProvider FileSystemAttributesToSkipProvider { get; } = new OptionValueProvider(OptionValueProviderNames.FileSystemAttributesToSkip,
            FileSystemAttributesProvider.Values.Where(f => f.Name != nameof(FileSystemAttributes.File) && f.Name != nameof(FileSystemAttributes.Directory))
        );

        public static OptionValueProvider NamePartKindProvider { get; } = new OptionValueProvider(MetaValues.NamePart,
            SimpleOptionValue.Create(NamePartKind.Extension, description: "Search in file extension."),
            SimpleOptionValue.Create(NamePartKind.FullName, description: "Search in full path."),
            SimpleOptionValue.Create(NamePartKind.Name, description: "Search in file name and its extension."),
            SimpleOptionValue.Create(NamePartKind.NameWithoutExtension, shortValue: "w", helpValue: "name-[w]ithout-extension", description: "Search in file name without extension.")
        );

        public static OptionValueProvider SyntaxSectionProvider { get; } = new OptionValueProvider(MetaValues.SyntaxSections,
            SimpleOptionValue.Create(SyntaxSection.AlternationConstructs, shortValue: "ac", helpValue: "[a]lternation-[c]onstructs"),
            SimpleOptionValue.Create(SyntaxSection.Anchors),
            SimpleOptionValue.Create(SyntaxSection.BackreferenceConstructs),
            SimpleOptionValue.Create(SyntaxSection.CharacterClasses),
            SimpleOptionValue.Create(SyntaxSection.CharacterEscapes, shortValue: "ce", helpValue: "[c]haracter-[e]scapes"),
            SimpleOptionValue.Create(SyntaxSection.GeneralCategories, shortValue: "gc", helpValue: "[g]eneral-[c]ategories"),
            SimpleOptionValue.Create(SyntaxSection.GroupingConstructs),
            SimpleOptionValue.Create(SyntaxSection.Miscellaneous),
            SimpleOptionValue.Create(SyntaxSection.NamedBlocks),
            SimpleOptionValue.Create(SyntaxSection.Options),
            SimpleOptionValue.Create(SyntaxSection.Quantifiers),
            SimpleOptionValue.Create(SyntaxSection.Substitutions)
        );

        public static OptionValueProvider HighlightOptionsProvider { get; } = new OptionValueProvider(MetaValues.Highlight,
            SimpleOptionValue.Create(HighlightOptions.None, description: "No highlighting."),
            SimpleOptionValue.Create(HighlightOptions.Match, description: "Highlight match value."),
            SimpleOptionValue.Create(HighlightOptions.Replacement, description: "Highlight replacement value."),
            SimpleOptionValue.Create(HighlightOptions.Split, description: "Highlight split value."),
            SimpleOptionValue.Create(HighlightOptions.EmptyMatch, shortValue: "em", helpValue: "[e]mpty-[m]atch", description: "Highlight match value that is empty string."),
            SimpleOptionValue.Create(HighlightOptions.EmptyReplacement, shortValue: "er", helpValue: "[e]mpty-[r]eplacement", description: "Highlight replacement value that is empty string."),
            SimpleOptionValue.Create(HighlightOptions.EmptySplit, shortValue: "es", helpValue: "[e]mpty-[s]plit", description: "Highlight split value that is empty string."),
            SimpleOptionValue.Create(HighlightOptions.Empty, description: "Highlight value that is empty string."),
            SimpleOptionValue.Create(HighlightOptions.Boundary, description: "Highlight start and end of the value."),
            SimpleOptionValue.Create(HighlightOptions.Tab, description: "Highlight tab character."),
            SimpleOptionValue.Create(HighlightOptions.CarriageReturn, shortValue: "cr", helpValue: "[c]arriage-[r]eturn", description: "Highlight carriage return character."),
            SimpleOptionValue.Create(HighlightOptions.Linefeed, shortValue: "lf", helpValue: "[l]ine[f]eed", description: "Highlight linefeed character."),
            SimpleOptionValue.Create(HighlightOptions.NewLine, shortValue: "nl", helpValue: "[n]ew-[l]ine", description: "Highlight carriage return and linefeed characters."),
            SimpleOptionValue.Create(HighlightOptions.Space, shortValue: "", description: "Highlight space character.")
        );

        public static OptionValueProvider DeleteHighlightOptionsProvider { get; } = new OptionValueProvider(OptionValueProviderNames.DeleteHighlightOptions,
            HighlightOptionsProvider.Values.Where(f =>
            {
                switch (f.Name)
                {
                    case nameof(HighlightOptions.EmptyMatch):
                    case nameof(HighlightOptions.Replacement):
                    case nameof(HighlightOptions.EmptyReplacement):
                    case nameof(HighlightOptions.Split):
                    case nameof(HighlightOptions.Boundary):
                    case nameof(HighlightOptions.Tab):
                    case nameof(HighlightOptions.Linefeed):
                    case nameof(HighlightOptions.CarriageReturn):
                    case nameof(HighlightOptions.NewLine):
                    case nameof(HighlightOptions.Space):
                        return false;
                }

                return true;
            })
        );

        public static OptionValueProvider FindHighlightOptionsProvider { get; } = new OptionValueProvider(OptionValueProviderNames.FindHighlightOptions,
            HighlightOptionsProvider.Values.Where(f =>
            {
                switch (f.Name)
                {
                    case nameof(HighlightOptions.Replacement):
                    case nameof(HighlightOptions.EmptyReplacement):
                    case nameof(HighlightOptions.Split):
                        return false;
                }

                return true;
            })
        );

        public static OptionValueProvider MatchHighlightOptionsProvider { get; } = new OptionValueProvider(OptionValueProviderNames.MatchHighlightOptions,
            HighlightOptionsProvider.Values.Where(f =>
            {
                switch (f.Name)
                {
                    case nameof(HighlightOptions.Replacement):
                    case nameof(HighlightOptions.EmptyReplacement):
                    case nameof(HighlightOptions.Split):
                        return false;
                }

                return true;
            })
        );

        public static OptionValueProvider RenameHighlightOptionsProvider { get; } = new OptionValueProvider(OptionValueProviderNames.RenameHighlightOptions,
            HighlightOptionsProvider.Values.Where(f =>
            {
                switch (f.Name)
                {
                    case nameof(HighlightOptions.EmptyMatch):
                    case nameof(HighlightOptions.EmptyReplacement):
                    case nameof(HighlightOptions.Split):
                    case nameof(HighlightOptions.Boundary):
                    case nameof(HighlightOptions.Tab):
                    case nameof(HighlightOptions.Linefeed):
                    case nameof(HighlightOptions.CarriageReturn):
                    case nameof(HighlightOptions.NewLine):
                    case nameof(HighlightOptions.Space):
                        return false;
                }

                return true;
            })
        );

        public static OptionValueProvider ReplaceHighlightOptionsProvider { get; } = new OptionValueProvider(OptionValueProviderNames.ReplaceHighlightOptions,
            HighlightOptionsProvider.Values.Where(f => f.Name != nameof(HighlightOptions.Split))
        );

        public static OptionValueProvider SplitHighlightOptionsProvider { get; } = new OptionValueProvider(OptionValueProviderNames.SplitHighlightOptions,
            HighlightOptionsProvider.Values.Where(f =>
            {
                switch (f.Name)
                {
                    case nameof(HighlightOptions.Match):
                    case nameof(HighlightOptions.Replacement):
                    case nameof(HighlightOptions.EmptyMatch):
                    case nameof(HighlightOptions.EmptyReplacement):
                        return false;
                }

                return true;
            })
        );

        public static OptionValueProvider FileLogOptionsProvider { get; } = new OptionValueProvider(MetaValues.LogOptions,
            OptionValues.Verbosity,
            SimpleOptionValue.Create(FileLogFlags.Append, description: "If the log file exists logging will start at the end of the file.")
        );

        public static OptionValueProvider ContentDisplayStyleProvider { get; } = new OptionValueProvider(MetaValues.ContentDisplay,
            SimpleOptionValue.Create(ContentDisplayStyle.Value, description: "Display just the matched value."),
            SimpleOptionValue.Create(ContentDisplayStyle.ValueDetail, shortValue: "d", helpValue: "value-[d]etail", description: "Display matched value and include information about each value such as index or length."),
            SimpleOptionValue.Create(ContentDisplayStyle.Line, description: "Display entire line containing the matched value."),
            SimpleOptionValue.Create(ContentDisplayStyle.UnmatchedLines, description: "Display all lines that do not contain the matched value."),
            SimpleOptionValue.Create(ContentDisplayStyle.AllLines, description: "Display all lines.")
        );

        public static OptionValueProvider ContentDisplayStyleProvider_WithoutUnmatchedLines { get; } = new OptionValueProvider(OptionValueProviderNames.ContentDisplayStyle_WithoutUnmatchedLines,
            ContentDisplayStyleProvider.Values.Where(f => f.Name != nameof(ContentDisplayStyle.UnmatchedLines))
        );

        public static OptionValueProvider ContentDisplayStyleProvider_WithoutLineAndUnmatchedLinesAndOmit { get; } = new OptionValueProvider(OptionValueProviderNames.ContentDisplayStyle_WithoutLineAndUnmatchedLines,
            ContentDisplayStyleProvider.Values.Where(f => f.Name != nameof(ContentDisplayStyle.Line) && f.Name != nameof(ContentDisplayStyle.UnmatchedLines))
        );

        public static OptionValueProvider AskModeProvider { get; } = new OptionValueProvider(MetaValues.AskMode,
            SimpleOptionValue.Create(AskMode.File, description: "Ask for confirmation after each file."),
            SimpleOptionValue.Create(AskMode.Value, description: "Ask for confirmation after each value.")
        );

        public static OptionValueProvider MaxOptionsProvider { get; } = new OptionValueProvider(MetaValues.MaxOptions,
            SimpleOptionValue.Create("MatchesInFile", "<NUM>", shortValue: "", description: "Stop searching after <NUM> matches (or matches in file when searching in file's content)."),
            OptionValues.MaxMatches,
            OptionValues.MaxMatchingFiles
        );

        public static OptionValueProvider PathDisplayStyleProvider { get; } = new OptionValueProvider(MetaValues.PathDisplay,
            SimpleOptionValue.Create(PathDisplayStyle.Full, description: "Display full path."),
            SimpleOptionValue.Create(PathDisplayStyle.Relative, description: "Display path relatively to the base directory."),
            SimpleOptionValue.Create(PathDisplayStyle.Omit, description: "Do not display path.")
        );

        public static OptionValueProvider DisplayProvider { get; } = new OptionValueProvider(MetaValues.DisplayOptions,
            OptionValues.Display_Content,
            OptionValues.Display_Count,
            OptionValues.Display_CreationTime,
            OptionValues.Display_ModifiedTime,
            OptionValues.Display_Path,
            OptionValues.Display_Size,
            OptionValues.Display_Summary
        );

        public static OptionValueProvider OutputOptionsProvider { get; } = new OptionValueProvider(MetaValues.OutputOptions,
            OptionValues.OutputOptions_Content,
            OptionValues.OutputOptions_Path,
            OptionValues.Encoding
        );

        public static OptionValueProvider SortFlagsProvider { get; } = new OptionValueProvider(MetaValues.SortOptions,
            SimpleOptionValue.Create(SortFlags.Ascending, description: "Sort items in ascending order."),
            SimpleOptionValue.Create(SortFlags.CreationTime, description: "Sort items by creation time."),
            SimpleOptionValue.Create(SortFlags.Descending, description: "Sort items in descending order."),
            OptionValues.Max,
            SimpleOptionValue.Create(SortFlags.ModifiedTime, description: "Sort items by last modified time."),
            SimpleOptionValue.Create(SortFlags.Name, description: "Sort items by full name."),
            SimpleOptionValue.Create(SortFlags.Size, description: "Sort items by size.")
        );

        public static OptionValueProvider FilePropertiesProvider { get; } = new OptionValueProvider(MetaValues.FileProperties,
            OptionValues.FileProperty_CreationTime,
            OptionValues.FileProperty_ModifiedTime,
            OptionValues.FileProperty_Size
        );

        public static ImmutableDictionary<string, OptionValueProvider> ProvidersByName
        {
            get
            {
                if (_providersByName == null)
                    Interlocked.CompareExchange(ref _providersByName, LoadProviders(), null);

                return _providersByName;

                ImmutableDictionary<string, OptionValueProvider> LoadProviders()
                {
                    PropertyInfo[] fieldInfo = typeof(OptionValueProviders).GetProperties();

                    return fieldInfo
                        .Where(f => f.PropertyType.Equals(typeof(OptionValueProvider)))
                        .OrderBy(f => f.Name)
                        .Select(f => (OptionValueProvider)f.GetValue(null))
                        .ToImmutableDictionary(f => f.Name, f => f);
                }
            }
        }
    }
}
