// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Orang.FileSystem.Commands;

internal class RenameCommand : DeleteOrRenameCommand
{
    public RenameOptions RenameOptions { get; set; } = null!;

    public Replacer Replacer { get; set; } = null!;

    public ConflictResolution ConflictResolution { get; set; }

    public IDialogProvider<ConflictInfo>? DialogProvider { get; set; }

    public Matcher? FileMatcher { get; set; }

    public Matcher? DirectoryMatcher { get; set; }

    public override OperationKind OperationKind => OperationKind.Rename;

    protected override void OnSearchStateCreating(SearchState search)
    {
        search.SupportsEnumeration = DryRun;
    }

    protected override void ExecuteMatch(
        FileMatch fileMatch,
        string directoryPath)
    {
        string newValue = fileMatch.GetReplacement(
            Replacer,
            count: 0,
            predicate: (fileMatch.IsDirectory) ? DirectoryMatcher!.Predicate : FileMatcher!.Predicate,
            cancellationToken: CancellationToken);

        if (string.IsNullOrWhiteSpace(newValue))
        {
            Log(fileMatch, null, new IOException("New file name cannot be empty or contains only white-space."));
            return;
        }

        string path = fileMatch.Path;

        bool changed = string.Compare(
            path,
            fileMatch.NameSpan.Start,
            newValue,
            0,
            Math.Max(fileMatch.NameSpan.Length, newValue.Length),
            StringComparison.Ordinal) != 0;

        bool isInvalidName = changed
            && FileSystemHelpers.ContainsInvalidFileNameChars(newValue);

        string newPath = path.Substring(0, fileMatch.NameSpan.Start)
            + newValue
            + path.Substring(fileMatch.NameSpan.Start + fileMatch.NameSpan.Length);

        if (!changed)
            return;

        if (isInvalidName)
        {
            Log(fileMatch, newValue, new IOException("New file name contains invalid characters."));
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

            Log(fileMatch, newPath);
        }
        catch (Exception ex) when (ex is IOException
            || ex is UnauthorizedAccessException)
        {
            Log(fileMatch, newPath, ex);
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
