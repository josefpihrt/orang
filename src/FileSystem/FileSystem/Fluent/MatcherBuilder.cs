// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class MatcherBuilder
{
    private string? _pattern;
    private int _groupName;
    private Func<string, bool>? _predicate;
    private bool _negate;
    private RegexOptions _options;

    internal MatcherBuilder()
    {
    }

    public MatcherBuilder WithPattern(string value, PatternOptions options = PatternOptions.None)
    {
        return WithPattern(options, value);
    }

    public MatcherBuilder WithPattern(string value1, string value2, PatternOptions options = PatternOptions.None)
    {
        return WithPattern(options, value1, value2);
    }

    public MatcherBuilder WithPattern(
        string value1,
        string value2,
        string value3,
        PatternOptions options = PatternOptions.None)
    {
        return WithPattern(options, value1, value2, value3);
    }

    private MatcherBuilder WithPattern(PatternOptions options, params string[] values)
    {
        _pattern = Pattern.FromValues(values, options);

        return this;
    }

    public MatcherBuilder WithOptions(RegexOptions options)
    {
        _options = options;

        return this;
    }

    public MatcherBuilder Negate()
    {
        _negate = true;

        return this;
    }

    public MatcherBuilder WithGroupName(int groupName)
    {
        _groupName = groupName;

        return this;
    }

    public MatcherBuilder WithPredicate(Func<string, bool> predicate)
    {
        _predicate = predicate;

        return this;
    }

    internal Matcher Build()
    {
        //TODO: ?
        if (_pattern is null)
            throw new InvalidOperationException();

        return new Matcher(
            _pattern,
            _options,
            _negate,
            _groupName,
            _predicate);
    }
}
