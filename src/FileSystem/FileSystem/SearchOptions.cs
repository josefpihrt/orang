// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class SearchOptions
{
    //TODO: rename SearchDirectory?
    public Matcher? SearchDirectory { get; set; }

    public FileNamePart SearchDirectoryPart { get; set; }

    internal Func<string, bool>? IncludeDirectoryPredicate
    {
        get
        {
            return (SearchDirectory?.IsNegative == false)
                ? path => SearchDirectory.IsDirectoryMatch(path, SearchDirectoryPart)
                : null;
        }
    }

    internal Func<string, bool>? ExcludeDirectoryPredicate
    {
        get
        {
            return (SearchDirectory?.IsNegative == true)
                ? path => !SearchDirectory.IsDirectoryMatch(path, SearchDirectoryPart)
                : null;
        }
    }

    public Action<SearchProgress>? LogProgress { get; set; }

    public bool TopDirectoryOnly { get; set; }

    public Encoding? DefaultEncoding { get; set; }
}
