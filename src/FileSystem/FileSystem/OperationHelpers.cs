// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using static Orang.FileSystem.FileSystemHelpers;

namespace Orang.FileSystem;

internal static class OperationHelpers
{
    public static void VerifyConflictResolution(
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

    public static void VerifyCopyMoveArguments(
        string directoryPath,
        string destinationPath,
        CopyOptions copyOptions,
        IDialogProvider<ConflictInfo>? dialogProvider)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (destinationPath is null)
            throw new ArgumentNullException(nameof(destinationPath));

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        if (!Directory.Exists(destinationPath))
            throw new DirectoryNotFoundException($"Directory not found: {destinationPath}");

        if (IsSubdirectory(destinationPath, directoryPath))
        {
            throw new ArgumentException(
                "Source directory cannot be subdirectory of a destination directory.",
                nameof(directoryPath));
        }

        VerifyConflictResolution(copyOptions.ConflictResolution, dialogProvider);
    }
}
