// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Orang.FileSystem.Commands;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Operations;

public class MoveOperation
{
    private SearchTarget _searchTarget;

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

    public List<NameFilter> DirectoryFilters { get; set; } = new();

    public SearchTarget SearchTarget
    {
        get { return _searchTarget; }
        set
        {
            if (value == SearchTarget.All)
                throw new ArgumentException($"Search target cannot be '{nameof(SearchTarget.All)}'.", nameof(value));

            _searchTarget = value;
        }
    }

    public bool RecurseSubdirectories { get; set; }

    public Encoding? DefaultEncoding { get; set; }

    public bool DryRun { get; set; }

    public CopyOptions? CopyOptions { get; set; }

    public IProgress<SearchProgress>? SearchProgress { get; set; }

    public IProgress<OperationProgress>? Progress { get; set; }

    public IDialogProvider<ConflictInfo>? DialogProvider { get; set; }

    public IOperationResult Execute(CancellationToken cancellationToken = default)
    {
        IEnumerable<NameFilter> directoryFilters = DirectoryFilters
            .ToArray()
            .Append(NameFilter.CreateFromDirectoryPath(DestinationPath, isNegative: true));

        var search = new FileSystemSearch(
            Filter,
            directoryFilters: directoryFilters,
            progress: SearchProgress,
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
