﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class CopyOperationBuilder
{
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

    internal CopyOperationBuilder()
    {
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

    internal CopyOperationBuilder CompareFile(Func<string, string, bool> comparer)
    {
        _compareFile = comparer;
        return this;
    }

    internal CopyOperationBuilder CompareDirectory(Func<string, string, bool> comparer)
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

    internal CopyOptions CreateOptions()
    {
        return new()
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
    }
}
