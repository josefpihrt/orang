// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;

namespace Orang.CommandLine
{
    internal static class PatternBuilder
    {
        public static string Join(
            IEnumerable<string> values,
            bool literal = false)
        {
            using (IEnumerator<string> en = values.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    StringBuilder sb = StringBuilderCache.GetInstance();

                    sb.Append((literal) ? "(?n:" : "(?:");

                    while (true)
                    {
                        sb.Append((literal) ? "(" : "(?:");
                        sb.Append((literal) ? RegexEscape.Escape(en.Current) : en.Current);
                        sb.Append(")");

                        if (en.MoveNext())
                        {
                            sb.Append("|");
                        }
                        else
                        {
                            break;
                        }
                    }

                    sb.Append(")");

                    return StringBuilderCache.GetStringAndFree(sb);
                }
            }

            return null;
        }
    }
}
