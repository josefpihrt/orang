// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class ReplaceOperation
{
    private readonly Search _search;
    private readonly string _directoryPath;
    private readonly string? _replacement;
    private readonly MatchEvaluator? _matchEvaluator;
    private ReplaceFunctions _functions;
    private bool _cultureInvariant;
    private bool _dryRun;
    private Action<OperationProgress>? _logOperation;

    internal ReplaceOperation(Search search, string directoryPath, string replacement)
    {
        _search = search;
        _directoryPath = directoryPath;
        _replacement = replacement;
    }

    internal ReplaceOperation(Search search, string directoryPath, MatchEvaluator matchEvaluator)
    {
        _search = search;
        _directoryPath = directoryPath;
        _matchEvaluator = matchEvaluator;
    }

    public ReplaceOperation WithFunctions(ReplaceFunctions functions)
    {
        _functions = functions;

        return this;
    }

    public ReplaceOperation CultureInvariant()
    {
        _cultureInvariant = true;

        return this;
    }

    public ReplaceOperation DryRun()
    {
        _dryRun = true;

        return this;
    }

    public ReplaceOperation LogOperation(Action<OperationProgress> logOperation)
    {
        _logOperation = logOperation;

        return this;
    }

    public IOperationResult Execute(CancellationToken cancellationToken = default)
    {
        var options = new ReplaceOptions()
        {
            ReplaceFunctions = _functions,
            CultureInvariant = _cultureInvariant,
            DryRun = _dryRun,
            LogOperation = _logOperation,
        };

        if (_replacement is not null)
        {
            return _search.Replace(_directoryPath, _replacement, options, cancellationToken);
        }
        else
        {
            return _search.Replace(_directoryPath, _matchEvaluator!, options, cancellationToken);
        }
    }
}
