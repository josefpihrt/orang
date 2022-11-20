// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;

namespace Orang.FileSystem;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class CopyOptions
{
    internal static CopyOptions Default { get; } = new();

    public CopyOptions(
        ConflictResolution conflictResolution = ConflictResolution.Skip,
        FileCompareOptions compareOptions = FileCompareOptions.None,
        FileAttributes noCompareAttributes = 0,
        TimeSpan? allowedTimeDiff = null,
        bool flat = false)
    {
        ConflictResolution = conflictResolution;
        CompareOptions = compareOptions;
        NoCompareAttributes = noCompareAttributes;
        AllowedTimeDiff = allowedTimeDiff;
        Flat = flat;
    }

    public ConflictResolution ConflictResolution { get; }

    public FileCompareOptions CompareOptions { get; }

    public FileAttributes NoCompareAttributes { get; }

    public TimeSpan? AllowedTimeDiff { get; }

    public bool Flat { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
        => $"{nameof(ConflictResolution)} = {ConflictResolution}  {nameof(CompareOptions)} = {CompareOptions}";

    internal bool StructureOnly { get; }
}
