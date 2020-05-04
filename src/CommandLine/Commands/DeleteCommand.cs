// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using Orang.FileSystem;
using static Orang.CommandLine.LogHelpers;

namespace Orang.CommandLine
{
    internal class DeleteCommand : DeleteOrRenameCommand<DeleteCommandOptions>
    {
        public DeleteCommand(DeleteCommandOptions options) : base(options)
        {
        }

        protected override void ProcessResult(
            FileSystemFinderResult result,
            SearchContext context,
            string baseDirectoryPath = null)
        {
            Debug.Assert(baseDirectoryPath == null || result.Path.StartsWith(baseDirectoryPath, FileSystemHelpers.Comparison), $"{baseDirectoryPath}\r\n{result.Path}");

            ExecuteOrAddResult(result, context, baseDirectoryPath);
        }

        protected override void ExecuteResult(
            FileSystemFinderResult result,
            SearchContext context,
            string baseDirectoryPath = null,
            ColumnWidths columnWidths = null)
        {
            string indent = GetPathIndent(baseDirectoryPath);

            if (!Options.OmitPath)
                WritePath(context, result, baseDirectoryPath, indent, columnWidths);

            bool deleted = false;

            if (!Options.Ask || AskToExecute(context, (Options.ContentOnly) ? "Delete content?" : "Delete?", indent))
            {
                try
                {
                    if (!Options.DryRun)
                    {
                        FileSystemHelpers.Delete(
                            result,
                            contentOnly: Options.ContentOnly,
                            includingBom: Options.IncludingBom,
                            filesOnly: Options.FilesOnly,
                            directoriesOnly: Options.DirectoriesOnly);

                        deleted = true;
                    }

                    if (result.IsDirectory)
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
                    WriteFileError(ex, indent: indent);
                }
            }

            if (result.IsDirectory
                && deleted)
            {
                OnDirectoryChanged(new DirectoryChangedEventArgs(result.Path, null));
            }
        }

        protected override void WriteSummary(SearchTelemetry telemetry, Verbosity verbosity)
        {
            WriteSearchedFilesAndDirectories(telemetry, Options.SearchTarget, verbosity);

            string filesTitle = (Options.ContentOnly)
                ? "Deleted files content"
                : "Deleted files";

            string directoriesTitle = (Options.ContentOnly)
                ? "Deleted directories content"
                : "Deleted directories";

            WriteProcessedFilesAndDirectories(telemetry, Options.SearchTarget, filesTitle, directoriesTitle, Options.DryRun, verbosity);
        }
    }
}
