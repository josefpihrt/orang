// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class SearchOptions
{
    public List<NameFilter> DirectoryFilters { get; } = new();

    public IProgress<SearchProgress>? SearchProgress { get; init; }

    public SearchTarget SearchTarget { get; init; }

    public bool RecurseSubdirectories { get; init; }

    public Encoding? DefaultEncoding { get; init; }
}
