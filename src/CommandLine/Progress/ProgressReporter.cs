// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.FileSystem;

internal class ProgressReporter : IProgress<SearchProgress>
{
    protected readonly Logger _logger;

    public ProgressReporter(string indent, Logger logger)
    {
        Indent = indent;
        _logger = logger;
    }

    public string Indent { get; }

    public string? BaseDirectoryPath { get; private set; }

    public int SearchedDirectoryCount { get; protected set; }

    public int DirectoryCount { get; protected set; }

    public int FileCount { get; protected set; }

    public bool ProgressReported { get; set; }

    public void SetBaseDirectoryPath(string? baseDirectoryPath)
    {
        BaseDirectoryPath = baseDirectoryPath;
    }

    public virtual void Report(SearchProgress value)
    {
        if (value.Exception is not null)
        {
            WriteError(value);
            return;
        }

        switch (value.Kind)
        {
            case SearchProgressKind.SearchDirectory:
                {
                    SearchedDirectoryCount++;
                    break;
                }
            case SearchProgressKind.Directory:
                {
                    DirectoryCount++;
                    break;
                }
            case SearchProgressKind.File:
                {
                    FileCount++;
                    break;
                }
            default:
                {
                    throw new InvalidOperationException($"Unknown enum value '{value.Kind}'.");
                }
        }
    }

    protected void WriteError(SearchProgress value)
    {
        _logger.WriteFileError(
            value.Exception!,
            value.Path,
            indent: Indent,
            verbosity: Verbosity.Detailed);
    }
}
