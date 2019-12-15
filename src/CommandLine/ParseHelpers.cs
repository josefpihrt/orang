// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Orang.Expressions;
using Orang.FileSystem;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal static class ParseHelpers
    {
        public static bool TryParseFileProperties(
            IEnumerable<string> values,
            string optionName,
            out FilePropertyFilter filter)
        {
            filter = null;
            Func<long, bool> sizePredicate = null;
            Func<DateTime, bool> creationTimePredicate = null;
            Func<DateTime, bool> modifiedTimePredicate = null;

            foreach (string value in values)
            {
                try
                {
                    Expression expression = Expression.Parse(value);

                    if (OptionValues.FileProperty_Size.IsKeyOrShortKey(expression.Identifier))
                    {
                        if (expression.Kind == ExpressionKind.DecrementExpression)
                        {
                            WriteError($"Option '{OptionNames.GetHelpText(optionName)}' has invalid expression '{value}'.");
                            return false;
                        }

                        sizePredicate = PredicateHelpers.GetLongPredicate(expression);
                    }
                    else if (OptionValues.FileProperty_CreationTime.IsKeyOrShortKey(expression.Identifier))
                    {
                        creationTimePredicate = PredicateHelpers.GetDateTimePredicate(expression);
                    }
                    else if (OptionValues.FileProperty_ModifiedTime.IsKeyOrShortKey(expression.Identifier))
                    {
                        modifiedTimePredicate = PredicateHelpers.GetDateTimePredicate(expression);
                    }
                    else
                    {
                        WriteParseError(value, optionName, OptionValueProviders.FilePropertiesProvider);
                        return false;
                    }
                }
                catch (ArgumentException)
                {
                    WriteError($"Option '{OptionNames.GetHelpText(optionName)}' has invalid expression '{value}'.");
                    return false;
                }
            }

            filter = new FilePropertyFilter(
                sizePredicate: sizePredicate,
                creationTimePredicate: creationTimePredicate,
                modifiedTimePredicate: modifiedTimePredicate);

            return true;
        }

        public static bool TryParseSortOptions(
            IEnumerable<string> values,
            string optionName,
            out SortOptions sortOptions)
        {
            sortOptions = null;
            int maxCount = 0;

            List<string> options = null;

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
                        if (!TryParseCount(value2, out maxCount, value))
                            return false;
                    }
                    else
                    {
                        WriteParseError(value, optionName, OptionValueProviders.SortFlagsProvider);
                        return false;
                    }
                }
                else
                {
                    (options ?? (options = new List<string>())).Add(value);
                }
            }

            if (!TryParseAsEnumValues(options, optionName, out ImmutableArray<SortFlags> flags, provider: OptionValueProviders.SortFlagsProvider))
                return false;

            SortDirection direction = (flags.Contains(SortFlags.Descending))
                ? SortDirection.Descending
                : SortDirection.Ascending;

            List<SortDescriptor> descriptors = null;

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
                        {
                            break;
                        }
                    default:
                        {
                            throw new InvalidOperationException($"Unknown enum value '{flag}'.");
                        }
                }
            }

            if (descriptors != null)
            {
                sortOptions = new SortOptions(descriptors.ToImmutableArray(), maxCount: maxCount);
            }
            else
            {
                sortOptions = new SortOptions(ImmutableArray.Create(new SortDescriptor(SortProperty.Name, SortDirection.Ascending)), maxCount: maxCount);
            }

            return true;

            void AddDescriptor(SortProperty p, SortDirection d)
            {
                (descriptors ?? (descriptors = new List<SortDescriptor>())).Add(new SortDescriptor(p, d));
            }
        }

        public static bool TryParseDisplay(
            IEnumerable<string> values,
            string optionName,
            out ContentDisplayStyle? contentDisplayStyle,
            out PathDisplayStyle? pathDisplayStyle,
            out LineDisplayOptions lineDisplayOptions,
            out DisplayParts displayParts,
            out ImmutableArray<FileProperty> fileProperties,
            out string indent,
            out string separator,
            OptionValueProvider contentDisplayStyleProvider = null,
            OptionValueProvider pathDisplayStyleProvider = null)
        {
            contentDisplayStyle = null;
            pathDisplayStyle = null;
            lineDisplayOptions = LineDisplayOptions.None;
            displayParts = DisplayParts.None;
            fileProperties = ImmutableArray<FileProperty>.Empty;
            indent = null;
            separator = null;

            ImmutableArray<FileProperty>.Builder builder = null;

            foreach (string value in values)
            {
                int index = value.IndexOf('=');

                if (index >= 0)
                {
                    string key = value.Substring(0, index);
                    string value2 = value.Substring(index + 1);

                    if (OptionValues.Display_Content.IsKeyOrShortKey(key))
                    {
                        if (!TryParseAsEnum(value2, optionName, out ContentDisplayStyle contentDisplayStyle2, provider: contentDisplayStyleProvider))
                            return false;

                        contentDisplayStyle = contentDisplayStyle2;
                    }
                    else if (OptionValues.Display_Path.IsKeyOrShortKey(key))
                    {
                        if (!TryParseAsEnum(value2, optionName, out PathDisplayStyle pathDisplayStyle2, provider: pathDisplayStyleProvider))
                            return false;

                        pathDisplayStyle = pathDisplayStyle2;
                    }
                    else if (OptionValues.Display_Indent.IsKeyOrShortKey(key))
                    {
                        indent = value2;
                    }
                    else if (OptionValues.Display_Separator.IsKeyOrShortKey(key))
                    {
                        separator = value2;
                    }
                    else
                    {
                        ThrowException(value);
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
                    (builder ?? (builder = ImmutableArray.CreateBuilder<FileProperty>())).Add(FileProperty.CreationTime);
                }
                else if (OptionValues.Display_ModifiedTime.IsValueOrShortValue(value))
                {
                    (builder ?? (builder = ImmutableArray.CreateBuilder<FileProperty>())).Add(FileProperty.ModifiedTime);
                }
                else if (OptionValues.Display_Size.IsValueOrShortValue(value))
                {
                    (builder ?? (builder = ImmutableArray.CreateBuilder<FileProperty>())).Add(FileProperty.Size);
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
                else
                {
                    ThrowException(value);
                }
            }

            if (builder != null)
                fileProperties = builder.ToImmutableArray();

            return true;

            void ThrowException(string value)
            {
                string helpText = OptionValueProviders.DisplayProvider.GetHelpText();

                throw new ArgumentException($"Option '{OptionNames.GetHelpText(optionName)}' has invalid value '{value}'. Allowed values: {helpText}.", nameof(values));
            }
        }

        public static bool TryParseOutputOptions(
            IEnumerable<string> values,
            string optionName,
            out string path,
            out Verbosity verbosity,
            out Encoding encoding,
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
                        WriteParseError(value, optionName, OptionValueProviders.OutputFlagsProvider);
                        return false;
                    }
                }
                else if (OptionValues.Output_Append.IsValueOrShortValue(value))
                {
                    append = true;
                }
                else
                {
                    WriteParseError(value, optionName, OptionValueProviders.OutputFlagsProvider);
                    return false;
                }
            }

            return true;
        }

        public static bool TryParseFilter(
            IEnumerable<string> values,
            string optionName,
            out Filter filter,
            OptionValueProvider provider = null,
            NamePartKind defaultNamePart = NamePartKind.Name,
            PatternOptions includedPatternOptions = PatternOptions.None)
        {
            filter = null;

            string pattern = null;
            TimeSpan matchTimeout = Regex.InfiniteMatchTimeout;
            string groupName = null;
            NamePartKind namePart = defaultNamePart;
            string separator = null;
            Func<Group, bool> predicate = null;

            List<string> options;

            try
            {
                options = EnumerateOptions().ToList();
            }
            catch (ArgumentException ex)
            {
                WriteError(ex);
                return false;
            }

            return TryParseFilter(pattern, options, matchTimeout, groupName, namePart, optionName, separator, predicate, out filter, provider, includedPatternOptions);

            IEnumerable<string> EnumerateOptions()
            {
                using (IEnumerator<string> en = values.GetEnumerator())
                {
                    if (!en.MoveNext())
                        throw new ArgumentException("", nameof(values));

                    pattern = en.Current;

                    while (en.MoveNext())
                    {
                        string option = en.Current;

                        if (Expression.TryParse(option, out Expression expression))
                        {
                            if (!ParseExpression(expression))
                            {
                                string helpText = (provider ?? OptionValueProviders.PatternOptionsProvider).GetHelpText();

                                throw new ArgumentException($"Option '{OptionNames.GetHelpText(optionName)}' has invalid value '{option}'. Allowed values: {helpText}.", nameof(values));
                            }
                        }
                        else
                        {
                            yield return option;
                        }
                    }
                }
            }

            bool ParseExpression(Expression expression)
            {
                string key = expression.Identifier;

                if (expression.Kind == ExpressionKind.EqualsExpression)
                {
                    var equalsExpression = (BinaryExpression)expression;
                    string value = equalsExpression.Value;

                    if (OptionValues.Group.IsKeyOrShortKey(key))
                    {
                        groupName = value;
                        return true;
                    }
                    else if (OptionValues.ListSeparator.IsKeyOrShortKey(key))
                    {
                        separator = value;
                        return true;
                    }
                    else if (OptionValues.Part.IsKeyOrShortKey(key))
                    {
                        if (!TryParseAsEnum(value, out namePart, provider: OptionValueProviders.NamePartKindProvider))
                        {
                            string helpText = OptionValueProviders.NamePartKindProvider?.GetHelpText() ?? OptionValue.GetDefaultHelpText<NamePartKind>();
                            throw new ArgumentException($"Option '{OptionNames.GetHelpText(optionName)}' has invalid value '{value}'. Allowed values: {helpText}.", nameof(values));
                        }

                        return true;
                    }
                    else if (OptionValues.Timeout.IsKeyOrShortKey(key))
                    {
                        matchTimeout = ParseMatchTimeout(value);
                        return true;
                    }
                }

                if (expression.Kind != ExpressionKind.DecrementExpression
                    && OptionValues.Length.IsKeyOrShortKey(key))
                {
                    try
                    {
                        predicate = PredicateHelpers.GetLengthPredicate(expression);
                        return true;
                    }
                    catch (ArgumentException)
                    {
                    }
                }

                return false;
            }
        }

        private static bool TryParseFilter(
            string pattern,
            IEnumerable<string> options,
            TimeSpan matchTimeout,
            string groupName,
            NamePartKind namePart,
            string optionName,
            string separator,
            Func<Group, bool> predicate,
            out Filter filter,
            OptionValueProvider provider = null,
            PatternOptions includedPatternOptions = PatternOptions.None)
        {
            filter = null;

            if (!TryParseRegexOptions(options, optionName, out RegexOptions regexOptions, out PatternOptions patternOptions, provider))
                return false;

            patternOptions |= includedPatternOptions;

            switch (patternOptions & (PatternOptions.WholeWord | PatternOptions.WholeLine | PatternOptions.WholeInput))
            {
                case PatternOptions.None:
                case PatternOptions.WholeWord:
                case PatternOptions.WholeLine:
                case PatternOptions.WholeInput:
                    {
                        break;
                    }
                default:
                    {
                        WriteError($"Values '{OptionValueProviders.PatternOptionsProvider.GetValue(nameof(PatternOptions.WholeWord)).HelpValue}', '{OptionValueProviders.PatternOptionsProvider.GetValue(nameof(PatternOptions.WholeLine)).HelpValue}' and '{OptionValueProviders.PatternOptionsProvider.GetValue(nameof(PatternOptions.WholeInput)).HelpValue}' cannot be combined.");
                        return false;
                    }
            }

            bool fromFile = (patternOptions & PatternOptions.FromFile) != 0;
            bool literal = (patternOptions & PatternOptions.Literal) != 0;

            if (fromFile
                && !FileSystemHelpers.TryReadAllText(pattern, out pattern))
            {
                return false;
            }

            if ((patternOptions & PatternOptions.List) != 0)
            {
                IEnumerable<string> values = (fromFile && separator == null)
                    ? TextHelpers.ReadLines(pattern).Where(f => f.Length > 0)
                    : pattern.Split(separator ?? ",", StringSplitOptions.RemoveEmptyEntries);

                pattern = PatternBuilder.Join(values, literal: literal);
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
            else if ((patternOptions & PatternOptions.WholeInput) != 0)
            {
                pattern = @"\A(?:" + pattern + @")\z";
            }

            if (!TryParseRegex(pattern, regexOptions, matchTimeout, optionName, out Regex regex))
                return false;

            int groupIndex = -1;

            if (groupName != null)
            {
                groupIndex = regex.GroupNumberFromName(groupName);
                if (groupIndex == -1)
                {
                    string message = $"Group '{groupName}' does not exist.";

                    string[] groupNames = regex.GetGroupNames();

                    if (groupNames.Length > 1)
                        message += $" Existing group names: {TextHelpers.Join(", ", " and ", groupNames.Where(f => f != "0"))}.";

                    WriteError(message);

                    return false;
                }
            }

            filter = new Filter(regex, namePart, groupNumber: groupIndex, isNegative: (patternOptions & PatternOptions.Negative) != 0, predicate);
            return true;
        }

        public static bool TryParseRegex(
            string pattern,
            RegexOptions regexOptions,
            TimeSpan matchTimeout,
            string patternOptionName,
            out Regex regex)
        {
            regex = null;

            if (pattern == null)
                return false;

            try
            {
                regex = new Regex(pattern, regexOptions, matchTimeout);
                return true;
            }
            catch (ArgumentException ex)
            {
                WriteError(ex, $"Could not parse '{OptionNames.GetHelpText(patternOptionName)}' value: {ex.Message}");
                return false;
            }
        }

        public static bool TryParseRegexOptions(IEnumerable<string> options, string optionsParameterName, out RegexOptions regexOptions)
        {
            return TryParseAsEnumFlags(options, optionsParameterName, out regexOptions, provider: OptionValueProviders.RegexOptionsProvider);
        }

        internal static bool TryParseRegexOptions(
            IEnumerable<string> options,
            string optionsParameterName,
            out RegexOptions regexOptions,
            out PatternOptions patternOptions,
            OptionValueProvider provider = null)
        {
            regexOptions = RegexOptions.None;

            if (!TryParseAsEnumFlags(options, optionsParameterName, out patternOptions, provider: provider ?? OptionValueProviders.PatternOptionsProvider))
                return false;

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

        public static bool TryParseReplacement(
            IEnumerable<string> values,
            out string replacement)
        {
            if (!values.Any())
            {
                replacement = null;
                return true;
            }

            replacement = values.First();

            if (!TryParseAsEnumFlags(values.Skip(1), OptionNames.Replacement, out ReplacementOptions options, ReplacementOptions.None, OptionValueProviders.ReplacementOptionsProvider))
                return false;

            if ((options & ReplacementOptions.FromFile) != 0
                && !FileSystemHelpers.TryReadAllText(replacement, out replacement))
            {
                return false;
            }

            if ((options & ReplacementOptions.Literal) != 0)
                replacement = RegexEscape.EscapeSubstitution(replacement);

            if ((options & ReplacementOptions.CharacterEscapes) != 0)
                replacement = RegexEscape.ConvertCharacterEscapes(replacement);

            return true;
        }

        public static bool TryParseAsEnumFlags<TEnum>(
            IEnumerable<string> values,
            string optionName,
            out TEnum result,
            TEnum? defaultValue = null,
            OptionValueProvider provider = null) where TEnum : struct
        {
            result = (TEnum)(object)0;

            if (values?.Any() != true)
            {
                if (defaultValue != null)
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

        public static bool TryParseAsEnumValues<TEnum>(
            IEnumerable<string> values,
            string optionName,
            out ImmutableArray<TEnum> result,
            ImmutableArray<TEnum> defaultValue = default,
            OptionValueProvider provider = null) where TEnum : struct
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
                    return false;

                builder.Add(result2);
            }

            result = builder.ToImmutableArray();

            return true;
        }

        public static bool TryParseAsEnum<TEnum>(
            string value,
            string optionName,
            out TEnum result,
            TEnum? defaultValue = null,
            OptionValueProvider provider = null) where TEnum : struct
        {
            if (!TryParseAsEnum(value, out result, defaultValue, provider))
            {
                WriteParseError(value, optionName, provider?.GetHelpText() ?? OptionValue.GetDefaultHelpText<TEnum>());
                return false;
            }

            return true;
        }

        public static bool TryParseAsEnum<TEnum>(
            string value,
            out TEnum result,
            TEnum? defaultValue = null,
            OptionValueProvider provider = default) where TEnum : struct
        {
            if (value == null
                && defaultValue != null)
            {
                result = defaultValue.Value;
                return true;
            }

            if (provider != null)
            {
                return provider.TryParseEnum(value, out result);
            }
            else
            {
                return Enum.TryParse(value?.Replace("-", ""), ignoreCase: true, out result);
            }
        }

        public static bool TryParseVerbosity(string value, out Verbosity verbosity)
        {
            return TryParseAsEnum(value, OptionNames.Verbosity, out verbosity, provider: OptionValueProviders.VerbosityProvider);
        }

        // https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding#remarks
        public static bool TryParseEncoding(string name, out Encoding encoding)
        {
            if (name == "utf-8-no-bom")
            {
                encoding = EncodingHelpers.UTF8NoBom;
                return true;
            }

            try
            {
                encoding = Encoding.GetEncoding(name);
                return true;
            }
            catch (ArgumentException ex)
            {
                WriteError(ex);

                encoding = null;
                return false;
            }
        }

        public static bool TryParseEncoding(string name, out Encoding encoding, Encoding defaultEncoding)
        {
            if (name == null)
            {
                encoding = defaultEncoding;
                return true;
            }

            return TryParseEncoding(name, out encoding, defaultEncoding);
        }

        public static bool TryParseMaxCount(IEnumerable<string> values, out int maxCount, out int maxMatches, out int maxMatchingFiles)
        {
            maxCount = 0;
            maxMatches = 0;
            maxMatchingFiles = 0;

            if (!values.Any())
                return true;

            foreach (string value in values)
            {
                int index = value.IndexOf('=');

                if (index >= 0)
                {
                    string key = value.Substring(0, index);
                    string value2 = value.Substring(index + 1);

                    if (OptionValues.MaxMatches.IsKeyOrShortKey(key))
                    {
                        if (!TryParseCount(value2, out maxMatches, value))
                            return false;
                    }
                    else if (OptionValues.MaxMatchingFiles.IsKeyOrShortKey(key))
                    {
                        if (!TryParseCount(value2, out maxMatchingFiles, value))
                            return false;
                    }
                    else
                    {
                        WriteParseError(value, OptionNames.MaxCount, OptionValueProviders.MaxOptionsProvider);
                        return false;
                    }
                }
                else if (!TryParseCount(value, out maxCount))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseCount(string value, out int count, string value2 = null)
        {
            if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out count))
                return true;

            WriteError($"Option '{OptionNames.GetHelpText(OptionNames.MaxCount)}' has invalid value '{value2 ?? value}'.");
            return false;
        }

        private static TimeSpan ParseMatchTimeout(string value)
        {
            if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int seconds))
                throw new ArgumentException($"Option '{OptionNames.GetHelpText(OptionNames.Timeout)}' has invalid value '{value}'.");

            try
            {
                return TimeSpan.FromSeconds(seconds);
            }
            catch (Exception ex) when (ex is ArgumentException
                || ex is OverflowException)
            {
                throw new ArgumentException($"Option '{OptionNames.GetHelpText(OptionNames.Timeout)}' has invalid value '{value}'.");
            }
        }

        public static bool TryEnsureFullPath(IEnumerable<string> paths, out ImmutableArray<string> fullPaths)
        {
            ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>();

            foreach (string path in paths)
            {
                if (!TryEnsureFullPath(path, out string fullPath))
                    return false;

                builder.Add(fullPath);
            }

            fullPaths = builder.ToImmutableArray();
            return true;
        }

        public static bool TryEnsureFullPath(string path, out string result)
        {
            try
            {
                if (!Path.IsPathRooted(path))
                    path = Path.GetFullPath(path);

                result = path;
                return true;
            }
            catch (ArgumentException ex)
            {
                WriteError($"Path '{path}' is invalid: {ex.Message}.");
                result = null;
                return false;
            }
        }

        private static void WriteParseError(string value, string optionName, OptionValueProvider provider)
        {
            string helpText = provider.GetHelpText();

            WriteParseError(value, optionName, helpText);
        }

        private static void WriteParseError(string value, string optionName, string helpText)
        {
            WriteError($"Option '{OptionNames.GetHelpText(optionName)}' has invalid value '{value}'. Allowed values: {helpText}.");
        }
    }
}
