// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Orang.FileSystem;

namespace Orang.CommandLine;

internal class CopyCommand : CommonCopyCommand<CopyCommandOptions>
{
    public CopyCommand(CopyCommandOptions options, Logger logger) : base(options, logger)
    {
    }

    protected override string GetQuestionText(bool isDirectory)
    {
        return (isDirectory) ? "Copy directory?" : "Copy file?";
    }

    protected override void ExecuteOperation(string sourcePath, string destinationPath)
    {
        File.Copy(sourcePath, destinationPath);
    }

    protected override void WriteSummary(SearchTelemetry telemetry, Verbosity verbosity)
    {
        base.WriteSummary(telemetry, verbosity);

        ConsoleColors colors = (Options.DryRun) ? Colors.Message_DryRun : Colors.Message_Change;

        if (telemetry.ProcessedFileCount > 0)
            _logger.WriteCount("Copied files", telemetry.ProcessedFileCount, colors, verbosity);

        if (telemetry.ProcessedDirectoryCount > 0)
        {
            if (telemetry.ProcessedFileCount > 0)
                _logger.Write("  ", verbosity);

            _logger.WriteCount("Copied directories", telemetry.ProcessedDirectoryCount, colors, verbosity);
        }

        _logger.WriteLine(verbosity);
    }
}
