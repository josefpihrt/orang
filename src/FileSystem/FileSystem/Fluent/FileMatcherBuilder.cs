// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text.RegularExpressions;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class FileMatcherBuilder
{
    private Matcher? _name;
    private FileNamePart? _namePart;
    private Matcher? _content;
    private static Matcher? _extension;
    private FileAttributes? _attributes;
    private FileAttributes? _attributesToSkip;
    private FileEmptyOption? _emptyOption;
    private Func<FileInfo, bool>? _predicate;

    internal FileMatcherBuilder()
    {
    }

    public FileMatcherBuilder Extension(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _extension = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
    }

    public FileMatcherBuilder WithExtensions(params string[] extensions)
    {
        _extension = GetExtensionMatcher(extensions, isNegative: false);

        return this;
    }

    public FileMatcherBuilder WithoutExtensions(params string[] extensions)
    {
        _extension = GetExtensionMatcher(extensions, isNegative: true);

        return this;
    }

    private static Matcher? GetExtensionMatcher(string[] extensions, bool isNegative)
    {
        if (extensions.Length == 0)
        {
            return null;
        }
        else if (extensions.Length == 1)
        {
            return new Matcher(
                Pattern.From(
                    extensions[0],
                    PatternOptions.Equals | PatternOptions.Literal),
                (FileSystemHelpers.IsCaseSensitive) ? RegexOptions.None : RegexOptions.IgnoreCase,
                isNegative: isNegative);
        }
        else
        {
            return new Matcher(
                Pattern.From(
                    extensions,
                    PatternOptions.Equals | PatternOptions.Literal),
                (FileSystemHelpers.IsCaseSensitive) ? RegexOptions.None : RegexOptions.IgnoreCase,
                isNegative: isNegative);
        }
    }

    public FileMatcherBuilder Name(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _name = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
    }

    public FileMatcherBuilder NamePart(FileNamePart namePart)
    {
        _namePart = namePart;

        return this;
    }

    public FileMatcherBuilder Content(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool isNegative = false,
        int groupNumber = -1,
        Func<string, bool>? predicate = null)
    {
        _content = new Matcher(pattern, options, isNegative, groupNumber, predicate);

        return this;
    }

    public FileMatcherBuilder WithAttributes(FileAttributes attributes)
    {
        _attributes = attributes;

        return this;
    }

    public FileMatcherBuilder WithoutAttributes(FileAttributes attributes)
    {
        _attributesToSkip = attributes;

        return this;
    }

    public FileMatcherBuilder EmptyOnly()
    {
        _emptyOption = FileEmptyOption.Empty;

        return this;
    }

    public FileMatcherBuilder NonEmptyOnly()
    {
        _emptyOption = FileEmptyOption.NonEmpty;

        return this;
    }

    //TODO: rename
    public FileMatcherBuilder Match(Func<FileInfo, bool> predicate)
    {
        _predicate = predicate;

        return this;
    }

    internal FileMatcher Build()
    {
        return new FileMatcher()
        {
            Name = _name,
            NamePart = _namePart ?? FileNamePart.Name,
            Extension = _extension,
            Content = _content,
            Attributes = _attributes ?? 0,
            AttributesToSkip = _attributesToSkip ?? 0,
            EmptyOption = _emptyOption ?? FileEmptyOption.None,
            Predicate = _predicate,
        };
    }
}
