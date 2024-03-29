﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Orang.CommandLine.Help;
using Orang.FileSystem;
using Orang.Syntax;

namespace Orang.CommandLine;

internal static class OptionValueProviders
{
    private static ImmutableDictionary<string, OptionValueProvider>? _providersByName;

    public static OptionValueProvider PatternOptionsProvider { get; } = new(
        MetaValues.PatternOptions,
        OptionValues.PatternOptions_Compiled,
        OptionValues.PatternOptions_CultureInvariant,
        OptionValues.PatternOptions_ECMAScript,
        OptionValues.PatternOptions_EndsWith,
        OptionValues.PatternOptions_Equals,
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
        OptionValues.PatternOptions_StartsWith,
        OptionValues.Timeout,
        OptionValues.PatternOptions_WholeLine,
        OptionValues.PatternOptions_WholeWord
    );

    public static OptionValueProvider PatternOptions_List_Provider { get; } = new(
        OptionValueProviderNames.PatternOptions_List,
        OptionValues.PatternOptions_CaseSensitive,
        OptionValues.PatternOptions_Compiled,
        OptionValues.PatternOptions_CultureInvariant,
        OptionValues.PatternOptions_ECMAScript,
        OptionValues.PatternOptions_ExplicitCapture,
        OptionValues.PatternOptions_FromFile,
        OptionValues.Group,
        OptionValues.PatternOptions_IgnorePatternWhitespace,
        OptionValues.PatternOptions_List,
        OptionValues.ListSeparator,
        OptionValues.PatternOptions_Literal,
        OptionValues.PatternOptions_Multiline,
        OptionValues.PatternOptions_Negative,
        OptionValues.PatternOptions_RightToLeft,
        OptionValues.PatternOptions_Singleline,
        OptionValues.Timeout,
        OptionValues.PatternOptions_WholeLine
    );

    public static OptionValueProvider PatternOptionsWithoutPartProvider { get; }
        = PatternOptionsProvider.WithoutValues(
            OptionValueProviderNames.PatternOptionsWithoutPart,
            OptionValues.Part);

    public static OptionValueProvider PatternOptionsWithoutGroupAndNegativeProvider { get; }
        = PatternOptionsProvider.WithoutValues(
            OptionValueProviderNames.PatternOptionsWithoutGroupAndNegative,
            OptionValues.Group,
            OptionValues.PatternOptions_Negative);

    public static OptionValueProvider PatternOptions_Match_Provider { get; }
        = PatternOptionsProvider.WithoutValues(
            OptionValueProviderNames.PatternOptions_Match,
            OptionValues.Part,
            OptionValues.PatternOptions_Negative);

    public static OptionValueProvider PatternOptionsWithoutGroupAndPartAndNegativeProvider { get; }
        = PatternOptionsProvider.WithoutValues(
            OptionValueProviderNames.PatternOptionsWithoutGroupAndPartAndNegative,
            OptionValues.Group,
            OptionValues.Part,
            OptionValues.PatternOptions_Negative);

    public static OptionValueProvider ExtensionOptionsProvider { get; } = new(
        MetaValues.ExtensionOptions,
        OptionValues.PatternOptions_CaseSensitive,
        OptionValues.PatternOptions_CultureInvariant,
        OptionValues.PatternOptions_FromFile,
        OptionValues.ListSeparator,
        OptionValues.PatternOptions_Literal,
        OptionValues.PatternOptions_Negative,
        OptionValues.Timeout
    );

    public static OptionValueProvider RegexOptionsProvider { get; } = new(
        MetaValues.RegexOptions,
        SimpleOptionValue.Create(RegexOptions.Compiled, description: "Compile the regular expression to an assembly."),
        SimpleOptionValue.Create(
            RegexOptions.CultureInvariant,
            shortValue: "ci",
            helpValue: "c[ulture-]i[nvariant]",
            description: "Ignore cultural differences between languages."),
        SimpleOptionValue.Create(
            RegexOptions.ECMAScript,
            value: "ecma-script",
            shortValue: "es",
            helpValue: "e[cma-]s[cript]",
            description: "Enable ECMAScript-compliant behavior for the expression."),
        SimpleOptionValue.Create(
            RegexOptions.ExplicitCapture,
            shortValue: "n",
            description: "Do not capture unnamed groups."),
        SimpleOptionValue.Create(RegexOptions.IgnoreCase, description: "Use case-insensitive matching."),
        SimpleOptionValue.Create(
            RegexOptions.IgnorePatternWhitespace,
            shortValue: "x",
            description: "Exclude unescaped white-space from the pattern and enable comments after a number sign (#)."),
        SimpleOptionValue.Create(
            RegexOptions.Multiline,
            description: "^ and $ match the beginning and end of each line "
                + "(instead of the beginning and end of the input string)."),
        SimpleOptionValue.Create(
            RegexOptions.RightToLeft,
            description: "Specifies that the search will be from right to left."),
        SimpleOptionValue.Create(
            RegexOptions.Singleline,
            description: "The period (.) matches every character (instead of every character except \\n).")
    );

    public static OptionValueProvider ModifierOptionsProvider { get; } = new(
        MetaValues.ModifierOptions,
        SimpleOptionValue.Create(
            ModifierOptions.FromDll,
            shortValue: "",
            description: "<MODIFIER> is a path to a method in DLL file. "
                + "The format is 'DllPath,FullTypeName.MethodName'."),
        SimpleOptionValue.Create(
            ModifierOptions.FromFile,
            description: "Load text from a file whose path is specified in <MODIFIER> value."));

    public static OptionValueProvider ReplacementOptionsProvider { get; } = new(
        MetaValues.ReplacementOptions,
        SimpleOptionValue.Create(
            ReplacementOptions.FromFile,
            description: "Load text from a file whose path is specified in <REPLACEMENT> value."),
        SimpleOptionValue.Create(
            ReplacementOptions.Literal,
            description: "Replacement should be treated as a literal expression and not as a replacement expression."),
        SimpleOptionValue.Create(
            ReplacementOptions.Escape,
            description: @"Interpret literals \a, \b, \f, \n, \r, \t and \v as character escapes."),
        SimpleOptionValue.Create(
            ReplacementOptions.CSharp,
            shortValue: "cs",
            description:
                "<REPLACEMENT> is either expression-body of a method with signature "
                    + "'string M(Match match)'"
                    + Environment.NewLine
                    + "or a path to a code file that contains public method with signature 'string M(Match match)'."
                    + Environment.NewLine
                    + "Imported namespaces (when inline expression is specified):"
                    + Environment.NewLine
                    + "  System"
                    + Environment.NewLine
                    + "  System.Collections.Generic"
                    + Environment.NewLine
                    + "  System.Linq"
                    + Environment.NewLine
                    + "  System.Text"
                    + Environment.NewLine
                    + "  System.Text.RegularExpressions"),
        OptionValues.ReplacementOptions_FromDll
    );

    public static OptionValueProvider InputOptionsProvider { get; } = new(
        MetaValues.InputOptions,
        SimpleOptionValue.Create(
            InputOptions.Escape,
            description: @"Interpret literals \a, \b, \f, \n, \r, \t and \v as character escapes.")
    );

    public static OptionValueProvider VerbosityProvider { get; } = new(
        MetaValues.Verbosity,
        SimpleOptionValue.Create(Verbosity.Quiet),
        SimpleOptionValue.Create(Verbosity.Minimal),
        SimpleOptionValue.Create(Verbosity.Normal),
        SimpleOptionValue.Create(Verbosity.Detailed),
        SimpleOptionValue.Create(Verbosity.Diagnostic, shortValue: "di")
    );

    public static OptionValueProvider FileSystemAttributesProvider { get; } = new(
        MetaValues.Attributes,
        SimpleOptionValue.Create(FileSystemAttributes.Archive, shortValue: ""),
        SimpleOptionValue.Create(FileSystemAttributes.Compressed, shortValue: ""),
        OptionValues.FileSystemAttributes_Directory,
        SimpleOptionValue.Create(FileSystemAttributes.Empty),
        SimpleOptionValue.Create(FileSystemAttributes.Encrypted, shortValue: ""),
        OptionValues.FileSystemAttributes_File,
        SimpleOptionValue.Create(FileSystemAttributes.Hidden),
        SimpleOptionValue.Create(FileSystemAttributes.IntegrityStream, shortValue: "", hidden: true),
        SimpleOptionValue.Create(FileSystemAttributes.Normal, shortValue: ""),
        SimpleOptionValue.Create(FileSystemAttributes.NoScrubData, shortValue: "", hidden: true),
        SimpleOptionValue.Create(FileSystemAttributes.NotContentIndexed, shortValue: "", hidden: true),
        SimpleOptionValue.Create(FileSystemAttributes.Offline, shortValue: ""),
        SimpleOptionValue.Create(FileSystemAttributes.ReadOnly),
        SimpleOptionValue.Create(FileSystemAttributes.ReparsePoint, shortValue: "rp", helpValue: "r[eparse-]p[oint]"),
        SimpleOptionValue.Create(FileSystemAttributes.SparseFile, shortValue: "", hidden: true),
        SimpleOptionValue.Create(FileSystemAttributes.System),
        SimpleOptionValue.Create(FileSystemAttributes.Temporary, shortValue: "")
    );

    public static OptionValueProvider FileSystemAttributesToSkipProvider { get; }
        = FileSystemAttributesProvider.WithoutValues(
            OptionValueProviderNames.FileSystemAttributesToSkip,
            OptionValues.FileSystemAttributes_Directory,
            OptionValues.FileSystemAttributes_File);

    public static OptionValueProvider NamePartKindProvider { get; } = new(
        MetaValues.NamePart,
        OptionValues.NamePart_Extension,
        OptionValues.NamePart_FullName,
        OptionValues.NamePart_Name,
        OptionValues.NamePart_NameWithoutExtension
    );

    public static OptionValueProvider NamePartKindProvider_WithoutFullName { get; }
        = NamePartKindProvider.WithoutValues(
            OptionValueProviderNames.NamePart_WithoutFullName,
            OptionValues.NamePart_FullName
    );

    public static OptionValueProvider NamePartKindProvider_WithoutExtension { get; }
        = NamePartKindProvider.WithoutValues(
            OptionValueProviderNames.NamePart_WithoutExtension,
            OptionValues.NamePart_Extension
    );

    public static OptionValueProvider SyntaxSectionProvider { get; } = new(
        MetaValues.SyntaxSections,
        SimpleOptionValue.Create(
            SyntaxSection.AlternationConstructs,
            shortValue: "ac",
            helpValue: "a[lternation-]c[onstructs]"),
        SimpleOptionValue.Create(SyntaxSection.Anchors),
        SimpleOptionValue.Create(SyntaxSection.BackreferenceConstructs),
        SimpleOptionValue.Create(SyntaxSection.CharacterClasses),
        SimpleOptionValue.Create(SyntaxSection.CharacterEscapes, shortValue: "ce", helpValue: "c[haracter-]e[scapes]"),
        SimpleOptionValue.Create(SyntaxSection.GeneralCategories, shortValue: "gc", helpValue: "g[eneral-]c[ategories]"),
        SimpleOptionValue.Create(SyntaxSection.GroupingConstructs),
        SimpleOptionValue.Create(SyntaxSection.Miscellaneous),
        SimpleOptionValue.Create(SyntaxSection.NamedBlocks),
        SimpleOptionValue.Create(SyntaxSection.Options),
        SimpleOptionValue.Create(SyntaxSection.RegexOptions, shortValue: "ro", helpValue: "r[egex-]o[ptions]"),
        SimpleOptionValue.Create(SyntaxSection.Quantifiers),
        SimpleOptionValue.Create(SyntaxSection.Substitutions),
        SimpleOptionValue.Create(SyntaxSection.All, shortValue: "")
    );

    public static OptionValueProvider ModifyFlagsProvider { get; } = new(
        MetaValues.ModifyOptions,
        OptionValues.ModifyFlags_Aggregate,
        OptionValues.ModifyFlags_AggregateOnly,
        OptionValues.ModifyOptions_Ascending,
        SimpleOptionValue.Create(
            ModifyFlags.CultureInvariant,
            shortValue: "ci",
            description: "Ignore cultural differences between languages."),
        OptionValues.ModifyOptions_Descending,
        SimpleOptionValue.Create(ModifyFlags.Distinct, shortValue: "di", description: "Return distinct values."),
        OptionValues.ModifyFlags_Except,
        OptionValues.ModifyFlags_Intersect,
        OptionValues.ModifyFlags_Group,
        OptionValues.ModifyFlags_Count,
        SimpleOptionValue.Create(ModifyFlags.IgnoreCase, description: "Use case-insensitive matching."),
        SimpleOptionValue.Create(
            ModifyFlags.RemoveEmpty,
            shortValue: "re",
            description: "Remove values that are empty strings."),
        SimpleOptionValue.Create(
            ModifyFlags.RemoveWhiteSpace,
            shortValue: "rw",
            description: "Remove values that are empty or consist of white-space."),
        OptionValues.SortBy,
        OptionValues.ToLower,
        OptionValues.ToUpper,
        OptionValues.Trim,
        OptionValues.TrimEnd,
        OptionValues.TrimStart
    );

    public static OptionValueProvider FunctionFlagsProvider { get; } = new(
        MetaValues.Function,
        OptionValues.FunctionFlags_Sort,
        OptionValues.FunctionFlags_SortDescending,
        SimpleOptionValue.Create(ModifyFlags.Distinct, shortValue: "", description: "Return distinct values."),
        OptionValues.ModifyFlags_Group,
        OptionValues.SortBy
    );

    public static OptionValueProvider HighlightOptionsProvider { get; } = new(
        MetaValues.Highlight,
        OptionValues.HighlightOptions_None,
        OptionValues.HighlightOptions_Match,
        OptionValues.HighlightOptions_Replacement,
        OptionValues.HighlightOptions_Split,
        OptionValues.HighlightOptions_EmptyMatch,
        OptionValues.HighlightOptions_EmptyReplacement,
        OptionValues.HighlightOptions_EmptySplit,
        OptionValues.HighlightOptions_Empty,
        OptionValues.HighlightOptions_Boundary,
        OptionValues.HighlightOptions_Tab,
        OptionValues.HighlightOptions_CarriageReturn,
        OptionValues.HighlightOptions_Linefeed,
        OptionValues.HighlightOptions_Newline,
        OptionValues.HighlightOptions_Space
    );

    public static OptionValueProvider DeleteHighlightOptionsProvider { get; } = new(
        OptionValueProviderNames.DeleteHighlightOptions,
        HighlightOptionsProvider,
        OptionValues.HighlightOptions_None,
        OptionValues.HighlightOptions_Match,
        OptionValues.HighlightOptions_Empty
    );

    public static OptionValueProvider FindHighlightOptionsProvider { get; }
        = HighlightOptionsProvider.WithoutValues(
            OptionValueProviderNames.FindHighlightOptions,
            OptionValues.HighlightOptions_Replacement,
            OptionValues.HighlightOptions_EmptyReplacement,
            OptionValues.HighlightOptions_Split,
            OptionValues.HighlightOptions_EmptySplit
    );

    public static OptionValueProvider MatchHighlightOptionsProvider { get; }
        = HighlightOptionsProvider.WithoutValues(
            OptionValueProviderNames.MatchHighlightOptions,
            OptionValues.HighlightOptions_Replacement,
            OptionValues.HighlightOptions_EmptyReplacement,
            OptionValues.HighlightOptions_Split,
            OptionValues.HighlightOptions_EmptySplit
    );

    public static OptionValueProvider RenameHighlightOptionsProvider { get; } = new(
        OptionValueProviderNames.RenameHighlightOptions,
        HighlightOptionsProvider,
        OptionValues.HighlightOptions_None,
        OptionValues.HighlightOptions_Match,
        OptionValues.HighlightOptions_Replacement,
        OptionValues.HighlightOptions_Empty
    );

    public static OptionValueProvider ReplaceHighlightOptionsProvider { get; }
        = HighlightOptionsProvider.WithoutValues(
            OptionValueProviderNames.ReplaceHighlightOptions,
            OptionValues.HighlightOptions_Split,
            OptionValues.HighlightOptions_EmptySplit);

    public static OptionValueProvider SplitHighlightOptionsProvider { get; }
        = HighlightOptionsProvider.WithoutValues(
            OptionValueProviderNames.SplitHighlightOptions,
            OptionValues.HighlightOptions_Match,
            OptionValues.HighlightOptions_Replacement,
            OptionValues.HighlightOptions_EmptyMatch,
            OptionValues.HighlightOptions_EmptyReplacement
    );

    public static OptionValueProvider OutputFlagsProvider { get; } = new(
        MetaValues.OutputOptions,
        OptionValues.Encoding,
        OptionValues.Verbosity,
        OptionValues.Output_Append
    );

    public static OptionValueProvider ContentDisplayStyleProvider { get; } = new(
        MetaValues.ContentMode,
        OptionValues.ContentDisplayStyle_AllLines,
        OptionValues.ContentDisplayStyle_Line,
        OptionValues.ContentDisplayStyle_UnmatchedLines,
        OptionValues.ContentDisplayStyle_Value,
        OptionValues.ContentDisplayStyle_ValueDetail,
        OptionValues.ContentDisplayStyle_Omit
    );

    public static OptionValueProvider ContentDisplayStyleProvider_WithoutUnmatchedLines { get; }
        = ContentDisplayStyleProvider.WithoutValues(
            OptionValueProviderNames.ContentDisplayStyle_WithoutUnmatchedLines,
            OptionValues.ContentDisplayStyle_UnmatchedLines
    );

    public static OptionValueProvider ContentDisplayStyleProvider_WithoutLineAndUnmatchedLines { get; }
        = ContentDisplayStyleProvider.WithoutValues(
            OptionValueProviderNames.ContentDisplayStyle_WithoutLineAndUnmatchedLinesAndOmit,
            OptionValues.ContentDisplayStyle_Line,
            OptionValues.ContentDisplayStyle_UnmatchedLines,
            OptionValues.ContentDisplayStyle_Omit
    );

    public static OptionValueProvider AskModeProvider { get; } = new(
        MetaValues.AskMode,
        SimpleOptionValue.Create(AskMode.File, description: "Ask for confirmation after each file."),
        SimpleOptionValue.Create(AskMode.Value, description: "Ask for confirmation after each value.")
    );

    public static OptionValueProvider PathDisplayStyleProvider { get; } = new(
        MetaValues.PathMode,
        OptionValues.PathDisplayStyle_Full,
        OptionValues.PathDisplayStyle_Relative,
        OptionValues.PathDisplayStyle_Match,
        OptionValues.PathDisplayStyle_Omit
    );

    public static OptionValueProvider PathDisplayStyleProvider_Rename { get; } = new(
        OptionValueProviderNames.PathDisplayStyle_Rename,
        PathDisplayStyleProvider,
        OptionValues.PathDisplayStyle_Full,
        OptionValues.PathDisplayStyle_Relative,
        OptionValues.PathDisplayStyle_Omit
    );

    public static OptionValueProvider SortFlagsProvider { get; } = new(
        MetaValues.SortOptions,
        OptionValues.SortFlags_Ascending,
        SimpleOptionValue.Create(
            SortFlags.CreationTime,
            shortValue: "ct",
            helpValue: "c[reation-]t[ime]",
            description: "Sort items by creation time."),
        SimpleOptionValue.Create(
            SortFlags.CultureInvariant,
            shortValue: "ci",
            description: "Ignore cultural differences between languages."),
        OptionValues.SortFlags_Descending,
        OptionValues.MaxCount,
        SimpleOptionValue.Create(
            SortFlags.ModifiedTime,
            shortValue: "mt",
            helpValue: "m[odified-]t[ime]",
            description: "Sort items by last modified time."),
        SimpleOptionValue.Create(SortFlags.Name, description: "Sort items by full name."),
        SimpleOptionValue.Create(SortFlags.Size, description: "Sort items by size.")
    );

    public static OptionValueProvider FilePropertiesProvider { get; } = new(
        MetaValues.FileProperties,
        OptionValues.FileProperty_CreationTime,
        OptionValues.FileProperty_ModifiedTime,
        OptionValues.FileProperty_Size
    );

    public static OptionValueProvider ValueSortPropertyProvider { get; } = new(
        MetaValues.SortProperty,
        SimpleOptionValue.Create(ValueSortProperty.Length, description: "Sort values by value's length."),
        SimpleOptionValue.Create(ValueSortProperty.Count, description: "Sort values by group's count.")
    );

    public static OptionValueProvider ReplaceFlagsProvider { get; } = new(
        MetaValues.ReplaceModify,
        SimpleOptionValue.Create(
            ReplaceFlags.CultureInvariant,
            shortValue: "ci",
            description: "Ignore cultural differences between languages."),
        OptionValues.ToLower,
        OptionValues.ToUpper,
        OptionValues.Trim,
        OptionValues.TrimEnd,
        OptionValues.TrimStart
    );

    public static OptionValueProvider ConflictResolutionProvider { get; } = new(
        MetaValues.ConflictResolution,
        OptionValues.ConflictResolution_Ask,
        OptionValues.ConflictResolution_Overwrite,
        OptionValues.ConflictResolution_Suffix,
        OptionValues.ConflictResolution_Skip
    );

    public static OptionValueProvider ConflictResolutionProvider_WithoutSuffix { get; }
        = ConflictResolutionProvider.WithoutValues(
            OptionValueProviderNames.ConflictResolution_WithoutSuffix,
            OptionValues.ConflictResolution_Suffix
    );

    public static OptionValueProvider ConflictResolutionProvider_Sync { get; } = new(
        OptionValueProviderNames.ConflictResolution_Sync,
        OptionValues.ConflictResolution_Ask,
        OptionValues.ConflictResolution_Overwrite,
        OptionValues.ConflictResolution_Skip
    );

    public static OptionValueProvider FileCompareOptionsProvider { get; } = new(
        MetaValues.CompareOptions,
        SimpleOptionValue.Create(FileCompareProperties.None, description: "Compare files only by name."),
        SimpleOptionValue.Create(FileCompareProperties.Attributes, description: "Compare file attributes."),
        SimpleOptionValue.Create(FileCompareProperties.Content, description: "Compare file content."),
        SimpleOptionValue.Create(
            FileCompareProperties.ModifiedTime,
            shortValue: "mt",
            helpValue: "m[odified-]t[ime]",
            description: "Compare time a file was last modified."),
        SimpleOptionValue.Create(FileCompareProperties.Size, description: "Compare file size.")
    );

    public static OptionValueProvider SyncConflictResolutionProvider { get; } = new(
        MetaValues.SyncConflictResolution,
        SimpleOptionValue.Create(SyncConflictResolution.Ask, description: ""),
        SimpleOptionValue.Create(SyncConflictResolution.FirstWins, description: ""),
        SimpleOptionValue.Create(SyncConflictResolution.SecondWins, description: "")
    );

    public static OptionValueProvider PipeMode { get; } = new(
        MetaValues.PipeMode,
        SimpleOptionValue.Create(
            CommandLine.PipeMode.Text,
            description: "Use redirected input as a text to be searched."),
        SimpleOptionValue.Create(
            CommandLine.PipeMode.Paths,
            description: "Use redirected input as a list of paths separated with newlines."));

    public static ImmutableDictionary<string, OptionValueProvider> ProvidersByName
    {
        get
        {
            if (_providersByName is null)
                Interlocked.CompareExchange(ref _providersByName, LoadProviders(), null);

            return _providersByName;

            static ImmutableDictionary<string, OptionValueProvider> LoadProviders()
            {
                PropertyInfo[] fieldInfo = typeof(OptionValueProviders).GetProperties();

                return fieldInfo
                    .Where(f => f.PropertyType.Equals(typeof(OptionValueProvider)))
                    .OrderBy(f => f.Name)
                    .Select(f => (OptionValueProvider)f.GetValue(null)!)
                    .ToImmutableDictionary(f => f.Name, f => f);
            }
        }
    }

    public static IEnumerable<OptionValueProvider> Providers => ProvidersByName.Select(f => f.Value);

    public static IEnumerable<OptionValueProvider> GetProviders(
        IEnumerable<CommandOption> options,
        IEnumerable<OptionValueProvider>? allProviders = null)
    {
        IEnumerable<string> metaValues = options
            .Where(f => !string.IsNullOrEmpty(f.Description))
            .SelectMany(f => OptionValueProvider.MetaValueRegex.Matches(f.Description!).Select(m => m.Value))
            .Concat(options.Where(f => f.MetaValue is not null).Select(f => f.MetaValue).Cast<string>())
            .Distinct();

        ImmutableArray<OptionValueProvider> providers = metaValues
            .Join(allProviders ?? ProvidersByName.Select(f => f.Value), f => f, f => f.Name, (_, f) => f)
            .ToImmutableArray();

        return providers
            .SelectMany(f => f.Values)
            .OfType<KeyValuePairOptionValue>()
            .Select(f => f.Value)
            .Distinct()
            .Join(allProviders ?? ProvidersByName.Select(f => f.Value), f => f, f => f.Name, (_, f) => f)
            .Concat(providers)
            .Distinct()
            .OrderBy(f => f.Name);
    }

    public static string? GetHelpText(
        OptionValueProvider? provider,
        Func<OptionValue, bool>? predicate = null,
        bool multiline = false)
    {
        if (provider is null)
            return null;

        ImmutableArray<OptionValue> Values = provider.Values;

        if (multiline)
        {
            IEnumerable<OptionValue> optionValues = (predicate is not null)
                ? Values.Where(predicate)
                : Values;

            StringBuilder sb = StringBuilderCache.GetInstance();

            int width = HelpProvider.CalculateOptionValuesWidth(optionValues);

            ImmutableArray<OptionValueItem>.Enumerator en = HelpProvider.GetOptionValueItems(
                optionValues,
                width)
                .GetEnumerator();

            if (en.MoveNext())
            {
                while (true)
                {
                    sb.Append("  ");
                    sb.Append(TextHelpers.Indent(en.Current.Text, "  "));

                    if (en.MoveNext())
                    {
                        sb.AppendLine();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return StringBuilderCache.GetStringAndFree(sb);
        }
        else
        {
            IEnumerable<string> values = (predicate is not null)
                ? Values.Where(predicate).Select(f => f.HelpValue)
                : Values.Select(f => f.HelpValue);

            return TextHelpers.Join(", ", " and ", values);
        }
    }
}
