// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Orang.FileSystem.Commands;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Operations
{
    public class CopyOperation
    {
        public CopyOperation(
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

        public CopyOperation WithDirectoryFilter(NameFilter directoryFilter)
        {
            if (directoryFilter is null)
                throw new ArgumentNullException(nameof(directoryFilter));

            DirectoryFilters = new NameFilter[] { directoryFilter };
            return this;
        }

        public CopyOperation WithDirectoryFilters(IEnumerable<NameFilter> directoryFilters)
        {
            if (directoryFilters is null)
                throw new ArgumentNullException(nameof(directoryFilters));

            DirectoryFilters = directoryFilters.ToArray();
            return this;
        }

        public CopyOperation WithSearchTarget(SearchTarget searchTarget)
        {
            if (searchTarget == SearchTarget.All)
                throw new ArgumentException($"Search target cannot be '{nameof(SearchTarget.All)}'.", nameof(searchTarget));

            SearchTarget = searchTarget;
            return this;
        }

        public CopyOperation WithTopDirectoryOnly()
        {
            RecurseSubdirectories = false;
            return this;
        }

        public CopyOperation WithDefaultEncoding(Encoding encoding)
        {
            DefaultEncoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            return this;
        }

        public CopyOperation WithDryRun()
        {
            DryRun = true;
            return this;
        }

        public CopyOperation WithCopyOptions(CopyOptions copyOptions)
        {
            CopyOptions = copyOptions ?? throw new ArgumentNullException(nameof(copyOptions));
            return this;
        }

        public CopyOperation WithProgress(IProgress<OperationProgress> progress)
        {
            Progress = progress ?? throw new ArgumentNullException(nameof(progress));
            return this;
        }

        public CopyOperation WithDialogProvider(IDialogProvider<ConflictInfo> dialogProvider)
        {
            DialogProvider = dialogProvider ?? throw new ArgumentNullException(nameof(dialogProvider));
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

            var operation = new CopyCommand()
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
}
