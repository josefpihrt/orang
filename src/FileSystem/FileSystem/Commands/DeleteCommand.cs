// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Orang.FileSystem.Commands;

internal class DeleteCommand : DeleteOrRenameCommand
{
    public DeleteOptions DeleteOptions { get; set; } = null!;

    public override OperationKind OperationKind => OperationKind.Delete;

    protected override void ExecuteMatch(
        FileMatch fileMatch,
        string directoryPath)
    {
        try
        {
            if (!DryRun)
            {
                FileSystemHelpers.Delete(
                    fileMatch,
                    contentOnly: DeleteOptions.ContentOnly,
                    includingBom: DeleteOptions.IncludingBom,
                    filesOnly: DeleteOptions.FilesOnly,
                    directoriesOnly: DeleteOptions.DirectoriesOnly);
            }

            Report(fileMatch);

            Telemetry.IncrementProcessedCount(fileMatch.IsDirectory);
        }
        catch (Exception ex) when (ex is IOException
            || ex is UnauthorizedAccessException)
        {
            Report(fileMatch, ex);
        }
    }
}
