// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

//TODO: ErrorBehavior
public class SearchOptions
{
    public Func<string, bool>? IncludeDirectory { get; set; }

    public Func<string, bool>? ExcludeDirectory { get; set; }

    public IProgress<SearchProgress>? SearchProgress { get; init; }

    public SearchTarget SearchTarget { get; init; }

    public bool TopDirectoryOnly { get; init; }

    public Encoding? DefaultEncoding { get; init; }
}
