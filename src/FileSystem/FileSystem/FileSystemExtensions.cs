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
    internal static string GetReplacement(
        this FileMatch fileMatch,
        Replacer replacer,
        int count = 0,
        Func<string, bool>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        ReplaceResult result = GetReplaceResult(fileMatch, replacer, count, predicate, cancellationToken);

        ListCache<ReplaceItem>.Free(result.Items);

        return result.NewName;
    }

    internal static ReplaceResult GetReplaceResult(
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

        string newName = GetNewValue(fileMatch, replaceItems);

        return new ReplaceResult(replaceItems, maxReason, newName);
    }

    private static string GetNewValue(FileMatch fileMatch, List<ReplaceItem> replaceItems)
    {
        StringBuilder sb = StringBuilderCache.GetInstance();

        string path = fileMatch.Path;
        FileNameSpan span = fileMatch.NameSpan;

        int lastPos = span.Start;

        foreach (ReplaceItem item in replaceItems)
        {
            Match match = item.Match;

            sb.Append(path, lastPos, match.Index - lastPos);
            sb.Append(item.Value);

            lastPos = match.Index + match.Length;
        }

        sb.Append(path, lastPos, span.Start + span.Length - lastPos);

        return StringBuilderCache.GetStringAndFree(sb);
    }
}
