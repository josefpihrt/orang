// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Orang.FileSystem.Commands;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public class Search
{
    public Search(
        FileMatcher matcher,
        SearchOptions? options = null)
    {
        if (matcher is null)
            throw new ArgumentNullException(nameof(matcher));

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
            LogProgress = Options.LogProgress,
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

        VerifyCopyMoveArguments(directoryPath, destinationPath, options);

        var command = new CopyCommand()
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
        DeleteOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        options ??= new DeleteOptions();

        var command = new DeleteCommand()
        {
            DeleteOptions = options,
            LogOperation = options.LogOperation,
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

        VerifyCopyMoveArguments(directoryPath, destinationPath, options);

        var command = new MoveCommand()
        {
            DestinationPath = destinationPath,
            CopyOptions = options,
            LogOperation = options.LogOperation,
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

        if (matcher.NamePart == FileNamePart.FullName)
            throw new InvalidOperationException($"Invalid file name part '{nameof(FileNamePart.FullName)}'.");

        if (matcher.Name is null)
            throw new InvalidOperationException("Name filter is not defined.");

        if (matcher.Name.IsNegative)
            throw new InvalidOperationException("Name filter cannot be negative.");

        VerifyConflictResolution(options.ConflictResolution, options.DialogProvider);

        var command = new RenameCommand()
        {
            NameFilter = matcher.Name,
            RenameOptions = options,
            Replacer = replacer,
            LogOperation = options.LogOperation,
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

        var command = new ReplaceCommand()
        {
            ContentFilter = matcher.Content,
            ReplaceOptions = options,
            Replacer = replacer,
            LogOperation = options.LogOperation,
            DryRun = options.DryRun,
            MaxMatchingFiles = 0,
            MaxMatchesInFile = 0,
            MaxTotalMatches = 0,
            CancellationToken = cancellationToken,
        };

        command.Execute(directoryPath, this, cancellationToken);

        return new OperationResult(command.Telemetry);
    }

    private static void VerifyConflictResolution(
        ConflictResolution conflictResolution,
        IDialogProvider<ConflictInfo>? dialogProvider)
    {
        if (conflictResolution == ConflictResolution.Ask
            && dialogProvider is null)
        {
            throw new ArgumentNullException(
                nameof(dialogProvider),
                $"'{nameof(dialogProvider)}' cannot be null when {nameof(ConflictResolution)} "
                    + $"is set to {nameof(ConflictResolution.Ask)}.");
        }
    }

    private static void VerifyCopyMoveArguments(
        string directoryPath,
        string destinationPath,
        CopyOptions copyOptions)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (destinationPath is null)
            throw new ArgumentNullException(nameof(destinationPath));

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        if (!Directory.Exists(destinationPath))
            throw new DirectoryNotFoundException($"Directory not found: {destinationPath}");

        if (FileSystemHelpers.IsSubdirectory(destinationPath, directoryPath))
        {
            throw new ArgumentException(
                "Source directory cannot be subdirectory of a destination directory.",
                nameof(directoryPath));
        }

        VerifyConflictResolution(copyOptions.ConflictResolution, copyOptions.DialogProvider);
    }
}
