// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Orang
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Filter
    {
        public Filter(
            Regex regex,
            int groupNumber = -1,
            bool isNegative = false,
            Func<Capture, bool> predicate = null)
        {
            Regex = regex ?? throw new ArgumentNullException(nameof(regex));

            Debug.Assert(groupNumber < 0 || regex.GetGroupNumbers().Contains(groupNumber), groupNumber.ToString());

            GroupNumber = groupNumber;
            IsNegative = isNegative;
            Predicate = predicate;
        }

        public Regex Regex { get; }

        public bool IsNegative { get; }

        public int GroupNumber { get; }

        public Func<Capture, bool> Predicate { get; }

        public string GroupName => Regex.GroupNameFromNumber(GroupNumber);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"Negative = {IsNegative}  Group {GroupNumber}  {Regex}";

        public Match Match(string input)
        {
            Match match = Regex.Match(input);

            return Match(match);
        }

        private Match Match(Match match)
        {
            if (Predicate != null)
            {
                if (GroupNumber < 1)
                {
                    while (match.Success)
                    {
                        if (Predicate.Invoke(match))
                            return (IsNegative) ? null : match;

                        match = match.NextMatch();
                    }
                }
                else
                {
                    while (match.Success)
                    {
                        Group group = match.Groups[GroupNumber];

                        if (group.Success
                            && Predicate.Invoke(group))
                        {
                            return (IsNegative) ? null : match;
                        }

                        match = match.NextMatch();
                    }
                }
            }
            else if (GroupNumber < 1)
            {
                if (match.Success)
                    return (IsNegative) ? null : match;
            }
            else
            {
                Group group = match.Groups[GroupNumber];

                if (group.Success)
                    return (IsNegative) ? null : match;
            }

            return (IsNegative)
                ? System.Text.RegularExpressions.Match.Empty
                : null;
        }

        internal bool IsMatch(string input)
        {
            return Match(input) != null;
        }

        internal bool IsMatch(Match match)
        {
            return Match(match) != null;
        }
    }
}
