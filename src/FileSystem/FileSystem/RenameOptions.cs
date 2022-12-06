// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.FileSystem;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

public class RenameOptions
{
    public ReplaceFunctions ReplaceFunctions { get; set; }

    public bool CultureInvariant { get; set; }

    public bool DryRun { get; set; }

    public Action<OperationProgress>? LogOperation { get; set; }

    public IDialogProvider<ConflictInfo>? DialogProvider { get; set; }

    public ConflictResolution ConflictResolution { get; set; } = ConflictResolution.Skip;
}
