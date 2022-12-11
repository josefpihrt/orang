// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class SearchOptions
{
    public DirectoryMatcher? SearchDirectory { get; set; }

    internal Func<string, bool>? IncludeDirectoryPredicate
    {
        get
        {
            if (SearchDirectory is not null
                && SearchDirectory.Name?.InvertMatch != true)
            {
                return path => SearchDirectory.IsMatch(path);
            }

            return null;
        }
    }

    internal Func<string, bool>? ExcludeDirectoryPredicate
    {
        get
        {
            if (SearchDirectory is not null
                && SearchDirectory.Name?.InvertMatch != false)
            {
                return path => !SearchDirectory.IsMatch(path);
            }

            return null;
        }
    }

    public Action<SearchProgress>? LogProgress { get; set; }

    public Encoding? DefaultEncoding { get; set; }

    public bool TopDirectoryOnly { get; set; }

    public bool IgnoreInaccessible { get; set; } = true;
}
