// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;

namespace Orang.FileSystem.Fluent;

public class DeleteOperation
{
    private readonly Search _search;
    private readonly string _directoryPath;
    private bool _contentOnly;
    private bool _directoriesOnly;
    private bool _filesOnly;
    private bool _includeBom;
    private bool _dryRun;
    private Action<OperationProgress>? _logOperation;

    internal DeleteOperation(Search search, string directoryPath)
    {
        _search = search;
        _directoryPath = directoryPath;
    }

    public DeleteOperation ContentOnly()
    {
        _contentOnly = true;

        return this;
    }

    public DeleteOperation IncludeBom()
    {
        _includeBom = true;

        return this;
    }

    public DeleteOperation FileOnly()
    {
        _filesOnly = true;

        return this;
    }

    public DeleteOperation DirectoriesOnly()
    {
        _directoriesOnly = true;

        return this;
    }

    public DeleteOperation DryRun()
    {
        _dryRun = true;

        return this;
    }

    public DeleteOperation LogOperation(Action<OperationProgress> logOperation)
    {
        _logOperation = logOperation;

        return this;
    }

    //TODO: rename Execute to Run
    public IOperationResult Execute(CancellationToken cancellationToken = default)
    {
        var options = new DeleteOptions()
        {
            ContentOnly = _contentOnly,
            IncludingBom = _includeBom,
            FilesOnly = _filesOnly,
            DirectoriesOnly = _directoriesOnly,
            DryRun = _dryRun,
            LogOperation = _logOperation
        };

        return _search.Delete(_directoryPath, options, cancellationToken);
    }
}
