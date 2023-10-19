// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class SearchOptions
{
    [Obsolete("This property is obsolete and will be removed in future versions. Use property Include/Exclude instead.")]
    public DirectoryMatcher? SearchDirectory { get; set; }

#pragma warning disable CS0618 // Type or member is obsolete
    internal Func<string, bool>? IncludeDirectoryPredicate
    {
        get
        {
            if (SearchDirectory is not null
                && SearchDirectory.Name?.Invert != true)
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
                && SearchDirectory.Name?.Invert != false)
            {
                return path => !SearchDirectory.IsMatch(path);
            }

            return null;
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete

    public Action<SearchProgress>? LogProgress { get; set; }

    public Encoding? DefaultEncoding { get; set; }

    public bool TopDirectoryOnly { get; set; }

    internal bool IgnoreInaccessible { get; set; } = true;

    /// <summary>
    /// Gets a list of glob patterns to include.
    /// </summary>
    public List<string> Include { get; } = new();

    /// <summary>
    /// Gets a list of glob patterns to exclude.
    /// </summary>
    public List<string> Exclude { get; } = new();
}
