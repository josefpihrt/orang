// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class DeleteOperationBuilder
{
    private bool _contentOnly;
    private bool _includingBom;
    private bool _dryRun;
    private Action<OperationProgress>? _logOperation;

    internal DeleteOperationBuilder()
    {
    }

    public DeleteOperationBuilder ContentOnly()
    {
        _contentOnly = true;
        return this;
    }

    public DeleteOperationBuilder IncludingBom()
    {
        _includingBom = true;
        return this;
    }

    public DeleteOperationBuilder DryRun()
    {
        _dryRun = true;
        return this;
    }

    public DeleteOperationBuilder LogOperation(Action<OperationProgress> logOperation)
    {
        _logOperation = logOperation;
        return this;
    }

    internal DeleteOptions CreateOptions()
    {
        return new DeleteOptions()
        {
            ContentOnly = _contentOnly,
            IncludingBom = _includingBom,
            DryRun = _dryRun,
            LogOperation = _logOperation
        };
    }
}
