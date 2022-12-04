// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class CopyOptions
{
    public ConflictResolution ConflictResolution { get; set; } = ConflictResolution.Skip;

    public FileCompareOptions CompareOptions { get; set; }

    public FileAttributes NoCompareAttributes { get; set; }

    public TimeSpan? AllowedTimeDiff { get; set; }

    public bool Flat { get; set; }

    internal bool StructureOnly { get; set; }

    public bool DryRun { get; set; }

    public IProgress<OperationProgress>? OperationProgress { get; set; }

    public IDialogProvider<ConflictInfo>? DialogProvider { get; set; }
}
