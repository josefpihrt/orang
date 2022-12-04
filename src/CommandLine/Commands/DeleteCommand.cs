// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Orang.FileSystem;

namespace Orang.CommandLine;

internal class DeleteCommand : DeleteOrRenameCommand<DeleteCommandOptions>
{
    public DeleteCommand(DeleteCommandOptions options, Logger logger) : base(options, logger)
    {
    }

    protected override void OnSearchCreating(SearchState search)
    {
        search.CanRecurseMatch = false;
    }

    protected override void ExecuteMatchCore(
        FileMatch fileMatch,
        SearchContext context,
        string? baseDirectoryPath = null,
        ColumnWidths? columnWidths = null)
    {
        string indent = GetPathIndent(baseDirectoryPath);

        if (!Options.OmitPath)
            WritePath(context, fileMatch, baseDirectoryPath, indent, columnWidths, includeNewline: true);

        if (Options.AskMode == AskMode.File
            && !AskToExecute(context, (Options.ContentOnly) ? "Delete content?" : "Delete?", indent))
        {
            return;
        }

        try
        {
            if (!Options.DryRun)
            {
                FileSystemHelpers.Delete(
                    fileMatch,
                    contentOnly: Options.ContentOnly,
                    includingBom: Options.IncludingBom,
                    filesOnly: Options.FilesOnly,
                    directoriesOnly: Options.DirectoriesOnly);
            }

            if (fileMatch.IsDirectory)
            {
                context.Telemetry.ProcessedDirectoryCount++;
            }
            else
            {
                context.Telemetry.ProcessedFileCount++;
            }
        }
        catch (Exception ex) when (ex is IOException
            || ex is UnauthorizedAccessException)
        {
            _logger.WriteFileError(ex, indent: indent);
        }
    }

    protected override void WriteSummary(SearchTelemetry telemetry, Verbosity verbosity)
    {
        _logger.WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget, verbosity);

        string filesTitle = (Options.ContentOnly)
            ? "Deleted files content"
            : "Deleted files";

        string directoriesTitle = (Options.ContentOnly)
            ? "Deleted directories (content only)"
            : "Deleted directories";

        _logger.WriteProcessedFilesAndDirectories(
            telemetry,
            Options.SearchTarget,
            filesTitle,
            directoriesTitle,
            Options.DryRun,
            verbosity);
    }
}
