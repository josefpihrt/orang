// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Orang.CommandLine;
using static Orang.CommandLine.LogHelpers;
using static Orang.Logger;

namespace Orang.FileSystem
{
    internal class FileSystemFinderProgressReporter : IProgress<FileSystemFinderProgress>
    {
        public FileSystemFinderProgressReporter(
            string baseDirectoryPath,
            ProgressReporterMode mode,
            CommonFindCommandOptions options)
        {
            BaseDirectoryPath = baseDirectoryPath;
            Mode = mode;
            Options = options;
        }

        public string BaseDirectoryPath { get; }

        public ProgressReporterMode Mode { get; }

        public CommonFindCommandOptions Options { get; }

        public int SearchedDirectoryCount { get; private set; }

        public int DirectoryCount { get; private set; }

        public int FileCount { get; private set; }

        public bool ProgressReported { get; set; }

        public void Report(FileSystemFinderProgress value)
        {
            if (value.Error != null)
            {
                return;
            }

            switch (value.Kind)
            {
                case ProgressKind.SearchedDirectory:
                    {
                        SearchedDirectoryCount++;

                        if (Mode == ProgressReporterMode.Dot)
                        {
                            if (SearchedDirectoryCount % 10 == 0)
                            {
                                ConsoleOut.Write(".", Colors.Path_Progress);
                                ProgressReported = true;
                            }
                        }
                        else if (Mode == ProgressReporterMode.Path)
                        {
                            WritePath(value.Path, BaseDirectoryPath, colors: Colors.Path_Progress, verbosity: Verbosity.Detailed);
                            WriteLine(Verbosity.Detailed);
                        }

                        break;
                    }
                case ProgressKind.Directory:
                    {
                        DirectoryCount++;

                        WritePath(value.Path, BaseDirectoryPath, colors: Colors.Path_Progress, Options.Indent, Verbosity.Diagnostic);
                        WriteLine(Verbosity.Diagnostic);
                        break;
                    }
                case ProgressKind.File:
                    {
                        FileCount++;

                        WritePath(value.Path, BaseDirectoryPath, colors: Colors.Path_Progress, Options.Indent, Verbosity.Diagnostic);
                        WriteLine(Verbosity.Diagnostic);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException($"Unknown enum value '{value.Kind}'.");
                    }
            }
        }
    }
}
