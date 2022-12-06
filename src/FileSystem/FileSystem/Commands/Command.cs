// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;

namespace Orang.FileSystem.Commands;

internal abstract class Command
{
    protected Command()
    {
        Telemetry = new SearchTelemetry();
    }

    public abstract OperationKind OperationKind { get; }

    public SearchTelemetry Telemetry { get; }

    public TerminationReason TerminationReason { get; set; }

    public int MaxMatchingFiles { get; set; }

    public Action<OperationProgress>? LogOperation { get; set; }

    public bool DryRun { get; set; }

    public CancellationToken CancellationToken { get; set; }

    protected abstract void ExecuteMatch(FileMatch fileMatch, string directoryPath);

    protected virtual void OnSearchStateCreating(SearchState search)
    {
    }

    public void Execute(string directoryPath, Search search, CancellationToken cancellationToken)
    {
        directoryPath = FileSystemHelpers.EnsureFullPath(directoryPath);

        if (!System.IO.Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var state = new SearchState(search.Matcher)
        {
            IncludeDirectory = search.Options.IncludeDirectory,
            ExcludeDirectory = search.Options.ExcludeDirectory,
            LogProgress = search.Options.LogProgress,
            SearchTarget = search.Options.SearchTarget,
            RecurseSubdirectories = !search.Options.TopDirectoryOnly,
            DefaultEncoding = search.Options.DefaultEncoding,
        };

        OnSearchStateCreating(state);

        foreach (FileMatch fileMatch in state.Find(directoryPath, this as INotifyDirectoryChanged, cancellationToken))
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

    protected void Log(FileMatch fileMatch, Exception? exception = null)
    {
        Log(fileMatch, newPath: null, exception);
    }

    protected void Log(FileMatch fileMatch, string? newPath, Exception? exception = null)
    {
        LogOperation?.Invoke(new OperationProgress(fileMatch, newPath, OperationKind, exception));
    }
}
