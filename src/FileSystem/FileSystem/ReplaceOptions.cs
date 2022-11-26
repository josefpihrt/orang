// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Orang.FileSystem;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ReplaceOptions : SearchOptions
{
    public string? Replacement { get; set; }

    public MatchEvaluator? MatchEvaluator { get; set; }

    public ReplaceFunctions Functions { get; set; }

    public bool CultureInvariant { get; set; }

    public bool DryRun { get; set; }

    public IProgress<OperationProgress>? Progress { get; set; }

    internal CultureInfo CultureInfo => (CultureInvariant) ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => (Replacement is not null) ? $"{Replacement}" : $"{MatchEvaluator!.Method}";
}
