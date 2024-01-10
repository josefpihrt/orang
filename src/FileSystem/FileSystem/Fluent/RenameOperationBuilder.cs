// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class RenameOperationBuilder
{
    private ReplaceFunctions _functions;
    private bool _cultureInvariant;
    private bool _dryRun;
    private Action<OperationProgress>? _logOperation;
    private IDialogProvider<ConflictInfo>? _dialogProvider;
    private ConflictResolution _conflictResolution = ConflictResolution.Skip;

    internal RenameOperationBuilder()
    {
    }

    internal RenameOperationBuilder WithFunctions(ReplaceFunctions functions)
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

    internal RenameOptions CreateOptions()
    {
        return new()
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
