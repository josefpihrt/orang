// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using Orang.FileSystem;

namespace Orang;

internal static class FileSystemExtensions
{
    public static void Report(
        this IProgress<SearchProgress> progress,
        string path,
        SearchProgressKind kind,
        bool isDirectory = false,
        Exception? exception = null)
    {
        progress.Report(new SearchProgress(path, kind, isDirectory, exception));
    }

    public static void Report(
        this IProgress<OperationProgress> progress,
        FileMatch fileMatch,
        string? newPath,
        OperationKind kind,
        Exception? exception = null)
    {
        progress.Report(new OperationProgress(fileMatch, newPath, kind, exception));
    }

    public static Match? Match(this Matcher matcher, FileNameSpan name)
    {
        return matcher.Match(matcher.Regex.Match(name.Path, name.Start, name.Length));
    }

    public static Match? Match(this Matcher matcher, FileNameSpan name, bool matchPartOnly)
    {
        return (matchPartOnly)
            ? matcher.Match(name.ToString())
            : matcher.Match(name);
    }

    public static bool IsMatch(this Matcher matcher, FileNameSpan name)
    {
        return Match(matcher, name) is not null;
    }
}
