// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Orang.CommandLine.Help;
using Orang.Expressions;
using Orang.FileSystem;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

internal class ParseContext
{
    private static readonly char[] _equalsOrLessThanOrGreaterThanChars = new[] { '=', '<', '>' };

    public Logger Logger { get; }

    public ParseContext(Logger logger)
    {
        Logger = logger;
    }

    public void WriteWarning(string message)
    {
        Logger.WriteLine(message);
    }

    public void WriteError(Exception exception)
    {
        Logger.WriteError(exception);
    }

    public void WriteError(string message)
    {
        Logger.WriteError(message);
    }

    public bool TryParseHighlightOptions(
        IEnumerable<string> values,
        out HighlightOptions highlightOptions,
        HighlightOptions? defaultValue = null,
        OptionValueProvider? provider = null)
    {
        return TryParseHighlightOptions(
            values,
            out highlightOptions,
            defaultValue,
            default(ContentDisplayStyle?),
            provider);
    }

    public bool TryParseHighlightOptions(
        IEnumerable<string> values,
        out HighlightOptions highlightOptions,
        HighlightOptions? defaultValue = null,
        ContentDisplayStyle? contentDisplayStyle = null,
        OptionValueProvider? provider = null)
    {
        if (contentDisplayStyle == ContentDisplayStyle.Value
            || contentDisplayStyle == ContentDisplayStyle.ValueDetail)
        {
            defaultValue = HighlightOptions.None;
        }

        if (values.Any())
        {
            string[] arr = values.ToArray();

            for (int i = 0; i < arr.Length; i++)
            {
                switch (arr[i])
                {
                    case "new-line":
                        {
                            Logger.WriteWarning($"Value '{arr[i]}' is has been deprecated "
                                + "and will be removed in future version. "
                                + $"Use value '{OptionValues.HighlightOptions_Newline.HelpValue}' instead.");

                            arr[i] = OptionValues.HighlightOptions_Newline.Value;
                            break;
                        }
                    case "nl":
                        {
                            Logger.WriteWarning($"Value '{arr[i]}' is has been deprecated "
                                + "and will be removed in future version. "
                                + $"Use value '{OptionValues.HighlightOptions_Newline.HelpValue}' instead.");

                            arr[i] = OptionValues.HighlightOptions_Newline.Value;
                            break;
                        }
                }
            }

            values = arr;
        }

        return TryParseAsEnumFlags(
            values,
            OptionNames.Highlight,
            out highlightOptions,
            defaultValue: defaultValue,
            provider: provider ?? OptionValueProviders.HighlightOptionsProvider);
    }

    public bool TryParseFileProperties(
        IEnumerable<string> values,
        string optionName,
        out bool creationTime,
        out bool modifiedTime,
        out bool size,
        out FilterPredicate<DateTime>? creationTimePredicate,
        out FilterPredicate<DateTime>? modifiedTimePredicate,
        out FilterPredicate<long>? sizePredicate)
    {
        creationTime = false;
        modifiedTime = false;
        size = false;
        creationTimePredicate = null;
        modifiedTimePredicate = null;
        sizePredicate = null;

        foreach (string value in values)
        {
            Expression? expression = null;
            OptionValue? optionValue = null;

            try
            {
                if (OptionValues.FileProperty_CreationTime.IsKeyOrShortKey(value))
                {
                    creationTime = true;
                    continue;
                }

                if (OptionValues.FileProperty_ModifiedTime.IsKeyOrShortKey(value))
                {
                    modifiedTime = true;
                    continue;
                }

                if (OptionValues.FileProperty_Size.IsKeyOrShortKey(value))
                {
                    size = true;
                    continue;
                }

                int index = Expression.GetOperatorIndex(value);

                if (index == -1)
                {
                    WriteOptionError(
                        value,
                        optionName,
                        OptionValueProviders.FilePropertiesProvider);

                    return false;
                }

                expression = Expression.Parse(value);

                if (OptionValues.FileProperty_CreationTime.IsKeyOrShortKey(expression.Identifier))
                {
                    optionValue = OptionValues.FileProperty_CreationTime;

                    creationTimePredicate = new FilterPredicate<DateTime>(
                        expression,
                        PredicateHelpers.GetDateTimePredicate(expression));
                }
                else if (OptionValues.FileProperty_ModifiedTime.IsKeyOrShortKey(expression.Identifier))
                {
                    optionValue = OptionValues.FileProperty_ModifiedTime;

                    modifiedTimePredicate = new FilterPredicate<DateTime>(
                        expression,
                        PredicateHelpers.GetDateTimePredicate(expression));
                }
                else if (OptionValues.FileProperty_Size.IsKeyOrShortKey(expression.Identifier))
                {
                    optionValue = OptionValues.FileProperty_Size;

                    if (expression.Kind == ExpressionKind.DecrementExpression)
                    {
                        WriteOptionError(value, optionName, HelpProvider.GetExpressionsText("  ", includeDate: false));
                        return false;
                    }

                    sizePredicate = new FilterPredicate<long>(expression, PredicateHelpers.GetLongPredicate(expression));
                }
                else
                {
                    WriteOptionError(value, optionName, OptionValueProviders.FilePropertiesProvider);
                    return false;
                }
            }
            catch (ArgumentException)
            {
                if (expression is not null
                    && optionValue is not null)
                {
                    WriteOptionValueError(
                        expression.Value,
                        optionValue,
                        HelpProvider.GetExpressionsText(
                            "  ",
                            includeDate: optionValue != OptionValues.FileProperty_Size));
                }
                else
                {
                    WriteOptionError(value, optionName, HelpProvider.GetExpressionsText("  "));
                }

                return false;
            }
        }

        return true;
    }

    public bool TryParseSortOptions(
        IEnumerable<string> values,
        string optionName,
        out SortOptions? sortOptions)
    {
        sortOptions = null;
        int maxCount = 0;

        List<string>? options = null;

        if (!values.Any())
            return true;

        foreach (string value in values)
        {
            int index = value.IndexOf('=');

            if (index >= 0)
            {
                string key = value.Substring(0, index);
                string value2 = value.Substring(index + 1);

                if (OptionValues.MaxCount.IsKeyOrShortKey(key))
                {
                    if (!TryParseCount(value2, OptionNames.Sort, out maxCount, value))
                        return false;
                }
                else
                {
                    WriteOptionError(value, optionName, OptionValueProviders.SortFlagsProvider);
                    return false;
                }
            }
            else
            {
                (options ??= new List<string>()).Add(value);
            }
        }

        if (!TryParseAsEnumValues(
            options,
            optionName,
            out ImmutableArray<SortFlags> flags,
            provider: OptionValueProviders.SortFlagsProvider))
        {
            return false;
        }

        if (flags.Contains(SortFlags.Ascending)
            && flags.Contains(SortFlags.Descending))
        {
            Logger.WriteError($"Option '{optionName}' cannot use both '{OptionValues.SortFlags_Ascending.HelpValue}' "
                + $"and '{OptionValues.SortFlags_Descending.HelpValue}' values.");

            return false;
        }

        SortDirection direction = (flags.Contains(SortFlags.Descending))
            ? SortDirection.Descending
            : SortDirection.Ascending;

        List<SortDescriptor>? descriptors = null;

        foreach (SortFlags flag in flags)
        {
            switch (flag)
            {
                case SortFlags.Name:
                    {
                        AddDescriptor(SortProperty.Name, direction);
                        break;
                    }
                case SortFlags.CreationTime:
                    {
                        AddDescriptor(SortProperty.CreationTime, direction);
                        break;
                    }
                case SortFlags.ModifiedTime:
                    {
                        AddDescriptor(SortProperty.ModifiedTime, direction);
                        break;
                    }
                case SortFlags.Size:
                    {
                        AddDescriptor(SortProperty.Size, direction);
                        break;
                    }
                case SortFlags.None:
                case SortFlags.Ascending:
                case SortFlags.Descending:
                case SortFlags.CultureInvariant:
                    {
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException($"Unknown enum value '{flag}'.");
                    }
            }
        }

        bool cultureInvariant = flags.Contains(SortFlags.CultureInvariant);

        if (descriptors is not null)
        {
            sortOptions = new SortOptions(descriptors.ToImmutableArray(), cultureInvariant: cultureInvariant, maxCount: maxCount);
        }
        else
        {
            sortOptions = new SortOptions(
                ImmutableArray.Create(new SortDescriptor(SortProperty.Name, direction)),
                cultureInvariant: cultureInvariant,
                maxCount: maxCount);
        }

        return true;

        void AddDescriptor(SortProperty p, SortDirection d)
        {
            (descriptors ??= new List<SortDescriptor>()).Add(new SortDescriptor(p, d));
        }
    }

    public bool TryParseModifyOptions(
        IEnumerable<string> values,
        string optionName,
        EnumerableModifier<string>? modifier,
        [NotNullWhen(true)] out ModifyOptions? modifyOptions,
        out bool aggregateOnly,
        out bool saveAggregatedValues)
    {
        modifyOptions = null;
        aggregateOnly = false;
        saveAggregatedValues = false;

        var sortProperty = ValueSortProperty.None;
        List<string>? options = null;

        foreach (string value in values)
        {
            int index = value.IndexOf('=');

            if (index >= 0)
            {
                string key = value.Substring(0, index);
                string value2 = value.Substring(index + 1);

                if (OptionValues.SortBy.IsKeyOrShortKey(key))
                {
                    if (!TryParseAsEnum(
                        value2,
                        optionName,
                        out sortProperty,
                        provider: OptionValueProviders.ValueSortPropertyProvider))
                    {
                        return false;
                    }
                }
                else
                {
                    WriteOptionError(value, optionName, OptionValueProviders.ModifyFlagsProvider);
                    return false;
                }
            }
            else
            {
                string value2 = value;

                if (value2 == "ao")
                {
                    Logger.WriteWarning($"Value '{value2}' is has been deprecated "
                        + "and will be removed in future version. "
                        + $"Use value '{OptionValues.ModifyFlags_AggregateOnly.HelpValue}' instead.");

                    value2 = OptionValues.ModifyFlags_AggregateOnly.ShortValue;
                }

                (options ??= new List<string>()).Add(value2);
            }
        }

        var modifyFlags = ModifyFlags.None;

        if (options is not null
            && !TryParseAsEnumFlags(
                options,
                optionName,
                out modifyFlags,
                provider: OptionValueProviders.ModifyFlagsProvider))
        {
            return false;
        }

        ModifyFlags except_Intersect_Group = modifyFlags & ModifyFlags.Except_Intersect_Group;

        if (except_Intersect_Group != ModifyFlags.None
            && except_Intersect_Group != ModifyFlags.Except
            && except_Intersect_Group != ModifyFlags.Intersect
            && except_Intersect_Group != ModifyFlags.Group)
        {
            Logger.WriteError($"Values '{OptionValues.ModifyFlags_Except.HelpValue}', "
                + $"'{OptionValues.ModifyFlags_Intersect.HelpValue}' and "
                + $"'{OptionValues.ModifyFlags_Group.HelpValue}' cannot be used at the same time.");

            return false;
        }

        var functions = ModifyFunctions.None;

        if ((modifyFlags & ModifyFlags.Ascending) != 0)
            functions |= ModifyFunctions.SortAscending;

        if ((modifyFlags & ModifyFlags.Descending) != 0)
            functions |= ModifyFunctions.SortDescending;

        if ((functions & ModifyFunctions.Sort) == ModifyFunctions.Sort)
        {
            Logger.WriteError($"Option '{optionName}' cannot use both '{OptionValues.ModifyOptions_Ascending.HelpValue}' "
                + $"and '{OptionValues.ModifyOptions_Descending.HelpValue}' values.");

            return false;
        }

        if ((modifyFlags & ModifyFlags.Distinct) != 0)
            functions |= ModifyFunctions.Distinct;

        if ((modifyFlags & ModifyFlags.Except) != 0)
            functions |= ModifyFunctions.Except;

        if ((modifyFlags & ModifyFlags.Intersect) != 0)
            functions |= ModifyFunctions.Intersect;

        if ((modifyFlags & ModifyFlags.Group) != 0)
            functions |= ModifyFunctions.Group;

        if ((modifyFlags & ModifyFlags.RemoveEmpty) != 0)
            functions |= ModifyFunctions.RemoveEmpty;

        if ((modifyFlags & ModifyFlags.RemoveWhiteSpace) != 0)
            functions |= ModifyFunctions.RemoveWhiteSpace;

        if ((modifyFlags & ModifyFlags.TrimStart) != 0)
            functions |= ModifyFunctions.TrimStart;

        if ((modifyFlags & ModifyFlags.TrimEnd) != 0)
            functions |= ModifyFunctions.TrimEnd;

        if ((modifyFlags & ModifyFlags.ToLower) != 0)
            functions |= ModifyFunctions.ToLower;

        if ((modifyFlags & ModifyFlags.ToUpper) != 0)
            functions |= ModifyFunctions.ToUpper;

        if ((modifyFlags & ModifyFlags.Count) != 0)
            functions |= ModifyFunctions.Count;

        if (sortProperty != ValueSortProperty.None
            && (functions & ModifyFunctions.Sort) == 0)
        {
            functions |= ModifyFunctions.SortAscending;
        }

        aggregateOnly = (modifyFlags & ModifyFlags.AggregateOnly) != 0;
        saveAggregatedValues = (modifyFlags & ModifyFlags.Save) != 0;

        if (modifyFlags != ModifyFlags.None
            || functions != ModifyFunctions.None
            || modifier is not null)
        {
            modifyOptions = new ModifyOptions(
                functions: functions,
                aggregate: (modifyFlags & ModifyFlags.Aggregate) != 0 || aggregateOnly,
                ignoreCase: (modifyFlags & ModifyFlags.IgnoreCase) != 0,
                cultureInvariant: (modifyFlags & ModifyFlags.CultureInvariant) != 0,
                sortProperty: sortProperty,
                modifier: modifier);
        }
        else
        {
            modifyOptions = ModifyOptions.Default;
        }

        return true;
    }

    public bool TryParseModifier(
        IEnumerable<string> values,
        string optionName,
        [NotNullWhen(true)] out EnumerableModifier<string>? modifier)
    {
        modifier = null;

        if (!values.Any())
            return false;

        string value = values.First();

        if (!TryParseAsEnumFlags(
            values.Skip(1),
            optionName,
            out ModifierOptions options,
            ModifierOptions.None,
            OptionValueProviders.ModifierOptionsProvider))
        {
            return false;
        }

        if ((options & ModifierOptions.FromFile) != 0
            && !FileSystemUtilities.TryReadAllText(value, out value!, ex => Logger.WriteError(ex)))
        {
            return false;
        }

        var replacementOptions = ReplacementOptions.None;

        if ((options & ModifierOptions.FromDll) != 0)
        {
            replacementOptions = ReplacementOptions.FromDll;
        }
        else if ((options & ModifierOptions.FromFile) != 0)
        {
            replacementOptions = ReplacementOptions.CSharp | ReplacementOptions.FromFile;
        }

        return TryParseDelegate(
            optionName: optionName,
            value: value,
            returnTypeName: "IEnumerable<string>",
            parameterTypeName: "IEnumerable<string>",
            parameterName: "items",
            returnType: typeof(IEnumerable<string>),
            parameterType: typeof(IEnumerable<string>),
            replacementOptions: replacementOptions,
            out modifier);
    }

    public bool TryParseReplaceOptions(
        IEnumerable<string> values,
        string optionName,
        string? replacement,
        MatchEvaluator? matchEvaluator,
        [NotNullWhen(true)] out Replacer? replacer)
    {
        replacer = null;
        var replaceFlags = ReplaceFlags.None;

        if (values is not null
            && !TryParseAsEnumFlags(
                values,
                optionName,
                out replaceFlags,
                provider: OptionValueProviders.ReplaceFlagsProvider))
        {
            return false;
        }

        var functions = ReplaceFunctions.None;

        if ((replaceFlags & ReplaceFlags.TrimStart) != 0)
            functions |= ReplaceFunctions.TrimStart;

        if ((replaceFlags & ReplaceFlags.TrimEnd) != 0)
            functions |= ReplaceFunctions.TrimEnd;

        if ((replaceFlags & ReplaceFlags.ToLower) != 0)
            functions |= ReplaceFunctions.ToLower;

        if ((replaceFlags & ReplaceFlags.ToUpper) != 0)
            functions |= ReplaceFunctions.ToUpper;

        if (matchEvaluator is not null)
        {
            replacer = new Replacer(
                matchEvaluator: matchEvaluator,
                functions: functions,
                cultureInvariant: (replaceFlags & ReplaceFlags.CultureInvariant) != 0);
        }
        else
        {
            replacer = new Replacer(
                replacement: replacement,
                functions: functions,
                cultureInvariant: (replaceFlags & ReplaceFlags.CultureInvariant) != 0);
        }

        return true;
    }

    public bool TryParseDisplay(
        IEnumerable<string> values,
        string optionName,
        out ContentDisplayStyle? contentDisplayStyle,
        out PathDisplayStyle? pathDisplayStyle,
        out LineDisplayOptions lineDisplayOptions,
        out LineContext lineContext,
        out DisplayParts displayParts,
        out bool includeCreationTime,
        out bool includeModifiedTime,
        out bool includeSize,
        out string? indent,
        out string? separator,
        out bool noAlign,
        OptionValueProvider? contentDisplayStyleProvider = null,
        OptionValueProvider? pathDisplayStyleProvider = null)
    {
        contentDisplayStyle = null;
        pathDisplayStyle = null;
        lineDisplayOptions = LineDisplayOptions.None;
        lineContext = default;
        displayParts = DisplayParts.None;
        includeCreationTime = false;
        includeModifiedTime = false;
        includeSize = false;
        indent = null;
        separator = null;
        noAlign = false;

        foreach (string value in values)
        {
            int index = value.IndexOf('=');

            if (index >= 0)
            {
                string key = value.Substring(0, index);
                string value2 = value.Substring(index + 1);

                if (key == "t")
                {
                    Logger.WriteWarning($"Value '{key}' is has been deprecated "
                        + "and will be removed in future version. "
                        + $"Use value '{OptionValues.Display_Context.HelpValue}' instead.");

                    key = OptionValues.Display_Context.ShortKey;
                }
                else if (key == "ta")
                {
                    Logger.WriteWarning($"Value '{key}' is has been deprecated "
                        + "and will be removed in future version. "
                        + $"Use value '{OptionValues.Display_ContextAfter.HelpValue}' instead.");

                    key = OptionValues.Display_ContextAfter.ShortKey;
                }
                else if (key == "tb")
                {
                    Logger.WriteWarning($"Value '{key}' is has been deprecated "
                        + "and will be removed in future version. "
                        + $"Use value '{OptionValues.Display_ContextBefore.HelpValue}' instead.");

                    key = OptionValues.Display_ContextBefore.ShortKey;
                }

                if (OptionValues.Display_Content.IsKeyOrShortKey(key))
                {
                    if (!TryParseAsEnum(
                        value2,
                        optionName,
                        out ContentDisplayStyle contentDisplayStyle2,
                        provider: contentDisplayStyleProvider))
                    {
                        return false;
                    }

                    contentDisplayStyle = contentDisplayStyle2;
                }
                else if (OptionValues.Display_Path.IsKeyOrShortKey(key))
                {
                    if (!TryParseAsEnum(
                        value2,
                        optionName,
                        out PathDisplayStyle pathDisplayStyle2,
                        provider: pathDisplayStyleProvider))
                    {
                        return false;
                    }

                    pathDisplayStyle = pathDisplayStyle2;
                }
                else if (OptionValues.Display_Indent.IsKeyOrShortKey(key))
                {
                    indent = value2;
                }
                else if (OptionValues.Display_Separator.IsKeyOrShortKey(key))
                {
                    separator = RegexEscape.ConvertCharacterEscapes(value2);
                }
                else if (OptionValues.Display_Context.IsKeyOrShortKey(key))
                {
                    if (!TryParseCount(value2, OptionNames.Display, out int count, value))
                        return false;

                    lineContext = new LineContext(count);
                }
                else if (OptionValues.Display_ContextBefore.IsKeyOrShortKey(key))
                {
                    if (!TryParseCount(value2, OptionNames.Display, out int before, value))
                        return false;

                    lineContext = lineContext.WithBefore(before);
                }
                else if (OptionValues.Display_ContextAfter.IsKeyOrShortKey(key))
                {
                    if (!TryParseCount(value2, OptionNames.Display, out int after, value))
                        return false;

                    lineContext = lineContext.WithAfter(after);
                }
                else
                {
                    WriteOptionError(value, optionName, OptionValueProviders.DisplayProvider);
                    return false;
                }
            }
            else if (OptionValues.Display_Summary.IsValueOrShortValue(value))
            {
                displayParts |= DisplayParts.Summary;
            }
            else if (OptionValues.Display_Count.IsValueOrShortValue(value))
            {
                displayParts |= DisplayParts.Count;
            }
            else if (OptionValues.Display_CreationTime.IsValueOrShortValue(value))
            {
                includeCreationTime = true;
            }
            else if (OptionValues.Display_ModifiedTime.IsValueOrShortValue(value))
            {
                includeModifiedTime = false;
            }
            else if (OptionValues.Display_Size.IsValueOrShortValue(value))
            {
                includeSize = false;
            }
            else if (OptionValues.Display_LineNumber.IsValueOrShortValue(value))
            {
                lineDisplayOptions |= LineDisplayOptions.IncludeLineNumber;
            }
            else if (OptionValues.Display_TrimLine.IsValueOrShortValue(value))
            {
                lineDisplayOptions |= LineDisplayOptions.TrimLine;
            }
            else if (OptionValues.Display_TrimLine.IsValueOrShortValue(value))
            {
                lineDisplayOptions |= LineDisplayOptions.TrimLine;
            }
            else if (OptionValues.Display_TrimLine.IsValueOrShortValue(value))
            {
                lineDisplayOptions |= LineDisplayOptions.TrimLine;
            }
            else if (OptionValues.Display_NoAlign.IsValueOrShortValue(value))
            {
                noAlign = true;
            }
            else
            {
                WriteOptionError(value, optionName, OptionValueProviders.DisplayProvider);
                return false;
            }
        }

        return true;
    }

    public bool TryParseOutputOptions(
        IEnumerable<string> values,
        string optionName,
        out string? path,
        out Verbosity verbosity,
        [NotNullWhen(true)] out Encoding? encoding,
        out bool append)
    {
        path = null;
        verbosity = Verbosity.Normal;
        encoding = Encoding.UTF8;
        append = false;

        if (!values.Any())
            return true;

        if (!TryEnsureFullPath(values.First(), out path))
            return false;

        foreach (string value in values.Skip(1))
        {
            string option = value;

            int index = option.IndexOf('=');

            if (index >= 0)
            {
                string key = option.Substring(0, index);
                string value2 = option.Substring(index + 1);

                if (OptionValues.Verbosity.IsKeyOrShortKey(key))
                {
                    if (!TryParseVerbosity(value2, out verbosity))
                        return false;
                }
                else if (OptionValues.Encoding.IsKeyOrShortKey(key))
                {
                    if (!TryParseEncoding(value2, out encoding))
                        return false;
                }
                else
                {
                    WriteOptionError(value, optionName, OptionValueProviders.OutputFlagsProvider);
                    return false;
                }
            }
            else if (OptionValues.Output_Append.IsValueOrShortValue(value))
            {
                append = true;
            }
            else
            {
                WriteOptionError(value, optionName, OptionValueProviders.OutputFlagsProvider);
                return false;
            }
        }

        return true;
    }

    public bool TryParseReplacement(
        IEnumerable<string> values,
        out string? replacement,
        out MatchEvaluator? matchEvaluator)
    {
        replacement = null;
        matchEvaluator = null;

        if (!values.Any())
            return true;

        string value = values.First();

        if (!TryParseAsEnumFlags(
            values.Skip(1),
            OptionNames.Replacement,
            out ReplacementOptions options,
            ReplacementOptions.None,
            OptionValueProviders.ReplacementOptionsProvider))
        {
            return false;
        }

        if ((options & ReplacementOptions.FromFile) != 0
            && !FileSystemUtilities.TryReadAllText(value, out value!, ex => Logger.WriteError(ex)))
        {
            return false;
        }

        if ((options & (ReplacementOptions.CSharp)) != 0
            || ((options & (ReplacementOptions.FromCSharpFile)) == (ReplacementOptions.FromCSharpFile))
            || (options & (ReplacementOptions.FromDll)) != 0)
        {
            return TryParseDelegate(
                optionName: OptionNames.Replacement,
                value: value,
                returnTypeName: "string",
                parameterTypeName: "Match",
                parameterName: "match",
                returnType: typeof(string),
                parameterType: typeof(Match),
                replacementOptions: options,
                out matchEvaluator);
        }
        else
        {
            replacement = value;

            if ((options & ReplacementOptions.Literal) != 0)
                replacement = RegexEscape.EscapeSubstitution(replacement);

            if ((options & ReplacementOptions.Escape) != 0)
                replacement = RegexEscape.ConvertCharacterEscapes(replacement);
        }

        return true;
    }

    private bool TryParseDelegate<TDelegate>(
        string optionName,
        string value,
        string returnTypeName,
        string parameterTypeName,
        string parameterName,
        Type returnType,
        Type parameterType,
        ReplacementOptions replacementOptions,
        out TDelegate? matchEvaluator,
        string? namespaceName = null,
        string? className = null,
        string? methodName = null) where TDelegate : Delegate
    {
        matchEvaluator = null;
        Assembly? assembly = null;
        string? delegateTypeName = null;
        string? delegateMethodName = null;

        if ((replacementOptions & ReplacementOptions.CSharp) != 0)
        {
            SyntaxTree syntaxTree;

            if ((replacementOptions & ReplacementOptions.FromFile) != 0)
            {
                syntaxTree = CSharpSyntaxTree.ParseText(value);
            }
            else
            {
                syntaxTree = RoslynUtilities.CreateSyntaxTree(
                    expressionText: value,
                    returnTypeName: returnTypeName,
                    parameterType: parameterTypeName,
                    parameterName: parameterName,
                    namespaceName: namespaceName,
                    className: className,
                    methodName: methodName);
            }

            Compilation compilation = RoslynUtilities.CreateCompilation(syntaxTree);
            SourceText? sourceText = null;
            var hasDiagnostic = false;

            foreach (Diagnostic diagnostic in compilation.GetDiagnostics()
                .Where(f => f.Severity == DiagnosticSeverity.Error)
                .OrderBy(f => f.Location.SourceSpan))
            {
                if (sourceText is null)
                    sourceText = syntaxTree.GetText();

                if (!hasDiagnostic)
                    WriteErrorMessage();

                Logger.WriteDiagnostic(diagnostic, sourceText, "  ", Verbosity.Quiet);
                hasDiagnostic = true;
            }

            if (hasDiagnostic)
                return false;

            hasDiagnostic = false;

            using (var memoryStream = new MemoryStream())
            {
                EmitResult emitResult = compilation.Emit(memoryStream);

                if (!emitResult.Success)
                {
                    foreach (Diagnostic diagnostic in emitResult.Diagnostics.OrderBy(f => f.Location.SourceSpan))
                    {
                        if (sourceText is null)
                            sourceText = syntaxTree.GetText();

                        if (!hasDiagnostic)
                            WriteErrorMessage();

                        Logger.WriteDiagnostic(diagnostic, sourceText, "  ", Verbosity.Quiet);
                        hasDiagnostic = true;
                    }

                    if (hasDiagnostic)
                        return false;
                }

                memoryStream.Seek(0, SeekOrigin.Begin);

                try
                {
                    assembly = Assembly.Load(memoryStream.ToArray());
                }
                catch (Exception ex) when (ex is ArgumentException || ex is BadImageFormatException)
                {
                    Logger.WriteError(ex, $"{GetErrorMessage()} {ex.Message}");
                    return false;
                }
            }
        }
        else if ((replacementOptions & ReplacementOptions.FromDll) != 0)
        {
            int index = value.LastIndexOf(',');

            if (index <= 0
                || index >= value.Length - 1)
            {
                Logger.WriteError($"{GetErrorMessage()} The expected format is \"MyLib.dll,MyNamespace.MyClass.MyMethod\".");
                return false;
            }

            string assemblyName = value.Substring(0, index);
            string methodFullName = value.Substring(index + 1);
            index = methodFullName.LastIndexOf('.');

            if (index < 0
                || index >= value.Length - 1)
            {
                Logger.WriteError($"{GetErrorMessage()} The expected format is \"MyLib.dll,MyNamespace.MyClass.MyMethod\".");
                return false;
            }

            try
            {
                assembly = Assembly.LoadFrom(assemblyName);
            }
            catch (Exception ex) when (ex is ArgumentException
                || ex is IOException
                || ex is BadImageFormatException)
            {
                Logger.WriteError(ex, $"{GetErrorMessage()} {ex.Message}");
                return false;
            }

            delegateTypeName = methodFullName.Substring(0, index);
            delegateMethodName = methodFullName.Substring(index + 1);
        }

        try
        {
            matchEvaluator = DelegateFactory.CreateDelegate<TDelegate>(
                assembly!,
                returnType,
                new Type[] { parameterType },
                delegateTypeName,
                delegateMethodName);

            return true;
        }
        catch (Exception ex) when (ex is ArgumentException
            || ex is AmbiguousMatchException
            || ex is TargetInvocationException
            || ex is MemberAccessException
            || ex is MissingMemberException
            || ex is TypeLoadException
            || ex is InvalidOperationException)
        {
            Logger.WriteError(ex, $"{GetErrorMessage()} {ex.Message}");
            return false;
        }

        void WriteErrorMessage()
        {
            Logger.WriteError(GetErrorMessage());
        }

        string GetErrorMessage()
        {
            return $"Option '{OptionNames.GetHelpText(optionName)}' has invalid value:";
        }
    }

    public bool TryParseInput(
        IEnumerable<string> values,
        out string input)
    {
        if (!values.Any())
            throw new InvalidOperationException("Input is missing.");

        input = values.First();

        if (!TryParseAsEnumFlags(
            values.Skip(1),
            OptionNames.Input,
            out InputOptions options,
            InputOptions.None,
            OptionValueProviders.InputOptionsProvider))
        {
            return false;
        }

        if ((options & InputOptions.Escape) != 0)
            input = RegexEscape.ConvertCharacterEscapes(input);

        return true;
    }

    public bool TryParseAsEnumFlags<TEnum>(
        IEnumerable<string>? values,
        string optionName,
        out TEnum result,
        TEnum? defaultValue = null,
        OptionValueProvider? provider = null) where TEnum : struct
    {
        result = (TEnum)(object)0;

        if (values?.Any() != true)
        {
            if (defaultValue is not null)
            {
                result = (TEnum)(object)defaultValue;
            }

            return true;
        }

        int flags = 0;

        foreach (string value in values)
        {
            if (!TryParseAsEnum(value, optionName, out TEnum result2, provider: provider))
                return false;

            flags |= (int)(object)result2;
        }

        result = (TEnum)(object)flags;

        return true;
    }

    public bool TryParseAsEnumValues<TEnum>(
        IEnumerable<string>? values,
        string optionName,
        out ImmutableArray<TEnum> result,
        ImmutableArray<TEnum> defaultValue = default,
        OptionValueProvider? provider = null) where TEnum : struct
    {
        if (values?.Any() != true)
        {
            result = (defaultValue.IsDefault) ? ImmutableArray<TEnum>.Empty : defaultValue;

            return true;
        }

        ImmutableArray<TEnum>.Builder builder = ImmutableArray.CreateBuilder<TEnum>();

        foreach (string value in values)
        {
            if (!TryParseAsEnum(value, optionName, out TEnum result2, provider: provider))
            {
                result = default;
                return false;
            }

            builder.Add(result2);
        }

        result = builder.ToImmutableArray();

        return true;
    }

    public bool TryParseAsEnum<TEnum>(
        string value,
        string optionName,
        out TEnum result,
        TEnum? defaultValue = null,
        OptionValueProvider? provider = null) where TEnum : struct
    {
        if (!TryParseAsEnum(value, out result, defaultValue, provider))
        {
            string optionValues = OptionValueProviders.GetHelpText(provider, multiline: true)
                ?? OptionValue.GetDefaultHelpText<TEnum>(multiline: true);

            WriteOptionError(value, optionName, optionValues);
            return false;
        }

        return true;
    }

    public bool TryParseAsEnum<TEnum>(
        string value,
        out TEnum result,
        TEnum? defaultValue = null,
        OptionValueProvider? provider = default) where TEnum : struct
    {
        if (value is null
            && defaultValue is not null)
        {
            result = defaultValue.Value;
            return true;
        }

        if (provider is not null)
        {
            return provider.TryParseEnum(value, out result);
        }
        else
        {
            return Enum.TryParse(value?.Replace("-", ""), ignoreCase: true, out result);
        }
    }

    public bool TryParseVerbosity(string value, out Verbosity verbosity)
    {
        return TryParseAsEnum(
            value,
            OptionNames.Verbosity,
            out verbosity,
            provider: OptionValueProviders.VerbosityProvider);
    }

    // https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding#remarks
    public bool TryParseEncoding(string name, [NotNullWhen(true)] out Encoding? encoding)
    {
        if (name == "utf-8-no-bom")
        {
            encoding = Text.EncodingHelpers.UTF8NoBom;
            return true;
        }

        try
        {
            encoding = Encoding.GetEncoding(name);
            return true;
        }
        catch (ArgumentException ex)
        {
            Logger.WriteError(ex);

            encoding = null;
            return false;
        }
    }

    public bool TryParseEncoding(string name, out Encoding encoding, Encoding defaultEncoding)
    {
        if (name is null)
        {
            encoding = defaultEncoding;
            return true;
        }

        return TryParseEncoding(name, out encoding, defaultEncoding);
    }

    private bool TryParseCount(string value, string optionName, out int count, string? value2 = null)
    {
        if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out count))
            return true;

        Logger.WriteError($"Option '{OptionNames.GetHelpText(optionName)}' has invalid value '{value2 ?? value}'.");
        return false;
    }

    public bool TryParseChar(string value, out char result)
    {
        if (value.Length == 2
            && value[0] == '\\'
            && value[1] >= 48
            && value[1] <= 57)
        {
            result = value[1];
            return true;
        }

        if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int charCode))
        {
            if (charCode < 0 || charCode > 0xFFFF)
            {
                Logger.WriteError("Value must be in range from 0 to 65535.");
                result = default;
                return false;
            }

            result = (char)charCode;
        }
        else if (!char.TryParse(value, out result))
        {
            Logger.WriteError($"Could not parse '{value}' as character value.");
            return false;
        }

        return true;
    }

    public bool TryEnsureFullPath(IEnumerable<string> paths, out ImmutableArray<string> fullPaths)
    {
        ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>();

        foreach (string path in paths)
        {
            if (!TryEnsureFullPath(path, out string? fullPath))
            {
                fullPaths = default;
                return false;
            }

            builder.Add(fullPath);
        }

        fullPaths = builder.ToImmutableArray();
        return true;
    }

    public bool TryEnsureFullPath(string path, [NotNullWhen(true)] out string? fullPath)
    {
        try
        {
            fullPath = FileSystemUtilities.EnsureFullPath(path);
            return true;
        }
        catch (ArgumentException ex)
        {
            Logger.WriteError($"Path '{path}' is invalid: {ex.Message}.");
            fullPath = null;
            return false;
        }
    }

    internal void WriteOptionError(string value, string optionName, OptionValueProvider? provider = null)
    {
        WriteOptionError(value, optionName, OptionValueProviders.GetHelpText(provider, multiline: true));
    }

    private void WriteOptionError(string value, string optionName, string? allowedValues)
    {
        WriteParseError(value, OptionNames.GetHelpText(optionName), allowedValues);
    }

    internal void WriteOptionValueError(
        string value,
        OptionValue optionValue,
        OptionValueProvider? provider = null)
    {
        WriteOptionValueError(value, optionValue, OptionValueProviders.GetHelpText(provider, multiline: true));
    }

    internal void WriteOptionValueError(string value, OptionValue optionValue, string? allowedValues)
    {
        WriteParseError(value, optionValue.HelpValue, allowedValues);
    }

    private void WriteParseError(string value, string optionText, string? allowedValues)
    {
        string message = $"Option '{optionText}' has invalid value '{value}'.";

        if (!string.IsNullOrEmpty(allowedValues))
            message += $"{Environment.NewLine}{Environment.NewLine}Allowed values:{Environment.NewLine}{allowedValues}";

        Logger.WriteError(message);
    }

    public bool TryParseFilter(
        IEnumerable<string> values,
        string optionName,
        OptionValueProvider provider,
        out Matcher? matcher,
        bool allowNull = false,
        bool allowEmptyPattern = false,
        OptionValueProvider? namePartProvider = null,
        FileNamePart defaultNamePart = FileNamePart.Name,
        PatternOptions includedPatternOptions = PatternOptions.None)
    {
        return TryParseFilter(
            values: values,
            optionName: optionName,
            provider: provider,
            matcher: out matcher,
            namePart: out _,
            allowNull: allowNull,
            allowEmptyPattern: allowEmptyPattern,
            namePartProvider: namePartProvider,
            defaultNamePart: defaultNamePart,
            includedPatternOptions: includedPatternOptions);
    }

    public bool TryParseFilter(
        IEnumerable<string> values,
        string optionName,
        OptionValueProvider provider,
        out Matcher? matcher,
        out FileNamePart namePart,
        bool allowNull = false,
        bool allowEmptyPattern = false,
        OptionValueProvider? namePartProvider = null,
        FileNamePart defaultNamePart = FileNamePart.Name,
        PatternOptions includedPatternOptions = PatternOptions.None)
    {
        matcher = null;
        namePart = defaultNamePart;

        string? pattern = values.FirstOrDefault();

        if (pattern is null)
        {
            if (allowNull)
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException($"Option '{OptionNames.GetHelpText(optionName)}' is required.");
            }
        }

        TimeSpan matchTimeout = Regex.InfiniteMatchTimeout;
        string? groupName = null;
        string? separator = null;
        Func<string, bool>? predicate = null;

        List<string>? options = null;

        foreach (string option in values.Skip(1))
        {
            int index = option.IndexOfAny(_equalsOrLessThanOrGreaterThanChars);

            if (index != -1)
            {
                string key = option.Substring(0, index);

                if (!provider.ContainsKeyOrShortKey(key))
                {
                    WriteOptionError(option, optionName, provider);
                    return false;
                }

                string value = option.Substring(index + 1);

                if (OptionValues.Group.IsKeyOrShortKey(key))
                {
                    groupName = value;
                    continue;
                }
                else if (OptionValues.ListSeparator.IsKeyOrShortKey(key))
                {
                    separator = value;
                    continue;
                }
                else if (OptionValues.Part.IsKeyOrShortKey(key))
                {
                    if (!TryParseAsEnum(
                        value,
                        out namePart,
                        provider: namePartProvider ?? OptionValueProviders.NamePartKindProvider))
                    {
                        WriteOptionValueError(
                            value,
                            OptionValues.Part,
                            namePartProvider ?? OptionValueProviders.NamePartKindProvider);

                        return false;
                    }

                    continue;
                }
                else if (OptionValues.Timeout.IsKeyOrShortKey(key))
                {
                    if (!TryParseMatchTimeout(value, out matchTimeout))
                    {
                        WriteOptionValueError(value, OptionValues.Timeout);
                        return false;
                    }

                    continue;
                }
            }

            if (Expression.TryParse(option, out Expression? expression))
            {
                if (OptionValues.Length.IsKeyOrShortKey(expression.Identifier)
                    && provider.ContainsKeyOrShortKey(expression.Identifier))
                {
                    try
                    {
                        predicate = PredicateHelpers.GetLengthPredicate(expression);
                        continue;
                    }
                    catch (ArgumentException)
                    {
                        WriteOptionValueError(
                            expression.Value,
                            OptionValues.Length,
                            HelpProvider.GetExpressionsText("  ", includeDate: false));
                        return false;
                    }
                }
                else
                {
                    WriteOptionError(option, optionName, provider);
                    return false;
                }
            }

            (options ??= new List<string>()).Add(option);
        }

        if (pattern.Length == 0
            && allowEmptyPattern)
        {
            matcher = new Matcher(new Regex(".+", RegexOptions.Singleline), predicate: predicate);
            return true;
        }

        if (!TryParseRegexOptions(
            options,
            optionName,
            out RegexOptions regexOptions,
            out PatternOptions patternOptions,
            includedPatternOptions,
            provider))
        {
            return false;
        }

        switch (patternOptions & (PatternOptions.WholeWord | PatternOptions.WholeLine))
        {
            case PatternOptions.None:
            case PatternOptions.WholeWord:
            case PatternOptions.WholeLine:
                {
                    break;
                }
            default:
                {
                    string helpValue = OptionValueProviders.PatternOptionsProvider
                        .GetValue(nameof(PatternOptions.WholeWord)).HelpValue;

                    string helpValue2 = OptionValueProviders.PatternOptionsProvider
                        .GetValue(nameof(PatternOptions.WholeLine)).HelpValue;

                    Logger.WriteError($"Values '{helpValue}' and '{helpValue2}' cannot be combined.");

                    return false;
                }
        }

        if ((patternOptions & PatternOptions.FromFile) != 0)
        {
            if (!FileSystemUtilities.TryReadAllText(pattern, out pattern, ex => Logger.WriteError(ex)))
                return false;

            if (pattern.Length == 0
                && (patternOptions & PatternOptions.List) != 0
                && allowNull)
            {
                pattern = null;
                return true;
            }
        }

        pattern = BuildPattern(pattern, patternOptions, separator);

        if (pattern.Length == 0)
        {
            throw new InvalidOperationException(
                $"Option '{OptionNames.GetHelpText(optionName)}' is invalid: pattern cannot be empty.");
        }

        Regex? regex = null;

        try
        {
            regex = new Regex(pattern, regexOptions, matchTimeout);
        }
        catch (ArgumentException ex)
        {
            Logger.WriteError(ex, $"Could not parse regular expression: {ex.Message}");
            return false;
        }

        if (groupName is not null)
        {
            int groupNumber = regex.GroupNumberFromName(groupName);
            if (groupNumber == -1)
            {
                string message = $"Group '{groupName}' does not exist.";

                string[] groupNames = regex.GetGroupNames();

                if (groupNames.Length > 1)
                {
                    message += " Existing group names: "
                        + $"{TextHelpers.Join(", ", " and ", groupNames.Where(f => f != "0"))}.";
                }

                Logger.WriteError(message);
                return false;
            }
        }

        matcher = new Matcher(
            regex,
            invert: (patternOptions & PatternOptions.Negative) != 0,
            group: groupName,
            predicate: predicate);

        return true;
    }

    private string BuildPattern(
        string pattern,
        PatternOptions patternOptions,
        string? separator)
    {
        bool literal = (patternOptions & PatternOptions.Literal) != 0;

        if ((patternOptions & PatternOptions.List) != 0)
        {
            IEnumerable<string> values;

            if ((patternOptions & PatternOptions.FromFile) != 0
                && separator is null)
            {
                values = TextHelpers.ReadLines(pattern).Where(f => f.Length > 0);
            }
            else
            {
                values = pattern.Split(separator ?? ",", StringSplitOptions.RemoveEmptyEntries);
            }

            pattern = JoinValues(values);
        }
        else if (literal)
        {
            pattern = RegexEscape.Escape(pattern);
        }

        if ((patternOptions & PatternOptions.WholeWord) != 0)
        {
            pattern = @"\b(?:" + pattern + @")\b";
        }
        else if ((patternOptions & PatternOptions.WholeLine) != 0)
        {
            pattern = @"(?:\A|(?<=\n))(?:" + pattern + @")(?:\z|(?=\r?\n))";
        }

        if ((patternOptions & PatternOptions.Equals) == PatternOptions.Equals)
        {
            return @"\A(?:" + pattern + @")\z";
        }
        else if ((patternOptions & PatternOptions.StartsWith) != 0)
        {
            return @"\A(?:" + pattern + ")";
        }
        else if ((patternOptions & PatternOptions.EndsWith) != 0)
        {
            return "(?:" + pattern + @")\z";
        }

        return pattern;

        string JoinValues(IEnumerable<string> values)
        {
            using (IEnumerator<string> en = values.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    string value = en.Current;

                    if (en.MoveNext())
                    {
                        StringBuilder sb = StringBuilderCache.GetInstance();

                        AppendValue(value, sb);

                        do
                        {
                            sb.Append("|");
                            AppendValue(en.Current, sb);
                        }
                        while (en.MoveNext());

                        return StringBuilderCache.GetStringAndFree(sb);
                    }

                    return (literal) ? RegexEscape.Escape(value) : value;
                }

                return "";
            }
        }

        void AppendValue(string value, StringBuilder sb)
        {
            if (literal)
            {
                sb.Append(RegexEscape.Escape(value));
            }
            else
            {
                sb.Append("(?:");
                sb.Append(value);
                sb.Append(")");
            }
        }
    }

    private bool TryParseRegexOptions(
        IEnumerable<string>? options,
        string optionsParameterName,
        out RegexOptions regexOptions,
        out PatternOptions patternOptions,
        PatternOptions includedPatternOptions = PatternOptions.None,
        OptionValueProvider? provider = null)
    {
        regexOptions = RegexOptions.None;

        if (!TryParseAsEnumFlags(
            options,
            optionsParameterName,
            out patternOptions,
            provider: provider ?? OptionValueProviders.PatternOptionsProvider))
        {
            return false;
        }

        Debug.Assert((patternOptions & (PatternOptions.CaseSensitive | PatternOptions.IgnoreCase))
            != (PatternOptions.CaseSensitive | PatternOptions.IgnoreCase));

        if ((patternOptions & PatternOptions.CaseSensitive) != 0)
        {
            includedPatternOptions &= ~PatternOptions.IgnoreCase;
        }
        else if ((patternOptions & PatternOptions.IgnoreCase) != 0)
        {
            includedPatternOptions &= ~PatternOptions.CaseSensitive;
        }

        patternOptions |= includedPatternOptions;

        if ((patternOptions & PatternOptions.Compiled) != 0)
            regexOptions |= RegexOptions.Compiled;

        if ((patternOptions & PatternOptions.CultureInvariant) != 0)
            regexOptions |= RegexOptions.CultureInvariant;

        if ((patternOptions & PatternOptions.ECMAScript) != 0)
            regexOptions |= RegexOptions.ECMAScript;

        if ((patternOptions & PatternOptions.ExplicitCapture) != 0)
            regexOptions |= RegexOptions.ExplicitCapture;

        if ((patternOptions & PatternOptions.IgnoreCase) != 0)
            regexOptions |= RegexOptions.IgnoreCase;

        if ((patternOptions & PatternOptions.IgnorePatternWhitespace) != 0)
            regexOptions |= RegexOptions.IgnorePatternWhitespace;

        if ((patternOptions & PatternOptions.Multiline) != 0)
            regexOptions |= RegexOptions.Multiline;

        if ((patternOptions & PatternOptions.RightToLeft) != 0)
            regexOptions |= RegexOptions.RightToLeft;

        if ((patternOptions & PatternOptions.Singleline) != 0)
            regexOptions |= RegexOptions.Singleline;

        return true;
    }

    private bool TryParseMatchTimeout(string value, out TimeSpan matchTimeout)
    {
        if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int seconds))
        {
            try
            {
                matchTimeout = TimeSpan.FromSeconds(seconds);
                return true;
            }
            catch (Exception ex) when (ex is ArgumentException
                || ex is OverflowException)
            {
            }
        }

        matchTimeout = default;
        return false;
    }

    internal bool TryParseProperties(
        string ask,
        IEnumerable<string> name,
        CommonFindCommandOptions options,
        bool allowEmptyPattern = false)
    {
        if (!TryParseAsEnum(
            ask,
            OptionNames.Ask,
            out AskMode askMode,
            defaultValue: AskMode.None,
            OptionValueProviders.AskModeProvider))
        {
            return false;
        }

        if (askMode == AskMode.Value
            && Logger.ConsoleOut.Verbosity < Verbosity.Normal)
        {
            Logger.WriteError($"Option '{OptionNames.GetHelpText(OptionNames.Ask)}' cannot have value "
                + $"'{OptionValueProviders.AskModeProvider.GetValue(nameof(AskMode.Value)).HelpValue}' when "
                + $"'{OptionNames.GetHelpText(OptionNames.Verbosity)}' is set to "
                + $"'{OptionValueProviders.VerbosityProvider.GetValue(Logger.ConsoleOut.Verbosity.ToString()).HelpValue}'.");

            return false;
        }

        if (!TryParseFilter(
            name,
            OptionNames.Name,
            OptionValueProviders.PatternOptionsProvider,
            out Matcher? nameFilter,
            out FileNamePart namePart,
            allowNull: true,
            allowEmptyPattern: allowEmptyPattern))
        {
            return false;
        }

        options.AskMode = askMode;
        options.NameFilter = nameFilter;
        options.NamePart = namePart;

        return true;
    }

    public bool TryParseTargetDirectory(
        string value,
        [NotNullWhen(true)] out string? result,
        CommonCopyCommandOptions options,
        string directoryName,
        string optionName)
    {
        result = null;

        if (value is not null)
        {
            if (!TryEnsureFullPath(value, out result))
                return false;
        }
        else
        {
            int length = options.Paths.Length;

            if (length < 2)
            {
                Logger.WriteError($"{directoryName} directory is required. It can be specified either as a last unnamed parameter "
                    + $"or using option '{OptionNames.GetHelpText(optionName)}'.");

                return false;
            }
            else
            {
                result = options.Paths[length - 1].Path;
                options.Paths = options.Paths.RemoveAt(length - 1);
            }
        }

        return true;
    }
}
