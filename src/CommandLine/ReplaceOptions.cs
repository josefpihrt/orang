// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal class ReplaceOptions
    {
        public ReplaceOptions(
            string replacement = null,
            MatchEvaluator matchEvaluator = null,
            ReplaceFunctions functions = ReplaceFunctions.None,
            bool cultureInvariant = false)
        {
            Replacement = replacement;
            MatchEvaluator = matchEvaluator;

            if (MatchEvaluator == null)
            {
                if (replacement != null
                    || functions == ReplaceFunctions.None)
                {
                    replacement ??= "";

                    MatchEvaluator = new MatchEvaluator(match => match.Result(replacement));
                }
            }

            Functions = functions;
            CultureInvariant = cultureInvariant;
        }

        public string Replacement { get; }

        public MatchEvaluator MatchEvaluator { get; }

        public ReplaceFunctions Functions { get; }

        public bool CultureInvariant { get; }

        public CultureInfo CultureInfo => (CultureInvariant) ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;

        public bool TrimStart => (Functions & ReplaceFunctions.TrimStart) != 0;

        public bool TrimEnd => (Functions & ReplaceFunctions.TrimEnd) != 0;

        public bool Trim => (Functions & ReplaceFunctions.Trim) == ReplaceFunctions.Trim;

        public bool ToLower => (Functions & ReplaceFunctions.ToLower) != 0;

        public bool ToUpper => (Functions & ReplaceFunctions.ToUpper) != 0;

        public string Replace(Match match)
        {
            string s = MatchEvaluator?.Invoke(match) ?? match.Value;

            if (Functions != ReplaceFunctions.None)
            {
                if (Trim)
                {
                    s = s.Trim();
                }
                else if (TrimStart)
                {
                    s = s.TrimStart();
                }
                else if (TrimEnd)
                {
                    s = s.TrimEnd();
                }

                if (ToLower)
                {
                    s = s.ToLower(CultureInfo);
                }
                else if (ToUpper)
                {
                    s = s.ToUpper(CultureInfo);
                }
            }

            return s;
        }
    }
}
