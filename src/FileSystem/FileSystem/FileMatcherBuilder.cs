// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text.RegularExpressions;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class FileMatcherBuilder
{
    private Matcher? _name;
    private FileNamePart? _namePart;
    private Matcher? _content;
    private static Matcher? _extension;
    private FileAttributes? _withAttributes;
    private FileAttributes? _withoutAttributes;
    private FileEmptyOption? _emptyOption;
    private Func<FileInfo, bool>? _matchFileInfo;

    internal FileMatcherBuilder()
    {
    }

    public FileMatcherBuilder Extension(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool invert = false,
        object? group = null,
        Func<string, bool>? predicate = null)
    {
        _extension = new Matcher(pattern, options, invert, group, predicate);

        return this;
    }

    public FileMatcherBuilder WithExtensions(params string[] extensions)
    {
        _extension = GetExtensionMatcher(extensions, invert: false);

        return this;
    }

    public FileMatcherBuilder WithoutExtensions(params string[] extensions)
    {
        _extension = GetExtensionMatcher(extensions, invert: true);

        return this;
    }

    private static Matcher? GetExtensionMatcher(string[] extensions, bool invert)
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
                FileSystemUtilities.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase,
                invert: invert);
        }
        else
        {
            return new Matcher(
                Pattern.Any(
                    extensions,
                    PatternOptions.Equals | PatternOptions.Literal),
                FileSystemUtilities.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase,
                invert: invert);
        }
    }

    public FileMatcherBuilder Name(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool invert = false,
        object? group = null,
        Func<string, bool>? predicate = null)
    {
        _name = new Matcher(pattern, options, invert, group, predicate);

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
        bool invert = false,
        object? group = null,
        Func<string, bool>? predicate = null)
    {
        _content = new Matcher(pattern, options, invert, group, predicate);

        return this;
    }

    public FileMatcherBuilder WithAttributes(FileAttributes attributes)
    {
        _withAttributes = attributes;

        return this;
    }

    public FileMatcherBuilder WithoutAttributes(FileAttributes attributes)
    {
        _withoutAttributes = attributes;

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

    public FileMatcherBuilder Match(Func<FileInfo, bool> predicate)
    {
        _matchFileInfo = predicate;

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
            WithAttributes = _withAttributes ?? 0,
            WithoutAttributes = _withoutAttributes ?? 0,
            EmptyOption = _emptyOption ?? FileEmptyOption.None,
            MatchFileInfo = _matchFileInfo,
        };
    }
}
