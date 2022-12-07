// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class MoveOperation
{
    private readonly Search _search;
    private readonly string _directoryPath;
    private readonly string _destinationPath;
    private ConflictResolution _conflictResolution = ConflictResolution.Skip;
    private bool _dryRun;
    private bool _flat;
    private Action<OperationProgress>? _logOperation;
    private FileCompareOptions _fileCompareOptions;
    private FileAttributes _noCompareAttributes;
    private TimeSpan? _allowedTimeDiff;
    private IDialogProvider<ConflictInfo>? _dialogProvider;

    internal MoveOperation(Search search, string directoryPath, string destinationPath)
    {
        if (search is null)
            throw new ArgumentNullException(nameof(search));

        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (destinationPath is null)
            throw new ArgumentNullException(nameof(destinationPath));

        _search = search;
        _directoryPath = directoryPath;
        _destinationPath = destinationPath;
    }

    public MoveOperation WithConflictResolution(ConflictResolution conflictResolution)
    {
        _conflictResolution = conflictResolution;

        return this;
    }

    public MoveOperation WithFileCompareOptions(FileCompareOptions options)
    {
        _fileCompareOptions = options;

        return this;
    }

    //TODO: rename
    public MoveOperation SkipCompareAttributes(FileAttributes fileAttributes)
    {
        _noCompareAttributes = fileAttributes;

        return this;
    }

    public MoveOperation WithAllowedTimeDiff(TimeSpan diff)
    {
        _allowedTimeDiff = diff;

        return this;
    }

    public MoveOperation Flat()
    {
        _flat = true;

        return this;
    }

    public MoveOperation DryRun()
    {
        _dryRun = true;

        return this;
    }

    public MoveOperation LogOperation(Action<OperationProgress> logOperation)
    {
        _logOperation = logOperation;

        return this;
    }

    public MoveOperation WithDialogProvider(IDialogProvider<ConflictInfo> dialogProvider)
    {
        _dialogProvider = dialogProvider;

        return this;
    }

    public IOperationResult Execute(CancellationToken cancellationToken = default)
    {
        var options = new CopyOptions()
        {
            ConflictResolution = _conflictResolution,
            CompareOptions = _fileCompareOptions,
            NoCompareAttributes = _noCompareAttributes,
            AllowedTimeDiff = _allowedTimeDiff,
            Flat = _flat,
            DryRun = _dryRun,
            LogOperation = _logOperation,
            DialogProvider = _dialogProvider,
        };

        return _search.Move(_directoryPath, _destinationPath, options, cancellationToken);
    }
}
