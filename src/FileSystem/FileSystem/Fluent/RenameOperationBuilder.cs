// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class RenameOperationBuilder
{
    private readonly Search _search;
    private readonly string _directoryPath;
    private readonly string? _replacement;
    private readonly MatchEvaluator? _matchEvaluator;
    private ReplaceFunctions _functions;
    private bool _cultureInvariant;
    private bool _dryRun;
    private Action<OperationProgress>? _logOperation;
    private IDialogProvider<ConflictInfo>? _dialogProvider;
    private ConflictResolution _conflictResolution = ConflictResolution.Skip;

    internal RenameOperationBuilder(Search search, string directoryPath, string replacement)
    {
        if (search is null)
            throw new ArgumentNullException(nameof(search));

        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (replacement is null)
            throw new ArgumentNullException(nameof(replacement));

        _search = search;
        _directoryPath = directoryPath;
        _replacement = replacement;
    }

    internal RenameOperationBuilder(Search search, string directoryPath, MatchEvaluator matchEvaluator)
    {
        if (search is null)
            throw new ArgumentNullException(nameof(search));

        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (matchEvaluator is null)
            throw new ArgumentNullException(nameof(matchEvaluator));

        _search = search;
        _directoryPath = directoryPath;
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

    public IOperationResult Run(CancellationToken cancellationToken = default)
    {
        var options = new RenameOptions()
        {
            ReplaceFunctions = _functions,
            CultureInvariant = _cultureInvariant,
            DryRun = _dryRun,
            LogOperation = _logOperation,
            DialogProvider = _dialogProvider,
            ConflictResolution = _conflictResolution,
        };

        if (_replacement is not null)
        {
            return _search.Rename(_directoryPath, _replacement, options, cancellationToken);
        }
        else
        {
            return _search.Rename(_directoryPath, _matchEvaluator!, options, cancellationToken);
        }
    }
}
