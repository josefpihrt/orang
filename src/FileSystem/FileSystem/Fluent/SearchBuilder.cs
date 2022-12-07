// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class SearchBuilder
{
    private Matcher? _fileName;
    private Matcher? _fileContent;
    private FileNamePart? _fileNamePart;
    private static Matcher? _fileExtension;
    private FileAttributes? _fileAttributes;
    private FileAttributes? _fileAttributesToSkip;
    private FileEmptyOption? _fileEmptyOption;
    private Func<FileInfo, bool>? _filePredicate;

    private Matcher? _directoryName;
    private FileNamePart? _directoryNamePart;
    private FileAttributes? _directoryAttributes;
    private FileAttributes? _directoryAttributesToSkip;
    private FileEmptyOption? _directoryEmptyOption;
    private Func<DirectoryInfo, bool>? _directoryPredicate;

    private bool _topDirectoryOnly;
    private DirectoryMatcher? _searchDirectory;
    private Action<SearchProgress>? _logProgress;
    private Encoding? _defaultEncoding;

    internal SearchBuilder()
    {
    }

    public static SearchBuilder Create()
    {
        return new SearchBuilder();
    }

    public SearchBuilder MatchExtension(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _fileExtension = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
    }

    public SearchBuilder WithExtensions(params string[] extensions)
    {
        _fileExtension = GetExtensionMatcher(isNegative: false, extensions);

        return this;
    }

    public SearchBuilder WithoutExtensions(params string[] extensions)
    {
        _fileExtension = GetExtensionMatcher(isNegative: true, extensions);

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

    public SearchBuilder MatchFileName(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _fileName = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
    }

    public SearchBuilder MatchFileNamePart(FileNamePart namePart)
    {
        _fileNamePart = namePart;

        return this;
    }

    public SearchBuilder MatchFileContent(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _fileContent = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
    }

    public SearchBuilder WithFileAttributes(FileAttributes attributes)
    {
        _fileAttributes = attributes;

        return this;
    }

    public SearchBuilder WithoutFileAttributes(FileAttributes attributes)
    {
        _fileAttributesToSkip = attributes;

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

    public SearchBuilder MatchFile(Func<FileInfo, bool> predicate)
    {
        _filePredicate = predicate;

        return this;
    }

    public SearchBuilder MatchDirectoryName(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _directoryName = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
    }

    public SearchBuilder MatchDirectoryNamePart(FileNamePart namePart)
    {
        _directoryNamePart = namePart;

        return this;
    }

    public SearchBuilder WithDirectoryAttributes(FileAttributes attributes)
    {
        _directoryAttributes = attributes;

        return this;
    }

    public SearchBuilder WithoutDirectoryAttributes(FileAttributes attributes)
    {
        _directoryAttributesToSkip = attributes;

        return this;
    }

    public SearchBuilder EmptyDirectoryOnly()
    {
        _directoryEmptyOption = FileEmptyOption.Empty;

        return this;
    }

    public SearchBuilder NonEmptyDirectoryOnly()
    {
        _directoryEmptyOption = FileEmptyOption.NonEmpty;

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

    public SearchBuilder SkipDirectory(
        string pattern,
        RegexOptions options = RegexOptions.None,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _searchDirectory = new DirectoryMatcher()
        {
            Name = new Matcher(pattern, options, isNegative: true, groupNumber, predicate)
        };

        return this;
    }

    public SearchBuilder SearchDirectory(Action<DirectoryMatcherBuilder> configureMatcher)
    {
        var builder = new DirectoryMatcherBuilder();

        configureMatcher(builder);

        _searchDirectory = builder.Build();

        return this;
    }

    public SearchBuilder LogProgress(Action<SearchProgress> logProgress)
    {
        _logProgress = logProgress;

        return this;
    }

    //TODO: rename
    public SearchBuilder UseDefaultEncoding(Encoding encoding)
    {
        _defaultEncoding = encoding;

        return this;
    }

    public Search Build()
    {
        FileMatcher? fileMatcher = null;

        if (_fileName is not null
            || _fileNamePart is not null
            || _fileExtension is not null
            || _fileContent is not null
            || _fileAttributes is not null
            || _fileAttributesToSkip is not null
            || _fileEmptyOption is not null
            || _filePredicate is not null
            )
        {
            fileMatcher = new FileMatcher()
            {
                Name = _fileName,
                NamePart = _fileNamePart ?? FileNamePart.Name,
                Extension = _fileExtension,
                Content = _fileContent,
                Attributes = _fileAttributes ?? 0,
                AttributesToSkip = _fileAttributesToSkip ?? 0,
                EmptyOption = _fileEmptyOption ?? FileEmptyOption.None,
                Predicate = _filePredicate,
            };
        }

        DirectoryMatcher? directoryMatcher = null;

        if (_directoryName is not null
            || _directoryNamePart is not null
            || _directoryAttributes is not null
            || _directoryAttributesToSkip is not null
            || _directoryEmptyOption is not null
            || _directoryPredicate is not null
            )
        {
            directoryMatcher = new DirectoryMatcher()
            {
                Name = _directoryName,
                NamePart = _directoryNamePart ?? FileNamePart.Name,
                Attributes = _directoryAttributes ?? 0,
                AttributesToSkip = _directoryAttributesToSkip ?? 0,
                EmptyOption = _directoryEmptyOption ?? FileEmptyOption.None,
                Predicate = _directoryPredicate,
            };
        }

        var options = new SearchOptions()
        {
            SearchDirectory = _searchDirectory,
            LogProgress = _logProgress,
            TopDirectoryOnly = _topDirectoryOnly,
            DefaultEncoding = _defaultEncoding,
        };

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
            //TODO: message
            throw new InvalidOperationException();
        }
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
