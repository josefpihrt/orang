// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Orang.FileSystem.Operations;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class Search
{
    public Search(
        FileMatcher matcher,
        SearchOptions? options = null)
    {
        if (matcher is null)
        {
            throw new ArgumentNullException(nameof(matcher));
        }

        Matcher = matcher;
        Options = options ?? new SearchOptions();
    }

    public FileMatcher Matcher { get; }

    public SearchOptions Options { get; }

    public IEnumerable<FileMatch> Matches(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        var state = new SearchState(Matcher)
        {
            IncludeDirectory = Options.IncludeDirectory,
            ExcludeDirectory = Options.ExcludeDirectory,
            Progress = Options.SearchProgress,
            SearchTarget = Options.SearchTarget,
            RecurseSubdirectories = !Options.TopDirectoryOnly,
            DefaultEncoding = Options.DefaultEncoding,
        };

        return state.Find(directoryPath, cancellationToken);
    }

    public IOperationResult Copy(
        string directoryPath,
        string destinationPath,
        CopyOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (destinationPath is null)
            throw new ArgumentNullException(nameof(destinationPath));

        if (Options.SearchTarget == SearchTarget.All)
            throw new InvalidOperationException($"Search target cannot be '{nameof(SearchTarget.All)}'.");

        options ??= new CopyOptions();

        OperationHelpers.VerifyCopyMoveArguments(directoryPath, destinationPath, options);

        var command = new CopyOperation()
        {
            DestinationPath = destinationPath,
            CopyOptions = options,
            MaxMatchingFiles = 0,
            MaxMatchesInFile = 0,
            MaxTotalMatches = 0,
            ConflictResolution = options.ConflictResolution,
            CancellationToken = cancellationToken,
        };

        command.Execute(directoryPath, this, cancellationToken);

        return new OperationResult(command.Telemetry);
    }

    public IOperationResult Delete(
        string directoryPath,
        string destinationPath,
        DeleteOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (destinationPath is null)
            throw new ArgumentNullException(nameof(destinationPath));

        options ??= new DeleteOptions();

        var command = new DeleteOperation()
        {
            DeleteOptions = options,
            OperationProgress = options.OperationProgress,
            DryRun = options.DryRun,
            MaxMatchingFiles = 0,
            CancellationToken = cancellationToken,
        };

        command.Execute(directoryPath, this, cancellationToken);

        return new OperationResult(command.Telemetry);
    }

    public IOperationResult Move(
        string directoryPath,
        string destinationPath,
        CopyOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (destinationPath is null)
            throw new ArgumentNullException(nameof(destinationPath));

        if (Options.SearchTarget == SearchTarget.All)
            throw new InvalidOperationException($"Search target cannot be '{nameof(SearchTarget.All)}'.");

        options ??= new CopyOptions();

        OperationHelpers.VerifyCopyMoveArguments(directoryPath, destinationPath, options);

        var command = new MoveOperation()
        {
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

        command.Execute(directoryPath, this, cancellationToken);

        return new OperationResult(command.Telemetry);
    }

    public IOperationResult Rename(
        string directoryPath,
        string replacement,
        RenameOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new RenameOptions();

        var replacer = new Replacer(replacement, options?.ReplaceFunctions ?? ReplaceFunctions.None, options?.CultureInvariant ?? false);

        return Rename(directoryPath, replacer, options, cancellationToken);
    }

    public IOperationResult Rename(
        string directoryPath,
        MatchEvaluator matchEvaluator,
        RenameOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new RenameOptions();

        var replacer = new Replacer(matchEvaluator, options?.ReplaceFunctions ?? ReplaceFunctions.None, options?.CultureInvariant ?? false);

        return Rename(directoryPath, replacer, options, cancellationToken);
    }

    private IOperationResult Rename(
        string directoryPath,
        Replacer replacer,
        RenameOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        FileMatcher matcher = Matcher;

        options ??= new RenameOptions();

        if (matcher.Part == FileNamePart.FullName)
            throw new InvalidOperationException($"Invalid file name part '{nameof(FileNamePart.FullName)}'.");

        if (matcher.Name is null)
            throw new InvalidOperationException("Name filter is not defined.");

        if (matcher.Name.IsNegative)
            throw new InvalidOperationException("Name filter cannot be negative.");

        OperationHelpers.VerifyConflictResolution(options.ConflictResolution, options.DialogProvider);

        var command = new RenameOperation()
        {
            NameFilter = matcher.Name,
            RenameOptions = options,
            Replacer = replacer,
            OperationProgress = options.OperationProgress,
            DryRun = options.DryRun,
            DialogProvider = options.DialogProvider,
            MaxMatchingFiles = 0,
            ConflictResolution = options.ConflictResolution,
            CancellationToken = cancellationToken,
        };

        command.Execute(directoryPath, this, cancellationToken);

        return new OperationResult(command.Telemetry);
    }

    public IOperationResult Replace(
        string directoryPath,
        string replacement,
        ReplaceOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var replacer = new Replacer(replacement, options?.ReplaceFunctions ?? ReplaceFunctions.None, options?.CultureInvariant ?? false);

        return Replace(directoryPath, replacer, options, cancellationToken);
    }

    public IOperationResult Replace(
        string directoryPath,
        MatchEvaluator matchEvaluator,
        ReplaceOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var replacer = new Replacer(matchEvaluator, options?.ReplaceFunctions ?? ReplaceFunctions.None, options?.CultureInvariant ?? false);

        return Replace(directoryPath, replacer, options, cancellationToken);
    }

    private IOperationResult Replace(
        string directoryPath,
        Replacer replacer,
        ReplaceOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        FileMatcher matcher = Matcher;

        if (matcher.Content is null)
            throw new InvalidOperationException("Content filter is not defined.");

        if (matcher.Content.IsNegative)
            throw new InvalidOperationException("Content filter cannot be negative.");

        options ??= new ReplaceOptions();

        var command = new ReplaceOperation()
        {
            ContentFilter = matcher.Content,
            ReplaceOptions = options,
            Replacer = replacer,
            OperationProgress = options.OperationProgress,
            DryRun = options.DryRun,
            MaxMatchingFiles = 0,
            MaxMatchesInFile = 0,
            MaxTotalMatches = 0,
            CancellationToken = cancellationToken,
        };

        command.Execute(directoryPath, this, cancellationToken);

        return new OperationResult(command.Telemetry);
    }
}
