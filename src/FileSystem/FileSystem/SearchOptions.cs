// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class SearchOptions
{
    public Func<string, bool>? IncludeDirectory { get; set; }

    public Func<string, bool>? ExcludeDirectory { get; set; }

    public Action<SearchProgress>? LogProgress { get; set; }

    public SearchTarget SearchTarget { get; set; }

    public bool TopDirectoryOnly { get; set; }

    public Encoding? DefaultEncoding { get; set; }
}
