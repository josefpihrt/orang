// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

    public NameFilter[]? DirectoryFilters { get; set; }

    public IProgress<SearchProgress>? Progress { get; set; }

    public SearchTarget SearchTarget { get; set; }

    public bool RecurseSubdirectories { get; set; }

    public Encoding? DefaultEncoding { get; set; }

    public FindOperation WithDirectoryFilter(NameFilter directoryFilter)
    {
        return WithDirectoryFilters(new NameFilter[] { directoryFilter });
    }

    public FindOperation WithDirectoryFilters(IEnumerable<NameFilter> directoryFilters)
    {
        foreach (NameFilter directoryFilter in directoryFilters)
        {
            if (directoryFilter == null)
                throw new ArgumentException("", nameof(directoryFilter));

            if (directoryFilter?.Part == FileNamePart.Extension)
            {
                throw new ArgumentException(
                    $"Directory filter has invalid part '{FileNamePart.Extension}'.",
                    nameof(directoryFilter));
            }
        }

        DirectoryFilters = directoryFilters.ToArray();
        return this;
    }

    public FindOperation WithSearchTarget(SearchTarget searchTarget)
    {
        SearchTarget = searchTarget;
        return this;
    }

    public FindOperation WithTopDirectoryOnly()
    {
        RecurseSubdirectories = false;
        return this;
    }

    public FindOperation WithDefaultEncoding(Encoding encoding)
    {
        DefaultEncoding = encoding;
        return this;
    }

    public FindOperation WithProgress(IProgress<SearchProgress> progress)
    {
        Progress = progress;
        return this;
    }

    public IEnumerable<FileMatch> Execute(CancellationToken cancellationToken = default)
    {
        var search = new FileSystemSearch(
            Filter,
            DirectoryFilters ?? Array.Empty<NameFilter>(),
            Progress,
            searchTarget: SearchTarget,
            recurseSubdirectories: RecurseSubdirectories,
            defaultEncoding: DefaultEncoding);

        return search.Find(DirectoryPath, cancellationToken);
    }
}
