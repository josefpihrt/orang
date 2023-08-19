// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Orang.FileSystem.Commands;

internal class DeleteCommand : DeleteOrRenameCommand
{
    public DeleteOptions DeleteOptions { get; set; } = null!;

    public override OperationKind OperationKind => OperationKind.Delete;

    protected override void OnSearchStateCreating(SearchState search)
    {
        search.CanRecurseMatch = false;
    }

    protected override void ExecuteMatch(
        FileMatch fileMatch,
        string directoryPath)
    {
        try
        {
            if (!DryRun)
            {
                FileSystemUtilities.Delete(
                    fileMatch,
                    contentOnly: DeleteOptions.ContentOnly,
                    includingBom: DeleteOptions.IncludingBom,
                    filesOnly: DeleteOptions.FilesOnly,
                    directoriesOnly: DeleteOptions.DirectoriesOnly);
            }

            Log(fileMatch);

            Telemetry.IncrementProcessedCount(fileMatch.IsDirectory);
        }
        catch (Exception ex) when (ex is IOException
            || ex is UnauthorizedAccessException)
        {
            Log(fileMatch, ex);
        }
    }
}
