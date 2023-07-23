﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Orang.Text.RegularExpressions;

namespace Orang;

public static class Pattern
{
    public static string From(
        string value,
        PatternOptions options)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        if ((options & PatternOptions.Literal) != 0)
            value = RegexEscape.Escape(value);

        return Create(value, options);
    }

    public static string Any(
        string value1,
        string value2,
        PatternOptions options = PatternOptions.None)
    {
        return Any(new string[] { value1, value2 }, options);
    }

    public static string Any(
        string value1,
        string value2,
        string value3,
        PatternOptions options = PatternOptions.None)
    {
        return Any(new string[] { value1, value2, value3 }, options);
    }

    public static string Any(
        IEnumerable<string> values,
        PatternOptions options = PatternOptions.None)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        string text = JoinValues(values, (options & PatternOptions.Literal) != 0);

        return Create(text, options);

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

    private static string Create(string text, PatternOptions options)
    {
        if ((options & PatternOptions.WholeLine) != 0)
        {
            text = @"(?:\A|(?<=\n))(?:" + text + @")(?:\z|(?=\r?\n))";
        }
        else if ((options & PatternOptions.WholeWord) != 0)
        {
            text = @"\b(?:" + text + @")\b";
        }

        if ((options & PatternOptions.Equals) == PatternOptions.Equals)
        {
            return @"\A(?:" + text + @")\z";
        }
        else if ((options & PatternOptions.StartsWith) != 0)
        {
            return @"\A(?:" + text + ")";
        }
        else if ((options & PatternOptions.EndsWith) != 0)
        {
            return "(?:" + text + @")\z";
        }

        return text;
    }
}