// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Orang.FileSystem;

//TODO: JP ConflictInfo
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct ConflictInfo
{
    internal ConflictInfo(string sourcePath, string destinationPath)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
    }

    public string SourcePath { get; }

    public string DestinationPath { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{SourcePath}  {DestinationPath}";
}
