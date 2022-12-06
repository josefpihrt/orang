// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class SearchOptions
{
    //TODO: rename?
    public Matcher? SearchDirectory { get; set; }

    public FileNamePart SearchDirectoryPart { get; set; }

    public Func<string, bool>? IncludeDirectory { get; set; }

    internal Func<string, bool>? IncludeDirectoryPredicate
    {
        get
        {
            if (SearchDirectory?.IsNegative == false)
            {
                if (IncludeDirectory is not null)
                {
                    return path =>
                    {
                        return SearchDirectory.IsDirectoryMatch(path, SearchDirectoryPart)
                            && IncludeDirectory(path);
                    };
                }

                return path => SearchDirectory.IsDirectoryMatch(path, SearchDirectoryPart);
            }

            return IncludeDirectory;
        }
    }

    public Func<string, bool>? ExcludeDirectory { get; set; }

    internal Func<string, bool>? ExcludeDirectoryPredicate
    {
        get
        {
            if (SearchDirectory?.IsNegative == true)
            {
                if (ExcludeDirectory is not null)
                {
                    return path =>
                    {
                        return !SearchDirectory.IsDirectoryMatch(path, SearchDirectoryPart)
                            && ExcludeDirectory(path);
                    };
                }

                return path => !SearchDirectory.IsDirectoryMatch(path, SearchDirectoryPart);
            }

            return ExcludeDirectory;
        }
    }

    public Action<SearchProgress>? LogProgress { get; set; }

    public SearchTarget SearchTarget { get; set; }

    public bool TopDirectoryOnly { get; set; }

    public Encoding? DefaultEncoding { get; set; }
}
