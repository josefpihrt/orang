// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Orang.FileSystem.Commands;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public static class FileSystemOperation
{
    public static IEnumerable<FileMatch> GetMatches(
        string directoryPath,
        FileSystemFilter filter,
        SearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
        {
            throw new ArgumentNullException(nameof(directoryPath));
        }

        if (filter is null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        options ??= new SearchOptions();

        var directoryFilters = new List<NameFilter>();

        foreach (NameFilter directoryFilter in options.DirectoryFilters)
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
            filter: filter,
            directoryFilters: directoryFilters,
            progress: options.SearchProgress,
            searchTarget: options.SearchTarget,
            recurseSubdirectories: options.RecurseSubdirectories,
            defaultEncoding: options.DefaultEncoding);

        return search.Find(directoryPath, cancellationToken);
    }

    public static IOperationResult CopyMatches(
        string directoryPath,
        FileSystemFilter filter,
        string destinationPath,
        CopyOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
        {
            throw new ArgumentNullException(nameof(directoryPath));
        }

        if (filter is null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        if (destinationPath is null)
        {
            throw new ArgumentNullException(nameof(destinationPath));
        }

        options ??= new CopyOptions();

        if (options.SearchTarget == SearchTarget.All)
            throw new InvalidOperationException($"Search target cannot be '{nameof(SearchTarget.All)}'.");

        IEnumerable<NameFilter> directoryFilters = options.DirectoryFilters
            .ToArray()
            .Append(NameFilter.CreateFromDirectoryPath(destinationPath, isNegative: true));

        var search = new FileSystemSearch(
            filter,
            directoryFilters: directoryFilters,
            progress: options.SearchProgress,
            searchTarget: options.SearchTarget,
            recurseSubdirectories: options.RecurseSubdirectories,
            defaultEncoding: options.DefaultEncoding);

        OperationHelpers.VerifyCopyMoveArguments(directoryPath, destinationPath, options);

        var command = new CopyCommand()
        {
            Search = search,
            DestinationPath = destinationPath,
            CopyOptions = options,
            MaxMatchingFiles = 0,
            MaxMatchesInFile = 0,
            MaxTotalMatches = 0,
            ConflictResolution = options.ConflictResolution,
            CancellationToken = cancellationToken,
        };

        command.Execute(directoryPath);

        return new OperationResult(command.Telemetry);
    }

    public static IOperationResult DeleteMatches(
        string directoryPath,
        FileSystemFilter filter,
        string destinationPath,
        DeleteOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        if (destinationPath is null)
            throw new ArgumentNullException(nameof(destinationPath));

        options ??= new DeleteOptions();

        var search = new FileSystemSearch(
            filter,
            directoryFilters: options.DirectoryFilters.ToArray(),
            progress: options.SearchProgress,
            searchTarget: options.SearchTarget,
            recurseSubdirectories: options.RecurseSubdirectories,
            defaultEncoding: options.DefaultEncoding)
        {
            CanRecurseMatch = false,
        };

        var command = new DeleteCommand()
        {
            Search = search,
            DeleteOptions = options,
            Progress = options.Progress,
            DryRun = options.DryRun,
            MaxMatchingFiles = 0,
            CancellationToken = cancellationToken,
        };

        command.Execute(directoryPath);

        return new OperationResult(command.Telemetry);
    }

    public static IOperationResult MoveMatches(
        string directoryPath,
        FileSystemFilter filter,
        string destinationPath,
        CopyOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        if (destinationPath is null)
            throw new ArgumentNullException(nameof(destinationPath));

        options ??= new CopyOptions();

        if (options.SearchTarget == SearchTarget.All)
            throw new InvalidOperationException($"Search target cannot be '{nameof(SearchTarget.All)}'.");

        IEnumerable<NameFilter> directoryFilters = options.DirectoryFilters
            .ToArray()
            .Append(NameFilter.CreateFromDirectoryPath(destinationPath, isNegative: true));

        var search = new FileSystemSearch(
            filter,
            directoryFilters: directoryFilters,
            progress: options.SearchProgress,
            searchTarget: options.SearchTarget,
            recurseSubdirectories: options.RecurseSubdirectories,
            defaultEncoding: options.DefaultEncoding);

        options ??= new CopyOptions();

        OperationHelpers.VerifyCopyMoveArguments(directoryPath, destinationPath, options);

        var command = new MoveCommand()
        {
            Search = search,
            DestinationPath = destinationPath,
            CopyOptions = options,
            Progress = options.Progress,
            DryRun = options.DryRun,
            DialogProvider = options.DialogProvider,
            MaxMatchingFiles = 0,
            MaxMatchesInFile = 0,
            MaxTotalMatches = 0,
            ConflictResolution = options.ConflictResolution,
            CancellationToken = cancellationToken,
        };

        command.Execute(directoryPath);

        return new OperationResult(command.Telemetry);
    }

    public static IOperationResult RenameMatches(
        string directoryPath,
        FileSystemFilter filter,
        RenameOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        options ??= new RenameOptions();

        if (filter.Part == FileNamePart.FullName)
            throw new InvalidOperationException($"Invalid file name part '{nameof(FileNamePart.FullName)}'.");

        if (filter.Name is null)
            throw new InvalidOperationException("Name filter is not defined.");

        if (filter.Name.IsNegative)
            throw new InvalidOperationException("Name filter cannot be negative.");

        OperationHelpers.VerifyConflictResolution(options.ConflictResolution, options.DialogProvider);

        var search = new FileSystemSearch(
            filter,
            directoryFilters: options.DirectoryFilters.ToArray(),
            progress: options.SearchProgress,
            searchTarget: options.SearchTarget,
            recurseSubdirectories: options.RecurseSubdirectories,
            defaultEncoding: options.DefaultEncoding)
        {
            DisallowEnumeration = !options.DryRun,
            MatchPartOnly = true
        };

        var command = new RenameCommand()
        {
            Search = search,
            RenameOptions = options,
            Replacer = (options.MatchEvaluator is not null)
                ? new Replacer(options.MatchEvaluator, options.Functions, options.CultureInvariant)
                : new Replacer(options.Replacement, options.Functions, options.CultureInvariant),
            Progress = options.Progress,
            DryRun = options.DryRun,
            DialogProvider = options.DialogProvider,
            MaxMatchingFiles = 0,
            ConflictResolution = options.ConflictResolution,
            CancellationToken = cancellationToken,
        };

        command.Execute(directoryPath);

        return new OperationResult(command.Telemetry);
    }

    public static IOperationResult ReplaceMatches(
        string directoryPath,
        FileSystemFilter filter,
        ReplaceOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        if (filter.Content is null)
            throw new InvalidOperationException("Content filter is not defined.");

        if (filter.Content.IsNegative)
            throw new InvalidOperationException("Content filter cannot be negative.");

        options ??= new ReplaceOptions();

        var search = new FileSystemSearch(
            filter,
            directoryFilters: options.DirectoryFilters.ToArray(),
            progress: options.SearchProgress,
            searchTarget: options.SearchTarget,
            recurseSubdirectories: options.RecurseSubdirectories,
            defaultEncoding: options.DefaultEncoding)
        {
            CanRecurseMatch = false,
        };

        var command = new ReplaceCommand()
        {
            Search = search,
            ReplaceOptions = options,
            Replacer = (options.MatchEvaluator is not null)
                ? new Replacer(options.MatchEvaluator, options.Functions, options.CultureInvariant)
                : new Replacer(options.Replacement, options.Functions, options.CultureInvariant),
            Progress = options.Progress,
            DryRun = options.DryRun,
            MaxMatchingFiles = 0,
            MaxMatchesInFile = 0,
            MaxTotalMatches = 0,
            CancellationToken = cancellationToken,
        };

        command.Execute(directoryPath);

        return new OperationResult(command.Telemetry);
    }
}
