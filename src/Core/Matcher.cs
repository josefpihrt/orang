// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Orang;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Matcher
{
    internal static Matcher EntireInput { get; } = new(@"\A.+\z", RegexOptions.Singleline);

    public Matcher(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool invert = false,
        object? group = null,
        Func<string, bool>? predicate = null) : this(new Regex(pattern, options), invert, group, predicate)
    {
    }

    internal Matcher(
        Regex regex,
        bool invert = false,
        object? group = null,
        Func<string, bool>? predicate = null)
    {
        Regex = regex ?? throw new ArgumentNullException(nameof(regex));

        if (group is string groupName)
        {
            GroupNumber = regex.GroupNumberFromName(groupName);

            if (GroupNumber == -1)
                throw new ArgumentException($"Group name '{groupName}' was not found.", nameof(groupName));
        }
        else
        {
            GroupNumber = -1;
        }

        if (group is int groupNumber)
        {
            GroupName = regex.GroupNameFromNumber(groupNumber);

            if (GroupName.Length == 0)
                throw new ArgumentException($"Group number '{groupNumber}' was not found.", nameof(groupNumber));
        }
        else
        {
            GroupName = "";
        }

        Invert = invert;
        Predicate = predicate;
    }

    public Regex Regex { get; }

    public bool Invert { get; }

    public int GroupNumber { get; }

    public string GroupName { get; }

    public Func<string, bool>? Predicate { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => Regex.ToString();

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
                        return (Invert) ? null : match;

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
                        return (Invert) ? null : match;
                    }

                    match = match.NextMatch();
                }
            }
        }
        else if (GroupNumber < 1)
        {
            if (match.Success)
                return (Invert) ? null : match;
        }
        else
        {
            Group group = match.Groups[GroupNumber];

            if (group.Success)
                return (Invert) ? null : match;
        }

        return (Invert)
            ? System.Text.RegularExpressions.Match.Empty
            : null;
    }

    internal bool IsMatch(string input)
    {
        return Match(input) is not null;
    }
}
