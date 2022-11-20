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
    public class RenameOperation
    {
        public RenameOperation(
            string directoryPath,
            FileSystemFilter filter)
        {
            DirectoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));

            if (filter is null)
                throw new ArgumentNullException(nameof(filter));

            if (filter.Part == FileNamePart.FullName)
                throw new InvalidOperationException($"Invalid file name part '{nameof(FileNamePart.FullName)}'.");

            if (filter.Name == null)
                throw new InvalidOperationException("Name filter is not defined.");

            if (filter.Name.IsNegative)
                throw new InvalidOperationException("Name filter cannot be negative.");

            Filter = filter;
        }

        public string DirectoryPath { get; }

        public FileSystemFilter Filter { get; }

        public NameFilter[]? DirectoryFilters { get; set; }

        public SearchTarget SearchTarget { get; set; }

        public bool RecurseSubdirectories { get; set; }

        public Encoding? DefaultEncoding { get; set; }

        public bool DryRun { get; set; }

        public RenameOptions? RenameOptions { get; set; }

        public IProgress<OperationProgress>? Progress { get; set; }

        public IDialogProvider<ConflictInfo>? DialogProvider { get; set; }

        public RenameOperation WithDirectoryFilter(NameFilter directoryFilter)
        {
            DirectoryFilters = new NameFilter[] { directoryFilter };
            return this;
        }

        public RenameOperation WithDirectoryFilters(IEnumerable<NameFilter> directoryFilters)
        {
            if (directoryFilters is null)
                throw new ArgumentNullException(nameof(directoryFilters));

            DirectoryFilters = directoryFilters.ToArray();
            return this;
        }

        public RenameOperation WithSearchTarget(SearchTarget searchTarget)
        {
            SearchTarget = searchTarget;
            return this;
        }

        public RenameOperation WithTopDirectoryOnly()
        {
            RecurseSubdirectories = false;
            return this;
        }

        public RenameOperation WithDefaultEncoding(Encoding encoding)
        {
            DefaultEncoding = encoding;
            return this;
        }

        public RenameOperation WithDryRun()
        {
            DryRun = true;
            return this;
        }

        public RenameOperation WithRenameOptions(RenameOptions renameOptions)
        {
            RenameOptions = renameOptions;
            return this;
        }

        public RenameOperation WithProgress(IProgress<OperationProgress> progress)
        {
            Progress = progress;
            return this;
        }

        public RenameOperation WithDialogProvider(IDialogProvider<ConflictInfo> dialogProvider)
        {
            DialogProvider = dialogProvider;
            return this;
        }

        public OperationResult Execute(CancellationToken cancellationToken = default)
        {
            RenameOptions renameOptions = RenameOptions ?? new RenameOptions(replacement: "");

            OperationHelpers.VerifyConflictResolution(renameOptions.ConflictResolution, DialogProvider);

            var search = new FileSystemSearch(
                Filter,
                directoryFilters: DirectoryFilters?.ToImmutableArray() ?? ImmutableArray<NameFilter>.Empty,
                progress: null,
                searchTarget: SearchTarget,
                recurseSubdirectories: RecurseSubdirectories,
                defaultEncoding: DefaultEncoding)
            {
                DisallowEnumeration = !DryRun,
                MatchPartOnly = true
            };

            var command = new RenameCommand()
            {
                Search = search,
                RenameOptions = renameOptions,
                Progress = Progress,
                DryRun = DryRun,
                CancellationToken = cancellationToken,
                DialogProvider = DialogProvider,
                MaxMatchingFiles = 0,
            };

            command.Execute(DirectoryPath);

            return new OperationResult(command.Telemetry);
        }
    }
}
