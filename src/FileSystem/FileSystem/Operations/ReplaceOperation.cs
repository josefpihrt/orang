// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Orang.FileSystem.Commands;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Operations;

public class ReplaceOperation
{
    public ReplaceOperation(
        string directoryPath,
        FileSystemFilter filter)
    {
        DirectoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));

        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        if (filter.Content == null)
            throw new InvalidOperationException("Content filter is not defined.");

        if (filter.Content.IsNegative)
            throw new InvalidOperationException("Content filter cannot be negative.");

        Filter = filter;
    }

    public string DirectoryPath { get; }

    public FileSystemFilter Filter { get; }

    public NameFilter[]? DirectoryFilters { get; set; }

    public SearchTarget SearchTarget { get; set; }

    public bool RecurseSubdirectories { get; set; }

    public Encoding? DefaultEncoding { get; set; }

    public bool DryRun { get; set; }

    public ReplaceOptions? ReplaceOptions { get; set; }

    public IProgress<OperationProgress>? Progress { get; set; }

    public ReplaceOperation WithDirectoryFilter(NameFilter directoryFilter)
    {
        DirectoryFilters = new NameFilter[] { directoryFilter };
        return this;
    }

    public ReplaceOperation WithDirectoryFilters(IEnumerable<NameFilter> directoryFilters)
    {
        if (directoryFilters is null)
            throw new ArgumentNullException(nameof(directoryFilters));

        DirectoryFilters = directoryFilters.ToArray();
        return this;
    }

    public ReplaceOperation WithSearchTarget(SearchTarget searchTarget)
    {
        SearchTarget = searchTarget;
        return this;
    }

    public ReplaceOperation WithTopDirectoryOnly()
    {
        RecurseSubdirectories = false;
        return this;
    }

    public ReplaceOperation WithDefaultEncoding(Encoding encoding)
    {
        DefaultEncoding = encoding;
        return this;
    }

    public ReplaceOperation WithDryRun()
    {
        DryRun = true;
        return this;
    }

    public ReplaceOperation WithReplaceOptions(ReplaceOptions replaceOptions)
    {
        ReplaceOptions = replaceOptions;
        return this;
    }

    public ReplaceOperation WithProgress(IProgress<OperationProgress> progress)
    {
        Progress = progress;
        return this;
    }

    public OperationResult Execute(CancellationToken cancellationToken = default)
    {
        var search = new FileSystemSearch(
            Filter,
            directoryFilters: DirectoryFilters?.ToImmutableArray() ?? ImmutableArray<NameFilter>.Empty,
            progress: null,
            searchTarget: SearchTarget,
            recurseSubdirectories: RecurseSubdirectories,
            defaultEncoding: DefaultEncoding)
        {
            CanRecurseMatch = false,
        };

        var command = new ReplaceCommand()
        {
            Search = search,
            ReplaceOptions = ReplaceOptions ?? ReplaceOptions.Empty,
            Progress = Progress,
            DryRun = DryRun,
            CancellationToken = cancellationToken,
            MaxMatchingFiles = 0,
            MaxMatchesInFile = 0,
            MaxTotalMatches = 0,
        };

        command.Execute(DirectoryPath);

        return new OperationResult(command.Telemetry);
    }
}
