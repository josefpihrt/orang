// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Orang.Text.RegularExpressions;

namespace Orang.FileSystem;

internal static class FileSystemExtensions
{
    public static ReplaceResult GetResult(
        this FileMatch fileMatch,
        Replacer replacer,
        int count = 0,
        Func<string, bool>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        List<Capture> captures = ListCache<Capture>.GetInstance();

        MaxReason maxReason = CaptureFactory.GetCaptures(
            ref captures,
            fileMatch.NameMatch!,
            count: count,
            predicate: predicate,
            cancellationToken: cancellationToken);

        int offset = 0;
        List<ReplaceItem> replaceItems = ListCache<ReplaceItem>.GetInstance();

        if (replaceItems.Capacity < captures.Count)
            replaceItems.Capacity = captures.Count;

        foreach (Match match in captures)
        {
            string value = replacer.Replace(match);

            replaceItems.Add(new ReplaceItem(match, value, match.Index + offset));

            offset += value.Length - match.Length;

            cancellationToken.ThrowIfCancellationRequested();
        }

        ListCache<Capture>.Free(captures);

        string newName = GetNewName(fileMatch, replaceItems);

        return new ReplaceResult(replaceItems, maxReason, newName);
    }

    private static string GetNewName(FileMatch fileMatch, List<ReplaceItem> replaceItems)
    {
        StringBuilder sb = StringBuilderCache.GetInstance();

        string path = fileMatch.Path;
        FileNameSpan span = fileMatch.NameSpan;

        int lastPos = span.Start;

        foreach (ReplaceItem item in replaceItems)
        {
            Match match = item.Match;

            sb.Append(path, lastPos, span.Start + match.Index - lastPos);
            sb.Append(item.Value);

            lastPos = span.Start + match.Index + match.Length;
        }

        sb.Append(path, lastPos, path.Length - lastPos);

        return StringBuilderCache.GetStringAndFree(sb);
    }

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
