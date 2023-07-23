// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class CopyOperationBuilder
{
    private readonly Search _search;
    private ConflictResolution _conflictResolution = ConflictResolution.Skip;
    private bool _dryRun;
    private Action<OperationProgress>? _logOperation;
    private FileCompareProperties _comparedProperties;
    private FileAttributes? _comparedAttributes;
    private TimeSpan? _allowedTimeDiff;
    private bool _flat;
    private IDialogProvider<ConflictInfo>? _dialogProvider;
    private Func<string, string, bool>? _compareFile;
    private Func<string, string, bool>? _compareDirectory;

    internal CopyOperationBuilder(Search search)
    {
        if (search is null)
            throw new ArgumentNullException(nameof(search));

        _search = search;
    }

    public CopyOperationBuilder WithConflictResolution(ConflictResolution conflictResolution)
    {
        _conflictResolution = conflictResolution;

        return this;
    }

    public CopyOperationBuilder CompareSize()
    {
        _comparedProperties |= FileCompareProperties.Size;

        return this;
    }

    public CopyOperationBuilder CompareModifiedTime(TimeSpan? allowedTimeDiff = null)
    {
        _comparedProperties |= FileCompareProperties.ModifiedTime;
        _allowedTimeDiff = allowedTimeDiff;

        return this;
    }

    public CopyOperationBuilder CompareAttributes(FileAttributes? attributes = null)
    {
        _comparedProperties |= FileCompareProperties.Attributes;
        _comparedAttributes = attributes;

        return this;
    }

    public CopyOperationBuilder CompareContent()
    {
        _comparedProperties |= FileCompareProperties.Content;

        return this;
    }

    public CopyOperationBuilder CompareFile(Func<string, string, bool> comparer)
    {
        _compareFile = comparer;

        return this;
    }

    public CopyOperationBuilder CompareDirectory(Func<string, string, bool> comparer)
    {
        _compareDirectory = comparer;

        return this;
    }

    public CopyOperationBuilder Flat()
    {
        _flat = true;

        return this;
    }

    public CopyOperationBuilder DryRun()
    {
        _dryRun = true;

        return this;
    }

    public CopyOperationBuilder LogOperation(Action<OperationProgress> logOperation)
    {
        _logOperation = logOperation;

        return this;
    }

    public CopyOperationBuilder WithDialogProvider(IDialogProvider<ConflictInfo> dialogProvider)
    {
        _dialogProvider = dialogProvider;

        return this;
    }

    public IOperationResult Run(string directoryPath, string destinationPath, CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (destinationPath is null)
            throw new ArgumentNullException(nameof(destinationPath));

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

        return _search.Copy(directoryPath, destinationPath, options, cancellationToken);
    }
}
