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
        FileMatcher fileMatcher,
        SearchOptions? options = null)
    {
        FileMatcher = fileMatcher ?? throw new ArgumentNullException(nameof(fileMatcher));
        Options = options ?? new SearchOptions();
    }

    public Search(
        DirectoryMatcher directoryMatcher,
        SearchOptions? options = null)
    {
        DirectoryMatcher = directoryMatcher ?? throw new ArgumentNullException(nameof(directoryMatcher));
        Options = options ?? new SearchOptions();
    }

    public Search(
        FileMatcher fileMatcher,
        DirectoryMatcher directoryMatcher,
        SearchOptions? options = null)
    {
        FileMatcher = fileMatcher ?? throw new ArgumentNullException(nameof(fileMatcher));
        DirectoryMatcher = directoryMatcher ?? throw new ArgumentNullException(nameof(directoryMatcher));
        Options = options ?? new SearchOptions();
    }

    public FileMatcher? FileMatcher { get; }

    public DirectoryMatcher? DirectoryMatcher { get; }

    public SearchOptions Options { get; }

    public IEnumerable<FileMatch> Matches(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        var state = new SearchState(FileMatcher, DirectoryMatcher)
        {
            IncludeDirectory = Options.IncludeDirectoryPredicate,
            ExcludeDirectory = Options.ExcludeDirectoryPredicate,
            LogProgress = Options.LogProgress,
            RecurseSubdirectories = !Options.TopDirectoryOnly,
            DefaultEncoding = Options.DefaultEncoding,
            IgnoreInaccessible = Options.IgnoreInaccessible,
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

        if (DirectoryMatcher is not null)
            throw new InvalidOperationException("Directory matcher cannot be specified when executing 'copy' operation.");

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

        if (DirectoryMatcher is not null)
            throw new InvalidOperationException("Directory matcher cannot be specified when executing 'move' operation.");

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

        options ??= new RenameOptions();

        FileMatcher? fileMatcher = FileMatcher;

        if (fileMatcher is not null)
        {
            if (fileMatcher.NamePart == FileNamePart.FullName)
                throw new InvalidOperationException($"Invalid file name part '{nameof(FileNamePart.FullName)}'.");

            if (fileMatcher.Name is null)
                throw new InvalidOperationException("Name matcher is not defined.");

            if (fileMatcher.Name.InvertMatch)
                throw new InvalidOperationException("Name matcher cannot be negative.");
        }

        DirectoryMatcher? directoryMatcher = DirectoryMatcher;

        if (directoryMatcher is not null)
        {
            if (directoryMatcher.NamePart == FileNamePart.FullName
                || directoryMatcher.NamePart == FileNamePart.Extension)
            {
                throw new InvalidOperationException($"Invalid file name part '{directoryMatcher.NamePart}'.");
            }

            if (directoryMatcher.Name is null)
                throw new InvalidOperationException("Name matcher is not defined.");

            if (directoryMatcher.Name.InvertMatch)
                throw new InvalidOperationException("Name matcher cannot be negative.");
        }

        VerifyConflictResolution(options.ConflictResolution, options.DialogProvider);

        var command = new RenameCommand()
        {
            FileMatcher = FileMatcher?.Name,
            DirectoryMatcher = DirectoryMatcher?.Name,
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

        if (FileMatcher is null)
            throw new InvalidOperationException($"'{nameof(FileMatcher)}' is null.");

        FileMatcher matcher = FileMatcher;

        if (matcher.Content is null)
            throw new InvalidOperationException("Content matcher is not defined.");

        if (matcher.Content.InvertMatch)
            throw new InvalidOperationException("Content matcher cannot be negative.");

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

        if (FileSystemUtilities.IsSubdirectory(destinationPath, directoryPath))
        {
            throw new ArgumentException(
                "Source directory cannot be subdirectory of a destination directory.",
                nameof(directoryPath));
        }

        VerifyConflictResolution(copyOptions.ConflictResolution, copyOptions.DialogProvider);
    }
}
