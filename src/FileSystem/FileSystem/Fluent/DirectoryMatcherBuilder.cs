// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text.RegularExpressions;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class DirectoryMatcherBuilder
{
    private Matcher? _name;
    private FileNamePart? _namePart;
    private FileAttributes? _attributes;
    private FileAttributes? _attributesToSkip;
    private FileEmptyOption? _emptyOption;
    private Func<DirectoryInfo, bool>? _matchDirectoryInfo;

    internal DirectoryMatcherBuilder()
    {
    }

    public DirectoryMatcherBuilder Name(
        string pattern,
        RegexOptions options = RegexOptions.None,
        bool invert = false,
        object? group = null,
        Func<string, bool>? predicate = null)
    {
        _name = new Matcher(pattern, options, invert, group, predicate);

        return this;
    }

    public DirectoryMatcherBuilder NamePart(FileNamePart namePart)
    {
        _namePart = namePart;

        return this;
    }

    public DirectoryMatcherBuilder WithAttributes(FileAttributes attributes)
    {
        _attributes = attributes;

        return this;
    }

    public DirectoryMatcherBuilder WithoutAttributes(FileAttributes attributes)
    {
        _attributesToSkip = attributes;

        return this;
    }

    //TODO: EmptyOnly > Empty
    public DirectoryMatcherBuilder EmptyOnly()
    {
        _emptyOption = FileEmptyOption.Empty;

        return this;
    }

    public DirectoryMatcherBuilder NonEmptyOnly()
    {
        _emptyOption = FileEmptyOption.NonEmpty;

        return this;
    }

    //TODO: Match > Predicate, DirectoryInfoMatch, DirectoryInfoPredicate
    public DirectoryMatcherBuilder Match(Func<DirectoryInfo, bool> predicate)
    {
        _matchDirectoryInfo = predicate;

        return this;
    }

    internal DirectoryMatcher Build()
    {
        return new DirectoryMatcher()
        {
            Name = _name,
            NamePart = _namePart ?? FileNamePart.Name,
            WithAttributes = _attributes ?? 0,
            WithoutAttributes = _attributesToSkip ?? 0,
            EmptyOption = _emptyOption ?? FileEmptyOption.None,
            MatchDirectoryInfo = _matchDirectoryInfo,
        };
    }
}
