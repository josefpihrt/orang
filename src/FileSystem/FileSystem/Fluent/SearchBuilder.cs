﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class SearchBuilder
{
    private FileMatcher? _file;
    private DirectoryMatcher? _directory;
    private DirectoryMatcher? _searchDirectory;

    private bool _topDirectoryOnly;
    private Action<SearchProgress>? _logProgress;
    private Encoding? _defaultEncoding;

    private List<string>? _include;
    private List<string>? _exclude;

    public SearchBuilder MatchFile(Action<FileMatcherBuilder> configure)
    {
        var builder = new FileMatcherBuilder();

        configure(builder);

        _file = builder.Build();

        return this;
    }

    public SearchBuilder MatchDirectory(Action<DirectoryMatcherBuilder> configure)
    {
        var builder = new DirectoryMatcherBuilder();

        configure(builder);

        _directory = builder.Build();

        return this;
    }

    [Obsolete("This method is obsolete and will be removed in future versions. Use method Include/Exclude instead.")]
    public SearchBuilder SearchDirectory(Action<DirectoryMatcherBuilder> configure)
    {
        var builder = new DirectoryMatcherBuilder();

        configure(builder);

        _searchDirectory = builder.Build();

        return this;
    }

    [Obsolete("This method is obsolete and will be removed in future versions. Use method Include/Exclude instead.")]
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

    public SearchBuilder Include(string globPattern)
    {
        (_include ??= new List<string>()).Add(globPattern);

        return this;
    }

    public SearchBuilder Include(params string[] globPatterns)
    {
        (_include ??= new List<string>()).AddRange(globPatterns);

        return this;
    }

    public SearchBuilder Exclude(string globPattern)
    {
        (_exclude ??= new List<string>()).Add(globPattern);

        return this;
    }

    public SearchBuilder Exclude(params string[] globPatterns)
    {
        (_exclude ??= new List<string>()).AddRange(globPatterns);

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

    public Search ToSearch()
    {
        var options = new SearchOptions()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            SearchDirectory = _searchDirectory,
#pragma warning restore CS0618 // Type or member is obsolete
            LogProgress = _logProgress,
            TopDirectoryOnly = _topDirectoryOnly,
            DefaultEncoding = _defaultEncoding,
        };

        if (_include is not null)
            options.Include.AddRange(_include);

        if (_exclude is not null)
            options.Exclude.AddRange(_exclude);

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

    public IEnumerable<FileMatch> Matches(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        return ToSearch().Matches(directoryPath, cancellationToken);
    }

    public IEnumerable<FileMatch> Matches(
        IEnumerable<string> directoryPaths,
        CancellationToken cancellationToken = default)
    {
        if (directoryPaths is null)
            throw new ArgumentNullException(nameof(directoryPaths));

        return Matches();

        IEnumerable<FileMatch> Matches()
        {
            Search search = ToSearch();

            foreach (string directoryPath in directoryPaths)
            {
                foreach (FileMatch fileMatch in search.Matches(directoryPath, cancellationToken))
                    yield return fileMatch;
            }
        }
    }

    public DeleteOperation Delete(Action<DeleteOperationBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var deleteBuilder = new DeleteOperationBuilder();
        configure(deleteBuilder);

        return new DeleteOperation(ToSearch(), deleteBuilder.CreateOptions());
    }

    public CopyOperation Copy(Action<CopyOperationBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var copyBuilder = new CopyOperationBuilder();
        configure(copyBuilder);

        return new CopyOperation(ToSearch(), copyBuilder.CreateOptions());
    }

    public MoveOperation Move(Action<MoveOperationBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var moveBuilder = new MoveOperationBuilder();
        configure(moveBuilder);

        return new MoveOperation(ToSearch(), moveBuilder.CreateOptions());
    }

    public RenameOperation Rename(Action<RenameOperationBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var renameBuilder = new RenameOperationBuilder();
        configure(renameBuilder);

        return new RenameOperation(ToSearch(), renameBuilder.CreateOptions());
    }

    public ReplaceOperation Replace(Action<ReplaceOperationBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var replaceBuilder = new ReplaceOperationBuilder();
        configure(replaceBuilder);

        return new ReplaceOperation(ToSearch(), replaceBuilder.CreateOptions());
    }
}
