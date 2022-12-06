// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Orang.FileSystem.Commands;

internal abstract class CommonCopyCommand : CommonFindCommand
{
    protected CommonCopyCommand()
    {
    }

    public string DestinationPath { get; set; } = null!;

    public IDialogProvider<ConflictInfo>? DialogProvider { get; set; }

    public CopyOptions CopyOptions { get; set; } = null!;

    public ConflictResolution ConflictResolution { get; set; }

    protected override void OnSearchStateCreating(SearchState search)
    {
        search.ExcludeDirectory = DirectoryPredicate.Create(search.ExcludeDirectory, DestinationPath);
    }

    protected abstract void ExecuteOperation(string sourcePath, string destinationPath);

    protected override void ExecuteMatch(
        FileMatch fileMatch,
        string directoryPath)
    {
        string sourcePath = fileMatch.Path;
        string destinationPath;

        if (fileMatch.IsDirectory
            || (directoryPath is not null && !CopyOptions.Flat))
        {
            Debug.Assert(sourcePath.StartsWith(directoryPath, FileSystemHelpers.Comparison));

            string relativePath = sourcePath.Substring(directoryPath.Length + 1);

            destinationPath = Path.Combine(DestinationPath, relativePath);
        }
        else
        {
            string fileName = Path.GetFileName(sourcePath);

            destinationPath = Path.Combine(DestinationPath, fileName);
        }

        try
        {
            ExecuteOperation(fileMatch, fileMatch.Path, destinationPath, fileMatch.IsDirectory);
        }
        catch (Exception ex) when (ex is IOException
            || ex is UnauthorizedAccessException)
        {
            Log(fileMatch, destinationPath, ex);
        }
    }

    private DialogResult? ExecuteOperation(FileMatch? fileMatch, string sourcePath, string destinationPath, bool isDirectory)
    {
        bool fileExists = File.Exists(destinationPath);
        bool directoryExists = !fileExists && Directory.Exists(destinationPath);
        var ask = false;

        if (isDirectory)
        {
            if (fileExists)
            {
                ask = true;
            }
            else if (directoryExists)
            {
                if (CopyOptions.StructureOnly
                    && File.GetAttributes(sourcePath) == File.GetAttributes(destinationPath))
                {
                    return null;
                }
            }
        }
        else if (fileExists)
        {
            if (CopyOptions.CompareOptions != FileCompareOptions.None
                && FileSystemHelpers.FileEquals(
                    sourcePath,
                    destinationPath,
                    CopyOptions.CompareOptions,
                    CopyOptions.NoCompareAttributes,
                    CopyOptions.AllowedTimeDiff))
            {
                return null;
            }

            ask = true;
        }
        else if (directoryExists)
        {
            ask = true;
        }

        if (ask
            && ConflictResolution == ConflictResolution.Skip)
        {
            return null;
        }

        if (ConflictResolution == ConflictResolution.Suffix
            && ((isDirectory && directoryExists)
                || (!isDirectory && fileExists)))
        {
            destinationPath = FileSystemHelpers.CreateNewFilePath(destinationPath);
        }

        if (ask
            && ConflictResolution == ConflictResolution.Ask)
        {
            DialogResult dialogResult = DialogProvider!.GetResult(
                new ConflictInfo(sourcePath, destinationPath));

            switch (dialogResult)
            {
                case DialogResult.Yes:
                    {
                        break;
                    }
                case DialogResult.YesToAll:
                    {
                        ConflictResolution = ConflictResolution.Overwrite;
                        break;
                    }
                case DialogResult.No:
                case DialogResult.None:
                    {
                        return null;
                    }
                case DialogResult.NoToAll:
                    {
                        ConflictResolution = ConflictResolution.Skip;
                        return DialogResult.NoToAll;
                    }
                case DialogResult.Cancel:
                    {
                        throw new OperationCanceledException();
                    }
                default:
                    {
                        throw new InvalidOperationException($"Unknown enum value '{dialogResult}'.");
                    }
            }
        }

        if (fileMatch is not null)
            Log(fileMatch, destinationPath);

        if (isDirectory)
        {
            if (directoryExists)
            {
                if (!DryRun)
                {
                    if (CopyOptions.StructureOnly)
                    {
                        FileSystemHelpers.UpdateAttributes(sourcePath, destinationPath);
                    }
                    else
                    {
                        CopyDirectory(sourcePath, destinationPath);
                    }
                }
            }
            else
            {
                if (fileExists
                    && !DryRun)
                {
                    File.Delete(destinationPath);
                }

                if (!DryRun)
                {
                    if (CopyOptions.StructureOnly)
                    {
                        Directory.CreateDirectory(destinationPath);
                    }
                    else
                    {
                        CopyDirectory(sourcePath, destinationPath);
                    }
                }
            }

            Telemetry.ProcessedDirectoryCount++;
        }
        else
        {
            if (!DryRun)
            {
                if (fileExists)
                {
                    File.Delete(destinationPath);
                }
                else if (directoryExists)
                {
                    Directory.Delete(destinationPath, recursive: true);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                }

                ExecuteOperation(sourcePath, destinationPath);
            }

            Telemetry.ProcessedFileCount++;
        }

        return null;
    }

    private void CopyDirectory(
        string sourcePath,
        string destinationPath)
    {
        var directories = new Stack<(string, string)>();

        directories.Push((sourcePath, destinationPath));

        CopyDirectory(directories);
    }

    private void CopyDirectory(Stack<(string, string)> directories)
    {
        while (directories.Count > 0)
        {
            (string sourcePath, string destinationPath) = directories.Pop();

            var isEmpty = true;

            using (IEnumerator<string> en = Directory.EnumerateFiles(sourcePath).GetEnumerator())
            {
                if (en.MoveNext())
                {
                    isEmpty = false;

                    do
                    {
                        string newFilePath = Path.Combine(destinationPath, Path.GetFileName(en.Current));

                        DialogResult? result = ExecuteOperation(
                            null,
                            en.Current,
                            newFilePath,
                            isDirectory: false);

                        if (result == DialogResult.NoToAll
                            || result == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    while (en.MoveNext());
                }
            }

            using (IEnumerator<string> en = Directory.EnumerateDirectories(sourcePath).GetEnumerator())
            {
                if (en.MoveNext())
                {
                    isEmpty = false;

                    do
                    {
                        string newDirectoryPath = Path.Combine(destinationPath, Path.GetFileName(en.Current));

                        directories.Push((en.Current, newDirectoryPath));
                    }
                    while (en.MoveNext());
                }
            }

            if (isEmpty
                && !DryRun)
            {
                Directory.CreateDirectory(destinationPath);
            }
        }
    }
}
