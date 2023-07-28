// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class ReplaceOperationBuilder
{
    private ReplaceFunctions _functions;
    private bool _cultureInvariant;
    private bool _dryRun;
    private Action<OperationProgress>? _logOperation;

    internal ReplaceOperationBuilder()
    {
    }

    internal ReplaceOperationBuilder WithFunctions(ReplaceFunctions functions)
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

    internal ReplaceOptions CreateOptions()
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
