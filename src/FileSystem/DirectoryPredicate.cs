// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Orang.FileSystem;
using Orang.Text.RegularExpressions;

namespace Orang;

internal static class DirectoryPredicate
{
    public static Func<string, bool> Create(Matcher matcher, FileNamePart part = FileNamePart.FullName)
    {
        return path => (matcher.Invert) ? !IsMatch(matcher, part, path) : IsMatch(matcher, part, path);
    }

    public static Func<string, bool> Create(string path, FileNamePart part = FileNamePart.FullName)
    {
        Matcher matcher = CreateFilter(path);

        return path => IsMatch(matcher, part, path);
    }

    public static Func<string, bool> Create(Func<string, bool>? predicate, string path)
    {
        Matcher directoryFilter = CreateFilter(path);

        if (predicate is not null)
        {
            return path =>
            {
                return predicate(path)
                    && IsMatch(directoryFilter, FileNamePart.FullName, path);
            };
        }
        else
        {
            return path => IsMatch(directoryFilter, FileNamePart.FullName, path);
        }
    }

    private static Matcher CreateFilter(string path)
    {
        string pattern = RegexEscape.Escape(path);

        var options = RegexOptions.ExplicitCapture;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            pattern = Regex.Replace(pattern, @"(/|\\\\)", @"(/|\\)", options);

        pattern = $@"\A{pattern}(?=(/";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            pattern += @"|\\)";
        }
        else
        {
            pattern += ")";
        }

        pattern += @"|\z)";

        if (!FileSystemUtilities.IsCaseSensitive)
            options |= RegexOptions.IgnoreCase;

        return new Matcher(pattern, options);
    }

    private static bool IsMatch(Matcher matcher, FileNamePart part, string path)
    {
        return matcher.IsDirectoryMatch(path, part);
    }
}
