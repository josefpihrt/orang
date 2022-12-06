// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class SearchBuilder
{
    private Matcher? _name;
    private Matcher? _content;
    private FileNamePart _namePart;
    private static Matcher? _extension;
    private FileAttributes _attributesToSkip;
    private FileAttributes _attributes;
    private Func<FileInfo, bool>? _filePredicate;
    private Func<DirectoryInfo, bool>? _directoryPredicate;
    private FileEmptyOption _fileEmptyOption;
    private bool _topDirectoryOnly;
    private Func<string, bool>? _skipDirectory;
    private Action<SearchProgress>? _logProgress;
    private SearchTarget _searchTarget;
    private Encoding? _defaultEncoding;

    internal SearchBuilder()
    {
    }

    public static SearchBuilder Create()
    {
        return new SearchBuilder();
    }

    public SearchBuilder WithExtension(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _extension = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
    }

    public SearchBuilder WithExtension(Action<MatcherBuilder> configureMatcher)
    {
        if (configureMatcher is null)
            throw new ArgumentNullException(nameof(configureMatcher));

        var builder = new MatcherBuilder();

        configureMatcher?.Invoke(builder);

        _extension = builder.Build();

        return this;
    }

    public SearchBuilder WithExtensions(params string[] extensions)
    {
        _extension = GetExtensionMatcher(isNegative: false, extensions);

        return this;
    }

    public SearchBuilder WithoutExtensions(params string[] extensions)
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
                    PatternOptions.Equals | PatternOptions.Literal),
                (FileSystemHelpers.IsCaseSensitive) ? RegexOptions.None : RegexOptions.IgnoreCase,
                isNegative: isNegative);
        }
        else
        {
            return new Matcher(
                Pattern.FromValues(
                    extensions,
                    PatternOptions.Equals | PatternOptions.Literal),
                (FileSystemHelpers.IsCaseSensitive) ? RegexOptions.None : RegexOptions.IgnoreCase,
                isNegative: isNegative);
        }
    }

    public SearchBuilder WithName(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _name = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
    }

    public SearchBuilder WithName(Action<MatcherBuilder> configureMatcher)
    {
        if (configureMatcher is null)
            throw new ArgumentNullException(nameof(configureMatcher));

        var builder = new MatcherBuilder();

        configureMatcher(builder);

        _name = builder.Build();

        return this;
    }

    public SearchBuilder WithNamePart(FileNamePart namePart)
    {
        _namePart = namePart;

        return this;
    }

    public SearchBuilder WithContent(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _content = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
    }

    public SearchBuilder WithContent(Action<MatcherBuilder> configureMatcher)
    {
        var builder = new MatcherBuilder();

        configureMatcher(builder);

        _content = builder.Build();

        return this;
    }

    public SearchBuilder WithAttributes(FileAttributes attributes)
    {
        _attributes = attributes;

        return this;
    }

    public SearchBuilder WithoutAttributes(FileAttributes attributes)
    {
        _attributesToSkip = attributes;

        return this;
    }

    public SearchBuilder EmptyFilesOnly()
    {
        _fileEmptyOption = FileEmptyOption.Empty;

        return this;
    }

    public SearchBuilder NonEmptyFilesOnly()
    {
        _fileEmptyOption = FileEmptyOption.NonEmpty;

        return this;
    }

    public SearchBuilder WithFile(Func<FileInfo, bool> predicate)
    {
        _filePredicate = predicate;

        return this;
    }

    public SearchBuilder WithDirectory(Func<DirectoryInfo, bool> predicate)
    {
        _directoryPredicate = predicate;

        return this;
    }

    public SearchBuilder TopDirectoryOnly()
    {
        _topDirectoryOnly = true;

        return this;
    }

    public SearchBuilder SkipDirectory(Func<string, bool> predicate)
    {
        _skipDirectory = predicate;

        return this;
    }

    public SearchBuilder SkipDirectory(
        string pattern,
        RegexOptions options = RegexOptions.None,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        var matcher = new Matcher(pattern, options, isNegative: true, groupNumber, predicate);

        _skipDirectory = DirectoryPredicate.Create(matcher, FileNamePart.Name);

        return this;
    }

    public SearchBuilder SkipDirectory(Action<MatcherBuilder> configureMatcher)
    {
        if (configureMatcher is null)
            throw new ArgumentNullException(nameof(configureMatcher));

        var builder = new MatcherBuilder();

        //TODO: disable Negate
        configureMatcher(builder);

        builder.Negate();

        _skipDirectory = DirectoryPredicate.Create(builder.Build(), FileNamePart.Name);

        return this;
    }

    public SearchBuilder LogProgress(Action<SearchProgress> logProgress)
    {
        _logProgress = logProgress;

        return this;
    }

    public SearchBuilder DirectoriesOnly()
    {
        _searchTarget = SearchTarget.Directories;

        return this;
    }

    //TODO: rename
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
            ExcludeDirectory = _skipDirectory,
            LogProgress = _logProgress,
            SearchTarget = _searchTarget,
            TopDirectoryOnly = _topDirectoryOnly,
            DefaultEncoding = _defaultEncoding,
        };

        return new Search(matcher, options);
    }

    public DeleteOperation DeleteMatches(string directoryPath)
    {
        return new DeleteOperation(Build(), directoryPath);
    }

    public CopyOperation CopyMatches(string directoryPath, string destinationPath)
    {
        return new CopyOperation(Build(), directoryPath, destinationPath);
    }

    public MoveOperation MoveMatches(string directoryPath, string destinationPath)
    {
        return new MoveOperation(Build(), directoryPath, destinationPath);
    }

    public RenameOperation RenameMatches(string directoryPath, string replacement)
    {
        return new RenameOperation(Build(), directoryPath, replacement);
    }

    public RenameOperation RenameMatches(string directoryPath, MatchEvaluator matchEvaluator)
    {
        return new RenameOperation(Build(), directoryPath, matchEvaluator);
    }

    public ReplaceOperation ReplaceMatches(string directoryPath, string destinationPath)
    {
        return new ReplaceOperation(Build(), directoryPath, destinationPath);
    }

    public ReplaceOperation ReplaceMatches(string directoryPath, MatchEvaluator matchEvaluator)
    {
        return new ReplaceOperation(Build(), directoryPath, matchEvaluator);
    }
}
