// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

internal static class PatternBuilder
{
    public static string BuildPattern(
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
}
