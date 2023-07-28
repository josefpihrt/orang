// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Orang.Text.RegularExpressions;

namespace Orang;

public static class Pattern
{
    public static string Create(
        string value,
        PatternOptions options)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        if ((options & PatternOptions.Literal) != 0)
            value = RegexEscape.Escape(value);

        return CreateCore(value, options);
    }

    public static string Create(
        string value1,
        string value2,
        PatternOptions options = PatternOptions.None)
    {
        return Create(new string[] { value1, value2 }, options);
    }

    public static string Create(
        string value1,
        string value2,
        string value3,
        PatternOptions options = PatternOptions.None)
    {
        return Create(new string[] { value1, value2, value3 }, options);
    }

    public static string Create(
        IEnumerable<string> values,
        PatternOptions options = PatternOptions.None)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        string text = JoinValues(values, (options & PatternOptions.Literal) != 0);

        return CreateCore(text, options);

        static string JoinValues(IEnumerable<string> values, bool literal)
        {
            using (IEnumerator<string> en = values.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    string value = en.Current;

                    if (en.MoveNext())
                    {
                        StringBuilder sb = StringBuilderCache.GetInstance();

                        AppendValue(value, literal, sb);

                        do
                        {
                            sb.Append("|");
                            AppendValue(en.Current, literal, sb);
                        }
                        while (en.MoveNext());

                        return StringBuilderCache.GetStringAndFree(sb);
                    }

                    return (literal) ? RegexEscape.Escape(value) : value;
                }

                return "";
            }
        }

        static void AppendValue(string value, bool literal, StringBuilder sb)
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

    private static string CreateCore(string value, PatternOptions options)
    {
        if ((options & (PatternOptions.WholeLine | PatternOptions.WholeWord)) == (PatternOptions.WholeLine | PatternOptions.WholeWord))
        {
            throw new InvalidOperationException($"'{nameof(PatternOptions)}.{PatternOptions.WholeLine}' "
                + $"and '{nameof(PatternOptions)}.{nameof(PatternOptions.WholeWord)}' cannot be set both at the same time.");
        }

        if ((options & PatternOptions.WholeLine) != 0)
        {
            value = @"(?:\A|(?<=\n))(?:" + value + @")(?:\z|(?=\r?\n))";
        }
        else if ((options & PatternOptions.WholeWord) != 0)
        {
            value = @"\b(?:" + value + @")\b";
        }

        if ((options & PatternOptions.Equals) == PatternOptions.Equals)
        {
            return @"\A(?:" + value + @")\z";
        }
        else if ((options & PatternOptions.StartsWith) != 0)
        {
            return @"\A(?:" + value + ")";
        }
        else if ((options & PatternOptions.EndsWith) != 0)
        {
            return "(?:" + value + @")\z";
        }

        return value;
    }
}
