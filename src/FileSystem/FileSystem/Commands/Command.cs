// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Orang.FileSystem.Commands;

internal abstract class Command
{
    protected Command()
    {
        Telemetry = new SearchTelemetry();
    }

    public FileSystemSearch Search { get; set; } = null!;

    public SearchTelemetry Telemetry { get; }

    public TerminationReason TerminationReason { get; set; }

    public int MaxMatchingFiles { get; set; }

    public IProgress<OperationProgress>? Progress { get; set; }

    public bool DryRun { get; set; }

    public CancellationToken CancellationToken { get; set; }

    protected Filter? ContentFilter => Search.Filter.Content;

    protected Filter? NameFilter => Search.Filter.Name;

    protected abstract void ExecuteMatch(FileMatch fileMatch, string directoryPath);

    public void Execute(string directoryPath)
    {
        directoryPath = FileSystemHelpers.EnsureFullPath(directoryPath);

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        ExecuteDirectory(directoryPath);
    }

    protected virtual void ExecuteDirectory(string directoryPath)
    {
        foreach (FileMatch fileMatch in GetMatches(directoryPath))
        {
            Telemetry.IncrementMatchingCount(fileMatch.IsDirectory);

            ExecuteMatch(fileMatch, directoryPath);

            if (MaxMatchingFiles == Telemetry.MatchingFileDirectoryCount)
            {
                TerminationReason = TerminationReason.MaxReached;
                break;
            }
        }
    }

    protected IEnumerable<FileMatch> GetMatches(string directoryPath)
    {
        return Search.Find(
            directoryPath: directoryPath,
            notifyDirectoryChanged: this as INotifyDirectoryChanged,
            cancellationToken: CancellationToken);
    }

    protected void Report(FileMatch fileMatch, Exception? exception = null)
    {
        Report(fileMatch, newPath: null, exception);
    }

    protected void Report(FileMatch fileMatch, string? newPath, Exception? exception = null)
    {
        Progress?.Report(fileMatch, newPath, exception);
    }
}
