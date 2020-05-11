// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal class ReplaceOptions
    {
        public ReplaceOptions(
            string replacement = null,
            ReplaceFunctions functions = ReplaceFunctions.None,
            bool cultureInvariant = false)
        {
            Replacement = replacement ?? "";
            Functions = functions;
            CultureInvariant = cultureInvariant;
        }

        public ReplaceOptions(
            MatchEvaluator matchEvaluator,
            ReplaceFunctions functions = ReplaceFunctions.None,
            bool cultureInvariant = false)
        {
            MatchEvaluator = matchEvaluator ?? throw new ArgumentNullException(nameof(matchEvaluator));
            Functions = functions;
            CultureInvariant = cultureInvariant;
        }

        public string Replacement { get; }

        public MatchEvaluator MatchEvaluator { get; }

        public ReplaceFunctions Functions { get; }

        public bool CultureInvariant { get; }

        internal CultureInfo CultureInfo => (CultureInvariant) ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;

        public string Replace(Match match)
        {
            string s = (MatchEvaluator != null) ? MatchEvaluator(match) : match.Result(Replacement);

            if (Functions != ReplaceFunctions.None)
            {
                if ((Functions & ReplaceFunctions.TrimStart) != 0)
                {
                    if ((Functions & ReplaceFunctions.TrimEnd) != 0)
                    {
                        s = s.Trim();
                    }
                    else
                    {
                        s = s.TrimStart();
                    }
                }
                else if ((Functions & ReplaceFunctions.TrimEnd) != 0)
                {
                    s = s.TrimEnd();
                }

                if ((Functions & ReplaceFunctions.ToLower) != 0)
                {
                    s = s.ToLower(CultureInfo);
                }
                else if ((Functions & ReplaceFunctions.ToUpper) != 0)
                {
                    s = s.ToUpper(CultureInfo);
                }
            }

            return s;
        }
    }
}
