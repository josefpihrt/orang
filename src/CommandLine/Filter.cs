// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Orang.FileSystem;

namespace Orang
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Filter
    {
        public Filter(
            Regex regex,
            NamePartKind namePart = NamePartKind.Name,
            int groupNumber = -1,
            bool isNegative = false)
        {
            Regex = regex ?? throw new ArgumentNullException(nameof(regex));

            Debug.Assert(groupNumber < 0 || regex.GetGroupNumbers().Contains(groupNumber), groupNumber.ToString());

            GroupNumber = groupNumber;
            IsNegative = isNegative;
            NamePart = namePart;
        }

        public Regex Regex { get; }

        public bool IsNegative { get; }

        public int GroupNumber { get; }

        public NamePartKind NamePart { get; }

        public string GroupName => Regex.GroupNameFromNumber(GroupNumber);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"Negative = {IsNegative}  Part = {NamePart}  Group {GroupNumber}  {Regex}";

        internal bool IsMatch(string input)
        {
            return IsMatch(Regex.Match(input));
        }

        internal bool IsMatch(in NamePart part)
        {
            return IsMatch(Regex.Match(part.Path, part.Index, part.Length));
        }

        public bool IsMatch(Match match)
        {
            return (IsNegative)
                ? ((GroupNumber < 1) ? !match.Success : !match.Groups[GroupNumber].Success)
                : ((GroupNumber < 1) ? match.Success : match.Groups[GroupNumber].Success);
        }
    }
}
