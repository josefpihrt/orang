// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using Orang.Operations;
using static Orang.FileSystem.FileSystemHelpers;

namespace Orang.FileSystem
{
    public partial class FileSystemSearch
    {
        internal static void Replace(
            string directoryPath,
            FileSystemFilter filter,
            IEnumerable<NameFilter>? directoryFilters = null,
            ReplaceOptions? replaceOptions = null,
            SearchTarget searchTarget = SearchTarget.Files,
            bool recurseSubdirectories = true,
            bool dryRun = false,
            CancellationToken cancellationToken = default)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));

            var searchOptions = new FileSystemSearchOptions(
                searchTarget: searchTarget,
                recurseSubdirectories: recurseSubdirectories);

            var search = new FileSystemSearch(
                filter,
                directoryFilters: directoryFilters?.ToImmutableArray() ?? ImmutableArray<NameFilter>.Empty,
                searchProgress: null,
                options: searchOptions);

            if (filter.Content == null)
                throw new InvalidOperationException("Content filter is not defined.");

            if (filter.Content.IsNegative)
                throw new InvalidOperationException("Content filter cannot be negative.");

            var command = new ReplaceOperation()
            {
                Search = search,
                ReplaceOptions = replaceOptions ?? ReplaceOptions.Empty,
                Progress = null,
                DryRun = dryRun,
                CancellationToken = cancellationToken,
                MaxMatchingFiles = 0,
                MaxMatchesInFile = 0,
                MaxTotalMatches = 0,
            };

            command.Execute(directoryPath);
        }

        internal static void Delete(
            string directoryPath,
            FileSystemFilter filter,
            IEnumerable<NameFilter>? directoryFilters = null,
            DeleteOptions? deleteOptions = null,
            SearchTarget searchTarget = SearchTarget.Files,
            IProgress<OperationProgress>? progress = null,
            bool recurseSubdirectories = true,
            bool dryRun = false,
            CancellationToken cancellationToken = default)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));

            var searchOptions = new FileSystemSearchOptions(
                searchTarget: searchTarget,
                recurseSubdirectories: recurseSubdirectories);

            var search = new FileSystemSearch(
                filter,
                directoryFilters: directoryFilters?.ToImmutableArray() ?? ImmutableArray<NameFilter>.Empty,
                searchProgress: null,
                options: searchOptions)
            {
                CanRecurseMatch = false
            };

            var command = new DeleteOperation()
            {
                Search = search,
                DeleteOptions = deleteOptions ?? DeleteOptions.Default,
                Progress = progress,
                DryRun = dryRun,
                CancellationToken = cancellationToken,
                MaxMatchingFiles = 0,
            };

            command.Execute(directoryPath);
        }

        internal static void Rename(
            string directoryPath,
            FileSystemFilter filter,
            IEnumerable<NameFilter>? directoryFilters = null,
            RenameOptions? renameOptions = null,
            SearchTarget searchTarget = SearchTarget.Files,
            IDialogProvider<OperationProgress>? dialogProvider = null,
            IProgress<OperationProgress>? progress = null,
            bool recurseSubdirectories = true,
            bool dryRun = false,
            CancellationToken cancellationToken = default)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));

            if (renameOptions == null)
                throw new ArgumentNullException(nameof(renameOptions));

            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));

            var searchOptions = new FileSystemSearchOptions(
                searchTarget: searchTarget,
                recurseSubdirectories: recurseSubdirectories);

            var search = new FileSystemSearch(
                filter,
                directoryFilters: directoryFilters?.ToImmutableArray() ?? ImmutableArray<NameFilter>.Empty,
                searchProgress: null,
                options: searchOptions)
            {
                DisallowEnumeration = !dryRun,
                MatchPartOnly = true
            };

            if (filter.Part == FileNamePart.FullName)
                throw new InvalidOperationException($"Invalid file name part '{nameof(FileNamePart.FullName)}'.");

            if (filter.Name == null)
                throw new InvalidOperationException("Name filter is not defined.");

            if (filter.Name.IsNegative)
                throw new InvalidOperationException("Name filter cannot be negative.");

            renameOptions ??= new RenameOptions(replacement: "");

            VerifyConflictResolution(renameOptions.ConflictResolution, dialogProvider);

            var command = new RenameOperation()
            {
                Search = search,
                RenameOptions = renameOptions,
                Progress = progress,
                DryRun = dryRun,
                CancellationToken = cancellationToken,
                DialogProvider = dialogProvider,
                MaxMatchingFiles = 0,
            };

            command.Execute(directoryPath);
        }

        internal static void Copy(
            string directoryPath,
            string destinationPath,
            FileSystemFilter filter,
            IEnumerable<NameFilter>? directoryFilters = null,
            CopyOptions? copyOptions = null,
            SearchTarget searchTarget = SearchTarget.Files,
            IDialogProvider<OperationProgress>? dialogProvider = null,
            IProgress<OperationProgress>? progress = null,
            bool recurseSubdirectories = true,
            bool dryRun = false,
            CancellationToken cancellationToken = default)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));

            var searchOptions = new FileSystemSearchOptions(
                searchTarget: searchTarget,
                recurseSubdirectories: recurseSubdirectories);

            ImmutableArray<NameFilter> directoryFilters2 = directoryFilters?.ToImmutableArray() ?? ImmutableArray<NameFilter>.Empty;

            directoryFilters2 = directoryFilters2.Add(NameFilter.CreateFromDirectoryPath(destinationPath, isNegative: true));

            var search = new FileSystemSearch(
                filter,
                directoryFilters: directoryFilters2,
                searchProgress: null,
                options: searchOptions);

            copyOptions ??= CopyOptions.Default;

            VerifyCopyMoveArguments(directoryPath, destinationPath, copyOptions, dialogProvider);

            var command = new CopyOperation()
            {
                Search = search,
                DestinationPath = destinationPath,
                CopyOptions = copyOptions,
                Progress = progress,
                DryRun = dryRun,
                CancellationToken = cancellationToken,
                DialogProvider = dialogProvider,
                MaxMatchingFiles = 0,
                MaxMatchesInFile = 0,
                MaxTotalMatches = 0,
                ConflictResolution = copyOptions.ConflictResolution,
            };

            command.Execute(directoryPath);
        }

        internal static void Move(
            string directoryPath,
            string destinationPath,
            FileSystemFilter filter,
            IEnumerable<NameFilter>? directoryFilters = null,
            CopyOptions? copyOptions = null,
            SearchTarget searchTarget = SearchTarget.Files,
            IDialogProvider<OperationProgress>? dialogProvider = null,
            IProgress<OperationProgress>? progress = null,
            bool recurseSubdirectories = true,
            bool dryRun = false,
            CancellationToken cancellationToken = default)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));

            var searchOptions = new FileSystemSearchOptions(
                searchTarget: searchTarget,
                recurseSubdirectories: recurseSubdirectories);

            ImmutableArray<NameFilter> directoryFilters2 = directoryFilters?.ToImmutableArray() ?? ImmutableArray<NameFilter>.Empty;

            directoryFilters2 = directoryFilters2.Add(NameFilter.CreateFromDirectoryPath(destinationPath, isNegative: true));

            var search = new FileSystemSearch(
                filter,
                directoryFilters: directoryFilters2,
                searchProgress: null,
                options: searchOptions);

            copyOptions ??= CopyOptions.Default;

            VerifyCopyMoveArguments(directoryPath, destinationPath, copyOptions, dialogProvider);

            var command = new MoveOperation()
            {
                Search = search,
                DestinationPath = destinationPath,
                CopyOptions = copyOptions,
                Progress = progress,
                DryRun = dryRun,
                CancellationToken = cancellationToken,
                DialogProvider = dialogProvider,
                MaxMatchingFiles = 0,
                MaxMatchesInFile = 0,
                MaxTotalMatches = 0,
                ConflictResolution = copyOptions.ConflictResolution,
            };

            command.Execute(directoryPath);
        }

        private static void VerifyCopyMoveArguments(
            string directoryPath,
            string destinationPath,
            CopyOptions copyOptions,
            IDialogProvider<OperationProgress>? dialogProvider)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));

            if (destinationPath == null)
                throw new ArgumentNullException(nameof(destinationPath));

            if (!System.IO.Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            if (!System.IO.Directory.Exists(destinationPath))
                throw new DirectoryNotFoundException($"Directory not found: {destinationPath}");

            if (IsSubdirectory(destinationPath, directoryPath))
            {
                throw new ArgumentException(
                    "Source directory cannot be subdirectory of a destination directory.",
                    nameof(directoryPath));
            }

            VerifyConflictResolution(copyOptions.ConflictResolution, dialogProvider);
        }

        private static void VerifyConflictResolution(
            ConflictResolution conflictResolution,
            IDialogProvider<OperationProgress>? dialogProvider)
        {
            if (conflictResolution == ConflictResolution.Ask
                && dialogProvider == null)
            {
                throw new ArgumentNullException(
                    nameof(dialogProvider),
                    $"'{nameof(dialogProvider)}' cannot be null when {nameof(ConflictResolution)} "
                        + $"is set to {nameof(ConflictResolution.Ask)}.");
            }
        }
    }
}
