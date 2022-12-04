// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Orang;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Matcher
{
    internal static Matcher EntireInput { get; } = new(@"\A.+\z", RegexOptions.Singleline);

    public Matcher(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false) : this(new Regex(pattern, options), isNegative: isNegative)
    {
    }

    internal Matcher(
        Regex regex,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        Regex = regex ?? throw new ArgumentNullException(nameof(regex));

        Debug.Assert(groupNumber < 0 || regex.GetGroupNumbers().Contains(groupNumber), groupNumber.ToString());

        GroupNumber = groupNumber;
        IsNegative = isNegative;
        Predicate = predicate;
    }

    public Regex Regex { get; }

    //TODO: rename IsNegative
    public bool IsNegative { get; }

    internal int GroupNumber { get; }

    internal Func<string, bool>? Predicate { get; }

    internal string GroupName => Regex.GroupNameFromNumber(GroupNumber);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{nameof(IsNegative)} = {IsNegative}  {Regex}";

    public Match? Match(string input)
    {
        Match match = Regex.Match(input);

        return Match(match);
    }

    internal Match? Match(Match match)
    {
        if (Predicate is not null)
        {
            if (GroupNumber < 1)
            {
                while (match.Success)
                {
                    if (Predicate.Invoke(match.Value))
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
                        && Predicate.Invoke(group.Value))
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
        return Match(input) is not null;
    }
}
