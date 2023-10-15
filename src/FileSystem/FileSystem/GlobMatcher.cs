// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Orang.FileSystem;

internal sealed class GlobMatcher
{
    private GlobMatcher(Microsoft.Extensions.FileSystemGlobbing.Matcher matcher)
    {
        Matcher = matcher;
    }

    public Microsoft.Extensions.FileSystemGlobbing.Matcher Matcher { get; }

    public static GlobMatcher? TryCreate(
        IEnumerable<string> include,
        IEnumerable<string> exclude)
    {
        if (!include.Any()
            && !exclude.Any())
        {
            return null;
        }

        var matcher = new Microsoft.Extensions.FileSystemGlobbing.Matcher(
            (FileSystemUtilities.IsCaseSensitive) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

        if (include.Any())
        {
            matcher.AddIncludePatterns(include);
        }
        else
        {
            matcher.AddInclude("**");
        }

        if (exclude.Any())
            matcher.AddExcludePatterns(exclude);

        return new GlobMatcher(matcher);
    }

    public bool IsMatch(string filePath)
    {
        PatternMatchingResult result = Matcher.Match(filePath);

        if (!result.HasMatches)
            Debug.WriteLine($"Excluding file '{filePath}'");

        return result.HasMatches;
    }

    public bool IsMatch(string rootDirPath, string filePath)
    {
        PatternMatchingResult result = Matcher.Match(rootDirPath, filePath);

        if (!result.HasMatches)
            Debug.WriteLine($"Excluding file '{filePath}'");

        return result.HasMatches;
    }
}
