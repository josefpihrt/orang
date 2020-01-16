// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Orang.CommandLine;

namespace Orang
{
    internal static class Extensions
    {
        public static void WriteLineIf(this TextWriter writer, bool condition, string value)
        {
            if (condition)
                writer.WriteLine(value);
        }

        public static IEnumerable<RegexOptions> GetFlags(this RegexOptions options)
        {
            return GetFlags();

            IEnumerable<RegexOptions> GetFlags()
            {
                if ((options & RegexOptions.IgnoreCase) != 0)
                    yield return RegexOptions.IgnoreCase;

                if ((options & RegexOptions.Multiline) != 0)
                    yield return RegexOptions.Multiline;

                if ((options & RegexOptions.ExplicitCapture) != 0)
                    yield return RegexOptions.ExplicitCapture;

                if ((options & RegexOptions.Compiled) != 0)
                    yield return RegexOptions.Compiled;

                if ((options & RegexOptions.Singleline) != 0)
                    yield return RegexOptions.Singleline;

                if ((options & RegexOptions.IgnorePatternWhitespace) != 0)
                    yield return RegexOptions.IgnorePatternWhitespace;

                if ((options & RegexOptions.RightToLeft) != 0)
                    yield return RegexOptions.RightToLeft;

                if ((options & RegexOptions.ECMAScript) != 0)
                    yield return RegexOptions.ECMAScript;

                if ((options & RegexOptions.CultureInvariant) != 0)
                    yield return RegexOptions.CultureInvariant;
            }
        }

        public static IEnumerable<string> Modify(
            this IEnumerable<string> values,
            ModifyOptions options,
            ModifyFunctions? filter = null)
        {
            ModifyFunctions functions = options.Functions;

            if (filter != null)
                functions &= filter.Value;

            StringComparer comparer = options.StringComparer;

            bool trimStart = (functions & ModifyFunctions.TrimStart) != 0;
            bool trimEnd = (functions & ModifyFunctions.TrimEnd) != 0;

            if (trimStart && trimEnd)
            {
                values = values.Select(f => f.Trim());
            }
            else if (trimStart)
            {
                values = values.Select(f => f.TrimStart());
            }
            else if (trimEnd)
            {
                values = values.Select(f => f.TrimEnd());
            }

            if ((functions & ModifyFunctions.RemoveEmpty) != 0)
                values = values.Where(f => !string.IsNullOrEmpty(f));

            if ((functions & ModifyFunctions.RemoveWhiteSpace) != 0)
                values = values.Where(f => !string.IsNullOrWhiteSpace(f));

            if ((functions & ModifyFunctions.ToLower) != 0)
            {
                values = values.Select(f => f.ToLower((options.CultureInvariant) ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture));
            }
            else if ((functions & ModifyFunctions.ToUpper) != 0)
            {
                values = values.Select(f => f.ToUpper((options.CultureInvariant) ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture));
            }

            if (options.Modify != null)
                values = options.Modify(values);

            if ((functions & ModifyFunctions.Distinct) != 0)
                values = values.Distinct(comparer);

            if ((functions & ModifyFunctions.Sort) != 0)
            {
                if (options.SortProperty == ValueSortProperty.Length)
                {
                    return values.OrderBy(f => f.Length);
                }
                else
                {
                    return values.OrderBy(f => f, comparer);
                }
            }
            else if ((functions & ModifyFunctions.SortDescending) != 0)
            {
                if (options.SortProperty == ValueSortProperty.Length)
                {
                    return values.OrderByDescending(f => f.Length);
                }
                else
                {
                    return values.OrderByDescending(f => f, comparer);
                }
            }

            return values;
        }

        public static int GetDigitCount(this int value)
        {
            if (value < 0)
                value = -value;

            if (value < 10)
                return 1;

            if (value < 100)
                return 2;

            if (value < 1000)
                return 3;

            if (value < 10000)
                return 4;

            if (value < 100000)
                return 5;

            if (value < 1000000)
                return 6;

            if (value < 10000000)
                return 7;

            if (value < 100000000)
                return 8;

            if (value < 1000000000)
                return 9;

            return 10;
        }

        public static OperationCanceledException GetOperationCanceledException(this AggregateException aggregateException)
        {
            OperationCanceledException operationCanceledException = null;

            foreach (Exception ex in aggregateException.InnerExceptions)
            {
                if (ex is OperationCanceledException operationCanceledException2)
                {
                    if (operationCanceledException == null)
                        operationCanceledException = operationCanceledException2;
                }
                else if (ex is AggregateException aggregateException2)
                {
                    foreach (Exception ex2 in aggregateException2.InnerExceptions)
                    {
                        if (ex2 is OperationCanceledException operationCanceledException3)
                        {
                            if (operationCanceledException == null)
                                operationCanceledException = operationCanceledException3;
                        }
                        else
                        {
                            return null;
                        }
                    }

                    return operationCanceledException;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }
    }
}
