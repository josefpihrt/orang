// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Operations;

public class FindOperation
{
    public FindOperation(
        string directoryPath,
        FileSystemFilter filter)
    {
        DirectoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
        Filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }

    public string DirectoryPath { get; }

    public FileSystemFilter Filter { get; }

    public List<NameFilter> DirectoryFilters { get; set; } = new();

    public IProgress<SearchProgress>? Progress { get; set; }

    public SearchTarget SearchTarget { get; set; }

    public bool RecurseSubdirectories { get; set; }

    public Encoding? DefaultEncoding { get; set; }

    public IEnumerable<FileMatch> Execute(CancellationToken cancellationToken = default)
    {
        var directoryFilters = new List<NameFilter>();

        foreach (NameFilter directoryFilter in DirectoryFilters)
        {
            if (directoryFilter is null)
                throw new ArgumentException("", nameof(directoryFilter));

            if (directoryFilter?.Part == FileNamePart.Extension)
            {
                throw new ArgumentException(
                    $"Directory filter has invalid part '{FileNamePart.Extension}'.",
                    nameof(directoryFilter));
            }

            directoryFilters.Add(directoryFilter!);
        }

        var search = new FileSystemSearch(
            filter: Filter,
            directoryFilters: directoryFilters,
            progress: Progress,
            searchTarget: SearchTarget,
            recurseSubdirectories: RecurseSubdirectories,
            defaultEncoding: DefaultEncoding);

        return search.Find(DirectoryPath, cancellationToken);
    }
}
