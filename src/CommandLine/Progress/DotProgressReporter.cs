﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Orang.CommandLine;

namespace Orang.FileSystem;

internal class DotProgressReporter : ProgressReporter
{
    public DotProgressReporter(string indent, Logger logger) : base(indent, logger)
    {
    }

    public override void Report(SearchProgress value)
    {
        if (value.Exception is not null)
            return;

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
                    WriteProgress();
                    break;
                }
            case SearchProgressKind.File:
                {
                    FileCount++;
                    WriteProgress();
                    break;
                }
            default:
                {
                    throw new InvalidOperationException($"Unknown enum value '{value.Kind}'.");
                }
        }
    }

    protected void WriteProgress()
    {
        if ((FileCount + DirectoryCount) % ApplicationOptions.Default.ProgressCount == 0)
        {
            _logger.ConsoleOut.Write(".", Colors.Path_Progress);
            ProgressReported = true;
        }
    }
}
