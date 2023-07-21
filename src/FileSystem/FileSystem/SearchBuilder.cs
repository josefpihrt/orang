// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class SearchBuilder
{
    private FileMatcher? _file;
    private DirectoryMatcher? _directory;
    private DirectoryMatcher? _searchDirectory;

    private bool _topDirectoryOnly;
    private Action<SearchProgress>? _logProgress;
    private Encoding? _defaultEncoding;

    internal SearchBuilder()
    {
    }

    public static SearchBuilder Create()
    {
        return new SearchBuilder();
    }

    public SearchBuilder MatchFile(Action<FileMatcherBuilder> configureMatcher)
    {
        var builder = new FileMatcherBuilder();

        configureMatcher(builder);

        _file = builder.Build();

        return this;
    }

    public SearchBuilder MatchDirectory(Action<DirectoryMatcherBuilder> configureMatcher)
    {
        var builder = new DirectoryMatcherBuilder();

        configureMatcher(builder);

        _directory = builder.Build();

        return this;
    }

    public SearchBuilder SearchDirectory(Action<DirectoryMatcherBuilder> configureMatcher)
    {
        var builder = new DirectoryMatcherBuilder();

        configureMatcher(builder);

        _searchDirectory = builder.Build();

        return this;
    }

    public SearchBuilder SkipDirectory(
        string pattern,
        RegexOptions options = RegexOptions.None,
        object? group = null,
        Func<string, bool>? predicate = null)
    {
        _searchDirectory = new DirectoryMatcher()
        {
            Name = new Matcher(pattern, options, invert: true, group, predicate)
        };

        return this;
    }

    public SearchBuilder FileExtension(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool invert = false,
        object? group = null,
        Func<string, bool>? predicate = null)
    {
        if (_file is null)
            _file = new FileMatcher();

        _file.Extension = new Matcher(pattern, options, invert, group, predicate);

        return this;
    }

    public SearchBuilder FileName(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool invert = false,
        object? group = null,
        Func<string, bool>? predicate = null)
    {
        if (_file is null)
            _file = new FileMatcher();

        _file.Name = new Matcher(pattern, options, invert, group, predicate);

        return this;
    }

    public SearchBuilder FileContent(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool invert = false,
        object? group = null,
        Func<string, bool>? predicate = null)
    {
        if (_file is null)
            _file = new FileMatcher();

        _file.Name = new Matcher(pattern, options, invert, group, predicate);

        return this;
    }

    public SearchBuilder DirectoryName(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool invert = false,
        object? group = null,
        Func<string, bool>? predicate = null)
    {
        if (_directory is null)
            _directory = new DirectoryMatcher();

        _directory.Name = new Matcher(pattern, options, invert, group, predicate);

        return this;
    }

    public SearchBuilder TopDirectoryOnly()
    {
        _topDirectoryOnly = true;

        return this;
    }

    public SearchBuilder LogProgress(Action<SearchProgress> logProgress)
    {
        _logProgress = logProgress;

        return this;
    }

    public SearchBuilder WithDefaultEncoding(Encoding encoding)
    {
        _defaultEncoding = encoding;

        return this;
    }

    public Search Build()
    {
        var options = new SearchOptions()
        {
            SearchDirectory = _searchDirectory,
            LogProgress = _logProgress,
            TopDirectoryOnly = _topDirectoryOnly,
            DefaultEncoding = _defaultEncoding,
        };

        FileMatcher? fileMatcher = _file;
        DirectoryMatcher? directoryMatcher = _directory;

        if (fileMatcher is not null)
        {
            return (directoryMatcher is not null)
                ? new Search(fileMatcher, directoryMatcher, options)
                : new Search(fileMatcher, options);
        }
        else if (directoryMatcher is not null)
        {
            return new Search(directoryMatcher, options);
        }
        else
        {
            throw new InvalidOperationException("File matcher or directory matcher must be specified.");
        }
    }
}
