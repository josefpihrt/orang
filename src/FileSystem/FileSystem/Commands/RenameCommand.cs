// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Orang.Text.RegularExpressions;

namespace Orang.FileSystem.Commands;

internal class RenameCommand : DeleteOrRenameCommand
{
    public RenameOptions RenameOptions { get; set; } = null!;

    public Replacer Replacer { get; set; } = null!;

    public ConflictResolution ConflictResolution { get; set; }

    public IDialogProvider<ConflictInfo>? DialogProvider { get; set; }

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
            Replacer,
            count: 0,
            predicate: NameFilter!.Predicate,
            cancellationToken: CancellationToken);

        string path = fileMatch.Path;

        string newName = ReplaceHelpers.GetNewName(fileMatch, replaceItems);

        if (string.IsNullOrWhiteSpace(newName))
        {
            Report(fileMatch, null, new IOException("New file name cannot be empty or contains only white-space."));
            return;
        }

        bool changed = string.Compare(
            path,
            fileMatch.NameSpan.Start,
            newName,
            0,
            Math.Max(fileMatch.NameSpan.Length, newName.Length),
            StringComparison.Ordinal) != 0;

        bool isInvalidName = changed
            && FileSystemHelpers.ContainsInvalidFileNameChars(newName);

        string newPath = path.Substring(0, fileMatch.NameSpan.Start) + newName;

        ListCache<ReplaceItem>.Free(replaceItems);

        if (!changed)
            return;

        if (isInvalidName)
        {
            Report(fileMatch, newName, new IOException("New file name contains invalid characters."));
            return;
        }

        if (File.Exists(newPath)
            || Directory.Exists(newPath))
        {
            if (ConflictResolution == ConflictResolution.Skip)
                return;

            if (ConflictResolution == ConflictResolution.Ask
                && !DryRun
                && !AskToOverwrite(fileMatch, newPath))
            {
                return;
            }
        }

        var renamed = false;

        try
        {
            if (!DryRun)
            {
                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }
                else if (Directory.Exists(newPath))
                {
                    Directory.Delete(newPath, recursive: true);
                }

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

            if (fileMatch.IsDirectory)
            {
                Telemetry.ProcessedDirectoryCount++;
            }
            else
            {
                Telemetry.ProcessedFileCount++;
            }

            Report(fileMatch, newPath);
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
        DialogResult result = DialogProvider!.GetResult(new ConflictInfo(fileMatch.Path, newPath));

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
