// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Orang.Text.RegularExpressions;

namespace Orang;

public static class Pattern
{
    public static string FromText(
        string text,
        PatternCreationOptions patternOptions,
        string separator = ",")
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (separator is null)
        {
            throw new ArgumentNullException(nameof(separator));
        }

        bool literal = (patternOptions & PatternCreationOptions.Literal) != 0;

        if ((patternOptions & PatternCreationOptions.List) != 0)
        {
            IEnumerable<string> values = text.Split(separator ?? ",", StringSplitOptions.RemoveEmptyEntries);

            text = JoinValues(values, literal);
        }
        else if (literal)
        {
            text = RegexEscape.Escape(text);
        }

        else if ((patternOptions & PatternCreationOptions.WholeLine) != 0)
        {
            text = @"(?:\A|(?<=\n))(?:" + text + @")(?:\z|(?=\r?\n))";
        }
        else if ((patternOptions & PatternCreationOptions.WholeWord) != 0)
        {
            text = @"\b(?:" + text + @")\b";
        }

        if ((patternOptions & PatternCreationOptions.Equals) == PatternCreationOptions.Equals)
        {
            return @"\A(?:" + text + @")\z";
        }
        else if ((patternOptions & PatternCreationOptions.StartsWith) != 0)
        {
            return @"\A(?:" + text + ")";
        }
        else if ((patternOptions & PatternCreationOptions.EndsWith) != 0)
        {
            return "(?:" + text + @")\z";
        }

        return text;
    }

    private static string JoinValues(IEnumerable<string> values, bool literal)
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

    private static void AppendValue(string value, bool literal, StringBuilder sb)
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
