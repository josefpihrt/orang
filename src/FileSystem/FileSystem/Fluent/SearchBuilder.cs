// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class SearchBuilder
{
    private static Matcher? _extension;
    private FileAttributes _attributesToSkip;
    private FileAttributes _attributes;
    private Func<FileInfo, bool>? _filePredicate;
    private Func<DirectoryInfo, bool>? _directoryPredicate;
    private FileEmptyOption _fileEmptyOption;
    private bool _topDirectoryOnly;
    private Func<string, bool>? _excludeDirectory;
    private IProgress<SearchProgress>? _searchProgress;
    private SearchTarget _searchTarget;
    private Encoding? _defaultEncoding;
    private Matcher? _name;
    private Matcher? _content;
    private FileNamePart _namePart;

    internal SearchBuilder()
    {
    }

    public static SearchBuilder Create()
    {
        return new SearchBuilder();
    }

    public SearchBuilder IncludeExtensions(params string[] extensions)
    {
        _extension = GetExtensionMatcher(isNegative: false, extensions);

        return this;
    }

    public SearchBuilder ExcludeExtensions(params string[] extensions)
    {
        _extension = GetExtensionMatcher(isNegative: true, extensions);

        return this;
    }

    private static Matcher? GetExtensionMatcher(bool isNegative, params string[] extensions)
    {
        if (extensions.Length == 0)
        {
            return null;
        }
        else if (extensions.Length == 1)
        {
            return new Matcher(
                Pattern.FromValue(
                    extensions[0],
                    PatternCreationOptions.Equals | PatternCreationOptions.Literal),
                (FileSystemHelpers.IsCaseSensitive) ? RegexOptions.None : RegexOptions.IgnoreCase,
                isNegative: isNegative);
        }
        else
        {
            return new Matcher(
                Pattern.FromValues(
                    extensions,
                    PatternCreationOptions.Equals | PatternCreationOptions.Literal),
                (FileSystemHelpers.IsCaseSensitive) ? RegexOptions.None : RegexOptions.IgnoreCase,
                isNegative: isNegative);
        }
    }

    public SearchBuilder MatchExtension(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _extension = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
    }

    public SearchBuilder MatchName(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _name = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
    }

    public SearchBuilder MatchNamePart(FileNamePart namePart)
    {
        _namePart = namePart;

        return this;
    }

    public SearchBuilder MatchContent(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _content = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
}

    public SearchBuilder IncludeAttributes(FileAttributes attributes)
    {
        _attributes = attributes;

        return this;
    }

    public SearchBuilder ExcludeAttributes(FileAttributes attributes)
    {
        _attributesToSkip = attributes;

        return this;
    }

    public SearchBuilder IncludeEmptyFiles()
    {
        _fileEmptyOption = FileEmptyOption.Empty;

        return this;
    }

    public SearchBuilder ExcludeEmptyFiles()
    {
        _fileEmptyOption = FileEmptyOption.NonEmpty;

        return this;
    }

    public SearchBuilder MatchFile(Func<FileInfo, bool> predicate)
    {
        _filePredicate = predicate;

        return this;
    }

    public SearchBuilder MatchDirectory(Func<DirectoryInfo, bool> predicate)
    {
        _directoryPredicate = predicate;

        return this;
    }

    public SearchBuilder TopDirectoryOnly()
    {
        _topDirectoryOnly = true;

        return this;
    }

    public SearchBuilder ExcludeDirectories(Func<string, bool> predicate)
    {
        _excludeDirectory = predicate;

        return this;
    }

    public SearchBuilder WithSearchProgress(IProgress<SearchProgress> progress)
    {
        _searchProgress = progress;

        return this;
    }

    public SearchBuilder TargetDirectories()
    {
        _searchTarget = SearchTarget.Directories;

        return this;
    }

    public SearchBuilder TargetAll()
    {
        _searchTarget = SearchTarget.All;

        return this;
    }

    public SearchBuilder WithDefaultEncoding(Encoding encoding)
    {
        _defaultEncoding = encoding;

        return this;
    }

    public Search Build()
    {
        var matcher = new FileMatcher()
        {
            Name = _name,
            NamePart = _namePart,
            Extension = _extension,
            Content = _content,
            Attributes = _attributes,
            AttributesToSkip = _attributesToSkip,
            FileEmptyOption = _fileEmptyOption,
            FilePredicate = _filePredicate,
            DirectoryPredicate = _directoryPredicate,
        };

        var options = new SearchOptions()
        {
            IncludeDirectory = null,
            ExcludeDirectory = _excludeDirectory,
            SearchProgress = _searchProgress,
            SearchTarget = _searchTarget,
            TopDirectoryOnly = _topDirectoryOnly,
            DefaultEncoding = _defaultEncoding,
        };

        return new Search(matcher, options);
    }

    public RenameBuilder Rename(string directoryPath)
    {
        return new RenameBuilder(directoryPath);
    }
}
