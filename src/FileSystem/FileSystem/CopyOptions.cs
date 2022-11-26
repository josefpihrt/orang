// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;

namespace Orang.FileSystem;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class CopyOptions : SearchOptions
{
    public ConflictResolution ConflictResolution { get; set; } = ConflictResolution.Skip;

    public FileCompareOptions CompareOptions { get; set; }

    public FileAttributes NoCompareAttributes { get; set; }

    public TimeSpan? AllowedTimeDiff { get; set; }

    public bool Flat { get; set; }

    internal bool StructureOnly { get; set; }

    public bool DryRun { get; set; }

    public IProgress<OperationProgress>? Progress { get; set; }

    public IDialogProvider<ConflictInfo>? DialogProvider { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
        => $"{nameof(ConflictResolution)} = {ConflictResolution}  {nameof(CompareOptions)} = {CompareOptions}";
}
