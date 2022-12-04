// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Orang.FileSystem;

internal static class FileSystemExtensions
{
    public static FileMatcher IncludeExtensions(this FileMatcher matcher, params string[] extensions)
    {
        return SetExtensions(matcher, isNegative: false, extensions);
    }

    public static FileMatcher ExcludeExtensions(this FileMatcher matcher, params string[] extensions)
    {
        return SetExtensions(matcher, isNegative: true, extensions);
    }

    private static FileMatcher SetExtensions(FileMatcher matcher, bool isNegative, params string[] extensions)
    {
        matcher.Extension = new Matcher(
            Pattern.FromValues(
                extensions,
                PatternCreationOptions.Equals | PatternCreationOptions.Literal),
            (FileSystemHelpers.IsCaseSensitive) ? RegexOptions.None : RegexOptions.IgnoreCase,
            isNegative: isNegative);

        return matcher;
    }
}
