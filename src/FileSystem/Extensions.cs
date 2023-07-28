// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using Orang.FileSystem;

namespace Orang;

internal static class Extensions
{
    public static Match? Match(this Matcher matcher, FileNameSpan name)
    {
        return matcher.Match(matcher.Regex.Match(name.Path, name.Start, name.Length));
    }

    public static bool IsMatch(this Matcher matcher, FileNameSpan name)
    {
        return Match(matcher, name) is not null;
    }

    internal static bool IsDirectoryMatch(this Matcher matcher, string path, FileNamePart part)
    {
        return IsMatch(matcher, FileNameSpan.FromDirectory(path, part));
    }
}
