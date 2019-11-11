// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Orang.FileSystem;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal static class ParseHelpers
    {
        public static bool TryParseOutputOptions(
            IEnumerable<string> values,
            string optionName,
            out OutputOptions outputOptions)
        {
            outputOptions = null;
            Encoding encoding = null;
            bool includeContent = false;
            bool includePath = false;

            if (!values.Any())
                return true;

            if (!TryEnsureFullPath(values.First(), out string path))
                return false;

            foreach (string value in values.Skip(1))
            {
                int index = value.IndexOf('=');

                if (index >= 0)
                {
                    string key = value.Substring(0, index);
                    string value2 = value.Substring(index + 1);

                    if (OptionValues.Encoding.IsKeyOrShortKey(key))
                    {
                        if (!TryParseEncoding(value2, out encoding, Encoding.UTF8))
                            return false;
                    }
                    else
                    {
                        OnError(value);
                        return false;
                    }
                }
                else if (OptionValues.OutputOptions_Content.IsValueOrShortValue(value))
                {
                    includeContent = true;
                }
                else if (OptionValues.OutputOptions_Path.IsValueOrShortValue(value))
                {
                    includePath = true;
                }
                else
                {
                    OnError(value);
                    return false;
                }
            }

            if (!includePath
                && !includeContent)
            {
                includePath = true;
            }

            outputOptions = new OutputOptions(path, encoding ?? Encoding.UTF8, includeContent: includeContent, includePath: includePath);
            return true;

            void OnError(string value)
            {
                string helpText = OptionValueProviders.OutputOptionsProvider.GetHelpText();

                WriteError($"Option '{OptionNames.GetHelpText(optionName)}' has invalid value '{value}'. Allowed values: {helpText}.");
            }
        }

        public static bool TryParseDisplay(
            IEnumerable<string> values,
            string optionName,
            out ContentDisplayStyle contentDisplayStyle,
            out PathDisplayStyle pathDisplayStyle,
            ContentDisplayStyle defaultContentDisplayStyle,
            PathDisplayStyle defaultPathDisplayStyle,
            OptionValueProvider contentDisplayStyleProvider = null,
            OptionValueProvider pathDisplayStyleProvider = null)
        {
            if (!TryParseDisplay(
                values: values,
                optionName: optionName,
                contentDisplayStyle: out ContentDisplayStyle? contentDisplayStyle2,
                pathDisplayStyle: out PathDisplayStyle? pathDisplayStyle2,
                contentDisplayStyleProvider: contentDisplayStyleProvider,
                pathDisplayStyleProvider: pathDisplayStyleProvider))
            {
                contentDisplayStyle = 0;
                pathDisplayStyle = 0;
                return false;
            }

            contentDisplayStyle = contentDisplayStyle2 ?? defaultContentDisplayStyle;
            pathDisplayStyle = pathDisplayStyle2 ?? defaultPathDisplayStyle;
            return true;
        }

        public static bool TryParseDisplay(
            IEnumerable<string> values,
            string optionName,
            out ContentDisplayStyle? contentDisplayStyle,
            out PathDisplayStyle? pathDisplayStyle,
            OptionValueProvider contentDisplayStyleProvider = null,
            OptionValueProvider pathDisplayStyleProvider = null)
        {
            contentDisplayStyle = null;
            pathDisplayStyle = null;

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
                    else
                    {
                        ThrowException(value);
                    }
                }
                else
                {
                    ThrowException(value);
                }
            }

            return true;

            void ThrowException(string value)
            {
                string helpText = (OptionValueProviders.DisplayProvider ?? OptionValueProviders.PatternOptionsProvider).GetHelpText();

                throw new ArgumentException($"Option '{OptionNames.GetHelpText(optionName)}' has invalid value '{value}'. Allowed values: {helpText}.", nameof(values));
            }
        }

        public static bool TryParseFileLogOptions(
            IEnumerable<string> values,
            string optionName,
            out string path,
            out FileLogFlags flags,
            out Verbosity verbosity)
        {
            path = null;
            flags = FileLogFlags.None;
            verbosity = Verbosity.Normal;

            if (!values.Any())
                return true;

            if (!TryEnsureFullPath(values.First(), out path))
                return false;

            List<string> options = null;

            foreach (string value in values.Skip(1))
            {
                string option = value;

                int index = option.IndexOf('=');

                if (index >= 0)
                {
                    string key = option.Substring(0, index);
                    string value2 = option.Substring(index + 1);

                    if ((key.Length == 1 && key[0] == OptionShortNames.Verbosity)
                        || key == OptionNames.Verbosity)
                    {
                        if (!TryParseVerbosity(value2, out verbosity))
                            return false;
                    }
                    else
                    {
                        string helpText = OptionValueProviders.FileLogOptionsProvider.GetHelpText();

                        WriteError($"Option '{OptionNames.GetHelpText(optionName)}' has invalid value '{value}'. Allowed values: {helpText}.");
                        return false;
                    }
                }
                else
                {
                    (options ?? (options = new List<string>())).Add(option);
                }
            }

            return options == null
                || TryParseAsEnumFlags(options, optionName, out flags, provider: OptionValueProviders.FileLogOptionsProvider);
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

            return TryParseFilter(pattern, options, matchTimeout, groupName, namePart, optionName, separator, out filter, provider, includedPatternOptions);

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

                        int index = option.IndexOf('=');

                        if (index >= 0)
                        {
                            string key = option.Substring(0, index);
                            string value = option.Substring(index + 1);

                            if (OptionValues.Group.IsKeyOrShortKey(key))
                            {
                                groupName = value;
                            }
                            else if (OptionValues.ListSeparator.IsKeyOrShortKey(key))
                            {
                                separator = value;
                            }
                            else if (OptionValues.Part.IsKeyOrShortKey(key))
                            {
                                if (!TryParseAsEnum(value, out namePart, provider: OptionValueProviders.NamePartKindProvider))
                                {
                                    string helpText = OptionValueProviders.NamePartKindProvider?.GetHelpText() ?? OptionValue.GetDefaultHelpText<NamePartKind>();
                                    throw new ArgumentException($"Option '{OptionNames.GetHelpText(optionName)}' has invalid value '{value}'. Allowed values: {helpText}.", nameof(values));
                                }
                            }
                            else if (OptionValues.Timeout.IsKeyOrShortKey(key))
                            {
                                matchTimeout = ParseMatchTimeout(value);
                            }
                            else
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
        }

        private static bool TryParseFilter(
            string pattern,
            IEnumerable<string> options,
            TimeSpan matchTimeout,
            string groupName,
            NamePartKind namePart,
            string optionName,
            string separator,
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

            filter = new Filter(regex, namePart, groupNumber: groupIndex, isNegative: (patternOptions & PatternOptions.Negative) != 0);
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

            if ((options & ReplacementOptions.Multiline) != 0)
                replacement = RegexEscape.ConvertToMultiline(replacement);

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
                string helpText = provider?.GetHelpText() ?? OptionValue.GetDefaultHelpText<TEnum>();

                WriteError($"Option '{OptionNames.GetHelpText(optionName)}' has invalid value '{value}'. Allowed values: {helpText}.");
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
        public static bool TryParseEncoding(string name, out Encoding encoding, Encoding defaultEncoding)
        {
            if (name == null)
            {
                encoding = defaultEncoding;
                return true;
            }

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

        public static bool TryParseMaxCount(IEnumerable<string> values, out int maxMatches, out int maxMatchesInFile, out int maxMatchingFiles)
        {
            maxMatches = 0;
            maxMatchesInFile = 0;
            maxMatchingFiles = 0;

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
                    else if (OptionValues.MaxMatchesInFile.IsKeyOrShortKey(key))
                    {
                        if (!TryParseCount(value2, out maxMatchesInFile, value))
                            return false;
                    }
                    else if (OptionValues.MaxMatchingFiles.IsKeyOrShortKey(key))
                    {
                        if (!TryParseCount(value2, out maxMatchingFiles, value))
                            return false;
                    }
                    else
                    {
                        string helpText = OptionValueProviders.MaxOptionsProvider.GetHelpText();
                        WriteError($"Option '{OptionNames.GetHelpText(OptionNames.MaxCount)}' has invalid value '{value}'. Allowed values: {helpText}.");
                        return false;
                    }
                }
                else if (!TryParseCount(value, out maxMatches))
                {
                    return false;
                }
            }

            return true;

            bool TryParseCount(string value, out int count, string value2 = null)
            {
                if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out count))
                    return true;

                WriteError($"Option '{OptionNames.GetHelpText(OptionNames.MaxCount)}' has invalid value '{value2 ?? value}'.");
                return false;
            }
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
    }
}
