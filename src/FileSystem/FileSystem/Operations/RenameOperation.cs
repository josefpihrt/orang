// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Orang.Text.RegularExpressions;

namespace Orang.FileSystem.Operations
{
    internal class RenameOperation : DeleteOrRenameOperation
    {
        public override OperationKind OperationKind => OperationKind.Rename;

        public RenameOptions RenameOptions { get; set; } = null!;

        public ConflictResolution ConflictResolution { get; set; }

        public IDialogProvider<OperationProgress>? DialogProvider { get; set; }

        protected override void ExecuteDirectory(string directoryPath)
        {
            Debug.Assert(!NameFilter!.IsNegative);

            base.ExecuteDirectory(directoryPath);
        }

        protected override void ExecuteMatch(
            FileMatch fileMatch,
            string directoryPath)
        {
            (List<ReplaceItem> replaceItems, MaxReason maxReason) = ReplaceHelpers.GetReplaceItems(
                fileMatch.NameMatch!,
                RenameOptions,
                predicate: NameFilter?.Predicate,
                cancellationToken: CancellationToken);

            string path = fileMatch.Path;

            string newName = ReplaceHelpers.GetNewName(fileMatch, replaceItems);

            ListCache<ReplaceItem>.Free(replaceItems);

            if (string.IsNullOrWhiteSpace(newName))
            {
                Report(fileMatch, null, new IOException("New file name cannot be empty or contains only white-space."));
                return;
            }

            if (string.Compare(
                path,
                fileMatch.NameSpan.Start,
                newName,
                0,
                fileMatch.NameSpan.Length,
                StringComparison.Ordinal) == 0)
            {
                return;
            }

            if (FileSystemHelpers.ContainsInvalidFileNameChars(newName))
            {
                Report(fileMatch, newName, new IOException("New file name contains invalid characters."));
                return;
            }

            string newPath = path.Substring(0, fileMatch.NameSpan.Start) + newName;

            if (File.Exists(newPath))
            {
                if (ConflictResolution == ConflictResolution.Skip)
                {
                    return;
                }
                else if (ConflictResolution == ConflictResolution.Suffix)
                {
                    newPath = FileSystemHelpers.CreateNewFilePath(newPath);
                }
                else if (ConflictResolution == ConflictResolution.Ask)
                {
                    if (!AskToOverwrite(fileMatch, newPath))
                        return;
                }
            }

            var renamed = false;

            try
            {
                if (!DryRun)
                {
                    if (fileMatch.IsDirectory)
                    {
                        Directory.Move(path, newPath);
                    }
                    else
                    {
                        File.Move(path, newPath);
                    }

                    renamed = true;
                }

                Report(fileMatch, newPath);

                Telemetry.IncrementProcessedCount(fileMatch.IsDirectory);
            }
            catch (Exception ex) when (ex is IOException
                || ex is UnauthorizedAccessException)
            {
                Report(fileMatch, newPath, ex);
            }

            if (fileMatch.IsDirectory
                && renamed)
            {
                OnDirectoryChanged(new DirectoryChangedEventArgs(path, newPath));
            }
        }

        private bool AskToOverwrite(FileMatch fileMatch, string newPath)
        {
            DialogResult result = DialogProvider!.GetResult(new OperationProgress(fileMatch, newPath, OperationKind));

            switch (result)
            {
                case DialogResult.Yes:
                    {
                        return true;
                    }
                case DialogResult.YesToAll:
                    {
                        DialogProvider = null;
                        return true;
                    }
                case DialogResult.No:
                case DialogResult.None:
                    {
                        return false;
                    }
                case DialogResult.NoToAll:
                    {
                        ConflictResolution = ConflictResolution.Skip;
                        return false;
                    }
                case DialogResult.Cancel:
                    {
                        throw new OperationCanceledException();
                    }
                default:
                    {
                        throw new InvalidOperationException($"Unknown enum value '{result}'.");
                    }
            }
        }
    }
}
