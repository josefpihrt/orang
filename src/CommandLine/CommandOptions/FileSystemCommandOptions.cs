// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Orang.FileSystem;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

internal abstract class FileSystemCommandOptions : CommonRegexCommandOptions
{
    internal FileSystemCommandOptions()
    {
    }

    public ImmutableArray<PathInfo> Paths { get; internal set; }

    public Matcher? NameFilter { get; internal set; }

    public FileNamePart NamePart { get; internal set; }

    public Matcher? ExtensionFilter { get; internal set; }

    public Matcher? ContentFilter { get; internal set; }

    public Matcher? DirectoryFilter { get; internal set; }

    public FileNamePart DirectoryNamePart { get; internal set; }

    public SearchTarget SearchTarget { get; internal set; }

    public FileAttributes Attributes { get; internal set; }

    public FileAttributes AttributesToSkip { get; internal set; }

    public bool RecurseSubdirectories { get; internal set; }

    public bool Progress { get; internal set; }

    public Encoding DefaultEncoding { get; internal set; } = null!;

    public FileEmptyOption EmptyOption { get; internal set; }

    public int MaxMatchingFiles { get; internal set; }

    public SortOptions? SortOptions { get; internal set; }

    public bool AlignColumns { get; internal set; }

    public FilePropertyOptions FilePropertyOptions { get; internal set; } = null!;

    public FilterPredicate<DateTime>? CreationTimePredicate { get; internal set; }

    public FilterPredicate<DateTime>? ModifiedTimePredicate { get; internal set; }

    public FilterPredicate<long>? SizePredicate { get; internal set; }

    public ContentDisplayStyle ContentDisplayStyle => Format.ContentDisplayStyle;

    public bool OmitContent => ContentDisplayStyle == ContentDisplayStyle.Omit;

    public PathDisplayStyle PathDisplayStyle => Format.PathDisplayStyle;

    public bool OmitPath => PathDisplayStyle == PathDisplayStyle.Omit;

    public bool DisplayRelativePath => PathDisplayStyle == PathDisplayStyle.Relative;

    public string Indent => Format.Indent;

    internal MatchOutputInfo? CreateOutputInfo(string input, Match match, Matcher matcher)
    {
        if (ContentDisplayStyle != ContentDisplayStyle.ValueDetail)
            return null;

        int groupNumber = matcher.GroupNumber;

        return MatchOutputInfo.Create(
            MatchData.Create(input, matcher.Regex, match),
            groupNumber,
            includeGroupNumber: groupNumber >= 0,
            includeCaptureNumber: false,
            new OutputCaptions(shortMatch: ""));
    }
}
