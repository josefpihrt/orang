// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class ReplaceOptions : SearchOptions
{
    public ReplaceFunctions Functions { get; set; }

    public bool CultureInvariant { get; set; }

    public bool DryRun { get; set; }

    public IProgress<OperationProgress>? Progress { get; set; }
}
