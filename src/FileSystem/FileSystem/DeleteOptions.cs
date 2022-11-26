// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Orang.FileSystem;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class DeleteOptions : SearchOptions
{
    public bool ContentOnly { get; set; }

    public bool IncludingBom { get; set; }

    public bool FilesOnly { get; set; }

    public bool DirectoriesOnly { get; set; }

    public bool DryRun { get; set; }

    public IProgress<OperationProgress>? Progress { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{nameof(ContentOnly)} = {ContentOnly}  {nameof(IncludingBom)} = {IncludingBom}";
}
