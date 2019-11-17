// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Orang.CommandLine;
using static Orang.Logger;

namespace Orang.FileSystem
{
    internal class FileSystemFinderProgressReporter : IProgress<FileSystemFinderProgress>
    {
        public FileSystemFinderProgressReporter(
            string baseDirectoryPath,
            ProgressReportMode consoleReportMode,
            ProgressReportMode fileLogReportMode,
            CommonFindCommandOptions options)
        {
            BaseDirectoryPath = baseDirectoryPath;
            ConsoleReportMode = consoleReportMode;
            FileLogReportMode = fileLogReportMode;
            Options = options;
        }

        public string BaseDirectoryPath { get; }

        public ProgressReportMode ConsoleReportMode { get; }

        public ProgressReportMode FileLogReportMode { get; }

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

                        if (ConsoleReportMode == ProgressReportMode.Dot)
                        {
                            if (SearchedDirectoryCount % 10 == 0)
                            {
                                ConsoleOut.Write(".", Colors.Path_Progress);
                                ProgressReported = true;
                            }

                            if (FileLogReportMode == ProgressReportMode.Path)
                                WritePath(value);
                        }
                        else if (ConsoleReportMode == ProgressReportMode.Path)
                        {
                            WritePath(value, verbosity: Verbosity.Diagnostic);
                        }
                        else if (FileLogReportMode == ProgressReportMode.Path)
                        {
                            WritePath(value);
                        }

                        break;
                    }
                case ProgressKind.Directory:
                    {
                        DirectoryCount++;
                        WritePath(value, indent: Options.Indent, verbosity: Verbosity.Diagnostic);
                        break;
                    }
                case ProgressKind.File:
                    {
                        FileCount++;
                        WritePath(value, indent: Options.Indent, verbosity: Verbosity.Diagnostic);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException($"Unknown enum value '{value.Kind}'.");
                    }
            }
        }

        private void WritePath(in FileSystemFinderProgress value)
        {
            Out.WritePath(value.Path, BaseDirectoryPath, relativePath: Options.PathDisplayStyle == PathDisplayStyle.Relative, verbosity: Verbosity.Diagnostic);
            Out.WriteLine(Verbosity.Diagnostic);
        }

        private void WritePath(in FileSystemFinderProgress value, string indent = null, Verbosity verbosity = Verbosity.Quiet)
        {
            LogHelpers.WritePath(
                value.Path,
                BaseDirectoryPath,
                relativePath: Options.PathDisplayStyle == PathDisplayStyle.Relative,
                colors: Colors.Path_Progress,
                indent: indent,
                verbosity: verbosity);

            WriteLine(verbosity);
        }
    }
}
