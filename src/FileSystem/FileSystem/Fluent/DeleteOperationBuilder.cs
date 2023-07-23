// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class DeleteOperationBuilder
{
    private readonly Search _search;

    private bool _contentOnly;
    private bool _includingBom;
    private bool _dryRun;
    private Action<OperationProgress>? _logOperation;

    internal DeleteOperationBuilder(Search search)
    {
        _search = search ?? throw new ArgumentNullException(nameof(search));
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

    public IOperationResult Run(string directoryPath, CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        return _search.Delete(directoryPath, CreateOptions(), cancellationToken);
    }

    public IOperationResult Run(IEnumerable<string> directoryPaths, CancellationToken cancellationToken = default)
    {
        if (directoryPaths is null)
            throw new ArgumentNullException(nameof(directoryPaths));

        DeleteOptions options = CreateOptions();

        var telemetry = new SearchTelemetry();

        foreach (string directoryPath in directoryPaths)
        {
            IOperationResult result = _search.Delete(directoryPath, options, cancellationToken);
            telemetry.Add(result.Telemetry);
        }

        return new OperationResult(telemetry);
    }

    private DeleteOptions CreateOptions()
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
