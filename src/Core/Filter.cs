// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

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

        public Match Match(
            string input,
            CancellationToken cancellationToken = default)
        {
            Match match = Regex.Match(input);

            if (GroupNumber < 1)
            {
                while (match.Success)
                {
                    if (Predicate?.Invoke(match) != false)
                    {
                        return (IsNegative) ? null : match;
                    }

                    match = match.NextMatch();

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            else
            {
                while (match.Success)
                {
                    Group group = match.Groups[GroupNumber];

                    if (group.Success
                        && Predicate?.Invoke(group) != false)
                    {
                        return (IsNegative) ? null : match;
                    }

                    match = match.NextMatch();

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            return (IsNegative)
                ? System.Text.RegularExpressions.Match.Empty
                : null;
        }

        internal bool IsMatch(string input)
        {
            return IsMatch(Regex.Match(input));
        }

        internal bool IsMatch(Match match)
        {
            return (IsNegative) ? !IsMatchImpl(match) : IsMatchImpl(match);
        }

        private bool IsMatchImpl(Match match)
        {
            return IsMatchImpl((GroupNumber < 1) ? match : match.Groups[GroupNumber]);
        }

        private bool IsMatchImpl(Group group)
        {
            return group.Success && Predicate?.Invoke(group) != false;
        }
    }
}
