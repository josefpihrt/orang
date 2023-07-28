// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Orang.FileSystem;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct OperationProgress
{
    internal OperationProgress(FileMatch match, OperationKind kind, Exception? exception = null)
        : this(match, null, kind, exception)
    {
    }

    internal OperationProgress(FileMatch match, string? newPath, OperationKind kind, Exception? exception = null)
    {
        Match = match;
        NewPath = newPath;
        Kind = kind;
        Exception = exception;
    }

    public FileMatch Match { get; }

    public string Path => Match.Path;

    public string? NewPath { get; }

    internal OperationKind Kind { get; }

    public Exception? Exception { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => (Match is not null) ? $"{Kind}  {Match.Path}" : "Uninitialized";
}
