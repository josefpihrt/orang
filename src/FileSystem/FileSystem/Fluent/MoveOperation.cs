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
    private FileCompareProperties _comparedProperties;
    private FileAttributes? _comparedAttributes;
    private TimeSpan? _allowedTimeDiff;
    private IDialogProvider<ConflictInfo>? _dialogProvider;
    private Func<string, string, bool>? _compareFile;
    private Func<string, string, bool>? _compareDirectory;

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

    public MoveOperation CompareSize()
    {
        _comparedProperties |= FileCompareProperties.Size;

        return this;
    }

    public MoveOperation CompareModifiedTime(TimeSpan? allowedTimeDiff = null)
    {
        _comparedProperties |= FileCompareProperties.ModifiedTime;
        _allowedTimeDiff = allowedTimeDiff;

        return this;
    }

    public MoveOperation CompareAttributes(FileAttributes? attributes = null)
    {
        _comparedProperties |= FileCompareProperties.Attributes;
        _comparedAttributes = attributes;

        return this;
    }

    public MoveOperation CompareContent()
    {
        _comparedProperties |= FileCompareProperties.Content;

        return this;
    }

    public MoveOperation CompareFile(Func<string, string, bool> comparer)
    {
        _compareFile = comparer;

        return this;
    }

    public MoveOperation CompareDirectory(Func<string, string, bool> comparer)
    {
        _compareDirectory = comparer;

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
            ComparedProperties = _comparedProperties,
            ComparedAttributes = _comparedAttributes,
            AllowedTimeDiff = _allowedTimeDiff,
            Flat = _flat,
            DryRun = _dryRun,
            LogOperation = _logOperation,
            DialogProvider = _dialogProvider,
            CompareFile = _compareFile,
            CompareDirectory = _compareDirectory,
        };

        return _search.Move(_directoryPath, _destinationPath, options, cancellationToken);
    }
}
