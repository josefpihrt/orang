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

public class MoveOperation
{
    public MoveOperation(
        string directoryPath,
        string destinationPath,
        FileSystemFilter filter)
    {
        DirectoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
        DestinationPath = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
        Filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }

    public string DirectoryPath { get; }

    public string DestinationPath { get; }

    public FileSystemFilter Filter { get; }

    public NameFilter[]? DirectoryFilters { get; set; }

    public SearchTarget SearchTarget { get; set; }

    public bool RecurseSubdirectories { get; set; }

    public Encoding? DefaultEncoding { get; set; }

    public bool DryRun { get; set; }

    public CopyOptions? CopyOptions { get; set; }

    public IProgress<OperationProgress>? Progress { get; set; }

    public IDialogProvider<ConflictInfo>? DialogProvider { get; set; }

    public MoveOperation WithDirectoryFilter(NameFilter directoryFilter)
    {
        DirectoryFilters = new NameFilter[] { directoryFilter };
        return this;
    }

    public MoveOperation WithDirectoryFilters(IEnumerable<NameFilter> directoryFilters)
    {
        if (directoryFilters is null)
            throw new ArgumentNullException(nameof(directoryFilters));

        DirectoryFilters = directoryFilters.ToArray();
        return this;
    }

    public MoveOperation WithSearchTarget(SearchTarget searchTarget)
    {
        if (searchTarget == SearchTarget.All)
            throw new ArgumentException($"Search target cannot be '{nameof(SearchTarget.All)}'.", nameof(searchTarget));

        SearchTarget = searchTarget;
        return this;
    }

    public MoveOperation WithTopDirectoryOnly()
    {
        RecurseSubdirectories = false;
        return this;
    }

    public MoveOperation WithDefaultEncoding(Encoding encoding)
    {
        DefaultEncoding = encoding;
        return this;
    }

    public MoveOperation WithDryRun()
    {
        DryRun = true;
        return this;
    }

    public MoveOperation WithCopyOptions(CopyOptions copyOptions)
    {
        CopyOptions = copyOptions;
        return this;
    }

    public MoveOperation WithProgress(IProgress<OperationProgress> progress)
    {
        Progress = progress;
        return this;
    }

    public MoveOperation WithDialogProvider(IDialogProvider<ConflictInfo> dialogProvider)
    {
        DialogProvider = dialogProvider;
        return this;
    }

    public OperationResult Execute(CancellationToken cancellationToken = default)
    {
        ImmutableArray<NameFilter> directoryFilters2 = DirectoryFilters?.ToImmutableArray() ?? ImmutableArray<NameFilter>.Empty;

        directoryFilters2 = directoryFilters2.Add(NameFilter.CreateFromDirectoryPath(DestinationPath, isNegative: true));

        var search = new FileSystemSearch(
            Filter,
            directoryFilters: directoryFilters2,
            progress: null,
            searchTarget: SearchTarget,
            recurseSubdirectories: RecurseSubdirectories,
            defaultEncoding: DefaultEncoding);

        CopyOptions copyOptions = CopyOptions ?? CopyOptions.Default;

        OperationHelpers.VerifyCopyMoveArguments(DirectoryPath, DestinationPath, copyOptions, DialogProvider);

        var operation = new MoveCommand()
        {
            Search = search,
            DestinationPath = DestinationPath,
            CopyOptions = copyOptions,
            Progress = Progress,
            DryRun = DryRun,
            CancellationToken = cancellationToken,
            DialogProvider = DialogProvider,
            MaxMatchingFiles = 0,
            MaxMatchesInFile = 0,
            MaxTotalMatches = 0,
            ConflictResolution = copyOptions.ConflictResolution,
        };

        operation.Execute(DirectoryPath);

        return new OperationResult(operation.Telemetry);
    }
}
