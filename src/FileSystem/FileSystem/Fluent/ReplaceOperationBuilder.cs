// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class ReplaceOperationBuilder
{
    private readonly Search _search;
    private readonly string? _replacement;
    private readonly MatchEvaluator? _matchEvaluator;
    private ReplaceFunctions _functions;
    private bool _cultureInvariant;
    private bool _dryRun;
    private Action<OperationProgress>? _logOperation;

    internal ReplaceOperationBuilder(Search search, string replacement)
    {
        if (search is null)
            throw new ArgumentNullException(nameof(search));

        if (replacement is null)
            throw new ArgumentNullException(nameof(replacement));

        _search = search;
        _replacement = replacement;
    }

    internal ReplaceOperationBuilder(Search search, MatchEvaluator matchEvaluator)
    {
        if (search is null)
            throw new ArgumentNullException(nameof(search));

        if (matchEvaluator is null)
            throw new ArgumentNullException(nameof(matchEvaluator));

        _search = search;
        _matchEvaluator = matchEvaluator;
    }

    public ReplaceOperationBuilder WithFunctions(ReplaceFunctions functions)
    {
        _functions = functions;

        return this;
    }

    public ReplaceOperationBuilder CultureInvariant()
    {
        _cultureInvariant = true;

        return this;
    }

    public ReplaceOperationBuilder DryRun()
    {
        _dryRun = true;

        return this;
    }

    public ReplaceOperationBuilder LogOperation(Action<OperationProgress> logOperation)
    {
        _logOperation = logOperation;

        return this;
    }

    public IOperationResult Run(string directoryPath, CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        ReplaceOptions options = CreateOptions();

        return Run(directoryPath, options, cancellationToken);
    }

    public IOperationResult Run(IEnumerable<string> directoryPaths, CancellationToken cancellationToken = default)
    {
        if (directoryPaths is null)
            throw new ArgumentNullException(nameof(directoryPaths));

        ReplaceOptions options = CreateOptions();

        var telemetry = new SearchTelemetry();

        foreach (string directoryPath in directoryPaths)
        {
            IOperationResult result = Run(directoryPath, options, cancellationToken);
            telemetry.Add(result.Telemetry);
        }

        return new OperationResult(telemetry);
    }

    private IOperationResult Run(string directoryPath, ReplaceOptions options, CancellationToken cancellationToken)
    {
        return (_replacement is not null)
            ? _search.Replace(directoryPath, _replacement, options, cancellationToken)
            : _search.Replace(directoryPath, _matchEvaluator!, options, cancellationToken);
    }

    private ReplaceOptions CreateOptions()
    {
        return new ReplaceOptions()
        {
            ReplaceFunctions = _functions,
            CultureInvariant = _cultureInvariant,
            DryRun = _dryRun,
            LogOperation = _logOperation,
        };
    }
}
