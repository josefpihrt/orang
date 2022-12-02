// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Orang.FileSystem;

public static class Extensions
{
    public static FileSystemFilter IncludeExtensions(this FileSystemFilter filter, params string[] extensions)
    {
        return SetExtensions(filter, isNegative: false, extensions);
    }

    public static FileSystemFilter ExcludeExtensions(this FileSystemFilter filter, params string[] extensions)
    {
        return SetExtensions(filter, isNegative: true, extensions);
    }

    private static FileSystemFilter SetExtensions(this FileSystemFilter filter, bool isNegative, params string[] extensions)
    {
        filter.Extension = new Filter(
            Pattern.FromValues(
                extensions,
                PatternCreationOptions.Equals | PatternCreationOptions.Literal),
            (FileSystemHelpers.IsCaseSensitive) ? RegexOptions.None : RegexOptions.IgnoreCase,
            isNegative: isNegative);

        return filter;
    }
}
