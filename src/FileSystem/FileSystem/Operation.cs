// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Orang.FileSystem.Commands;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public static class Operation
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

        var search = new FileSystemSearch(
            filter: filter,
            includeDirectory: options.IncludeDirectory,
            excludeDirectory: options.ExcludeDirectory,
            progress: options.SearchProgress,
            searchTarget: options.SearchTarget,
            recurseSubdirectories: !options.TopDirectoryOnly,
            defaultEncoding: options.DefaultEncoding);

        return search.Find(directoryPath, cancellationToken);
    }

    public static IOperationResult CopyMatches(
        string directoryPath,
        string destinationPath,
        FileSystemFilter filter,
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

        Func<string, bool> excludeDirectory = DirectoryPredicate.Create(options.ExcludeDirectory, destinationPath);

        var search = new FileSystemSearch(
            filter,
            includeDirectory: options.IncludeDirectory,
            excludeDirectory: excludeDirectory,
            progress: options.SearchProgress,
            searchTarget: options.SearchTarget,
            recurseSubdirectories: !options.TopDirectoryOnly,
            defaultEncoding: options.DefaultEncoding);

        OperationHelpers.VerifyCopyMoveArguments(directoryPath, destinationPath, options);

        var command = new CopyOperation()
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
        string destinationPath,
        FileSystemFilter filter,
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
            includeDirectory: options.IncludeDirectory,
            excludeDirectory: options.ExcludeDirectory,
            progress: options.SearchProgress,
            searchTarget: options.SearchTarget,
            recurseSubdirectories: !options.TopDirectoryOnly,
            defaultEncoding: options.DefaultEncoding)
        {
            CanRecurseMatch = false,
        };

        var command = new DeleteOperation()
        {
            Search = search,
            DeleteOptions = options,
            OperationProgress = options.OperationProgress,
            DryRun = options.DryRun,
            MaxMatchingFiles = 0,
            CancellationToken = cancellationToken,
        };

        command.Execute(directoryPath);

        return new OperationResult(command.Telemetry);
    }

    public static IOperationResult MoveMatches(
        string directoryPath,
        string destinationPath,
        FileSystemFilter filter,
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

        Func<string, bool> excludeDirectory = DirectoryPredicate.Create(options.ExcludeDirectory, destinationPath);

        var search = new FileSystemSearch(
            filter,
            includeDirectory: options.IncludeDirectory,
            excludeDirectory: excludeDirectory,
            progress: options.SearchProgress,
            searchTarget: options.SearchTarget,
            recurseSubdirectories: !options.TopDirectoryOnly,
            defaultEncoding: options.DefaultEncoding);

        options ??= new CopyOptions();

        OperationHelpers.VerifyCopyMoveArguments(directoryPath, destinationPath, options);

        var command = new MoveOperation()
        {
            Search = search,
            DestinationPath = destinationPath,
            CopyOptions = options,
            OperationProgress = options.OperationProgress,
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
        string replacement,
        RenameOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new RenameOptions();

        var replacer = new Replacer(replacement, options?.ReplaceFunctions ?? ReplaceFunctions.None, options?.CultureInvariant ?? false);

        return RenameMatches(directoryPath, filter, replacer, options, cancellationToken);
    }

    public static IOperationResult RenameMatches(
        string directoryPath,
        FileSystemFilter filter,
        MatchEvaluator matchEvaluator,
        RenameOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new RenameOptions();

        var replacer = new Replacer(matchEvaluator, options?.ReplaceFunctions ?? ReplaceFunctions.None, options?.CultureInvariant ?? false);

        return RenameMatches(directoryPath, filter, replacer, options, cancellationToken);
    }

    private static IOperationResult RenameMatches(
        string directoryPath,
        FileSystemFilter filter,
        Replacer replacer,
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
            includeDirectory: options.IncludeDirectory,
            excludeDirectory: options.ExcludeDirectory,
            progress: options.SearchProgress,
            searchTarget: options.SearchTarget,
            recurseSubdirectories: !options.TopDirectoryOnly,
            defaultEncoding: options.DefaultEncoding)
        {
            DisallowEnumeration = !options.DryRun,
            MatchPartOnly = true
        };

        var command = new RenameOperation()
        {
            Search = search,
            RenameOptions = options,
            Replacer = replacer,
            OperationProgress = options.OperationProgress,
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
        string replacement,
        ReplaceOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var replacer = new Replacer(replacement, options?.ReplaceFunctions ?? ReplaceFunctions.None, options?.CultureInvariant ?? false);

        return ReplaceMatches(directoryPath, filter, replacer, options, cancellationToken);
    }

    public static IOperationResult ReplaceMatches(
        string directoryPath,
        FileSystemFilter filter,
        MatchEvaluator matchEvaluator,
        ReplaceOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var replacer = new Replacer(matchEvaluator, options?.ReplaceFunctions ?? ReplaceFunctions.None, options?.CultureInvariant ?? false);

        return ReplaceMatches(directoryPath, filter, replacer, options, cancellationToken);
    }

    private static IOperationResult ReplaceMatches(
        string directoryPath,
        FileSystemFilter filter,
        Replacer replacer,
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
            includeDirectory: options.IncludeDirectory,
            excludeDirectory: options.ExcludeDirectory,
            progress: options.SearchProgress,
            searchTarget: options.SearchTarget,
            recurseSubdirectories: !options.TopDirectoryOnly,
            defaultEncoding: options.DefaultEncoding)
        {
            CanRecurseMatch = false,
        };

        var command = new ReplaceOperation()
        {
            Search = search,
            ReplaceOptions = options,
            Replacer = replacer,
            OperationProgress = options.OperationProgress,
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
