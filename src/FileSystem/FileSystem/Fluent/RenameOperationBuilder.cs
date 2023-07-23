// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class RenameOperationBuilder
{
    private readonly Search _search;
    private readonly string? _replacement;
    private readonly MatchEvaluator? _matchEvaluator;
    private ReplaceFunctions _functions;
    private bool _cultureInvariant;
    private bool _dryRun;
    private Action<OperationProgress>? _logOperation;
    private IDialogProvider<ConflictInfo>? _dialogProvider;
    private ConflictResolution _conflictResolution = ConflictResolution.Skip;

    internal RenameOperationBuilder(Search search, string replacement)
    {
        if (search is null)
            throw new ArgumentNullException(nameof(search));

        if (replacement is null)
            throw new ArgumentNullException(nameof(replacement));

        _search = search;
        _replacement = replacement;
    }

    internal RenameOperationBuilder(Search search, MatchEvaluator matchEvaluator)
    {
        if (search is null)
            throw new ArgumentNullException(nameof(search));

        if (matchEvaluator is null)
            throw new ArgumentNullException(nameof(matchEvaluator));

        _search = search;
        _matchEvaluator = matchEvaluator;
    }

    public RenameOperationBuilder WithFunctions(ReplaceFunctions functions)
    {
        _functions = functions;

        return this;
    }

    public RenameOperationBuilder CultureInvariant()
    {
        _cultureInvariant = true;

        return this;
    }

    public RenameOperationBuilder DryRun()
    {
        _dryRun = true;

        return this;
    }

    public RenameOperationBuilder LogOperation(Action<OperationProgress> logOperation)
    {
        _logOperation = logOperation;

        return this;
    }

    public RenameOperationBuilder WithDialogProvider(IDialogProvider<ConflictInfo> dialogProvider)
    {
        _dialogProvider = dialogProvider;

        return this;
    }

    public RenameOperationBuilder WithConflictResolution(ConflictResolution conflictResolution)
    {
        _conflictResolution = conflictResolution;

        return this;
    }

    public IOperationResult Run(string directoryPath, CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        RenameOptions options = CreateOptions();

        return Run(directoryPath, options, cancellationToken);
    }

    public IOperationResult Run(IEnumerable<string> directoryPaths, CancellationToken cancellationToken = default)
    {
        if (directoryPaths is null)
            throw new ArgumentNullException(nameof(directoryPaths));

        RenameOptions options = CreateOptions();

        var telemetry = new SearchTelemetry();

        foreach (string directoryPath in directoryPaths)
        {
            IOperationResult result = Run(directoryPath, options, cancellationToken);
            telemetry.Add(result.Telemetry);
        }

        return new OperationResult(telemetry);
    }

    private IOperationResult Run(string directoryPath, RenameOptions options, CancellationToken cancellationToken)
    {
        return (_replacement is not null)
            ? _search.Rename(directoryPath, _replacement, options, cancellationToken)
            : _search.Rename(directoryPath, _matchEvaluator!, options, cancellationToken);
    }

    private RenameOptions CreateOptions()
    {
        return new RenameOptions()
        {
            ReplaceFunctions = _functions,
            CultureInvariant = _cultureInvariant,
            DryRun = _dryRun,
            LogOperation = _logOperation,
            DialogProvider = _dialogProvider,
            ConflictResolution = _conflictResolution,
        };
    }
}
