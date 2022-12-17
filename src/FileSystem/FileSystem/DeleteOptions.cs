// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Orang.FileSystem;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class DeleteOptions
{
    public bool ContentOnly { get; set; }

    public bool IncludingBom { get; set; }

    internal bool FilesOnly { get; set; }

    internal bool DirectoriesOnly { get; set; }

    public bool DryRun { get; set; }

    public Action<OperationProgress>? LogOperation { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{nameof(ContentOnly)} = {ContentOnly}  {nameof(IncludingBom)} = {IncludingBom}";
}
