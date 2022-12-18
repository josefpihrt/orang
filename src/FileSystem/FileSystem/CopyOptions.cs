// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class CopyOptions
{
    public ConflictResolution ConflictResolution { get; set; } = ConflictResolution.Skip;

    public FileCompareProperties ComparedProperties { get; set; }

    public Func<string, string, bool>? CompareFile { get; set; }

    public Func<string, string, bool>? CompareDirectory { get; set; }

    public FileAttributes? ComparedAttributes { get; set; }

    public TimeSpan? AllowedTimeDiff { get; set; }

    public bool Flat { get; set; }

    internal bool StructureOnly { get; set; }

    public bool DryRun { get; set; }

    public Action<OperationProgress>? LogOperation { get; set; }

    public IDialogProvider<ConflictInfo>? DialogProvider { get; set; }
}
