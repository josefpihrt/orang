// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Orang.CommandLine;
using static Orang.Logger;

namespace Orang.FileSystem
{
    internal class FileSystemFinderProgressReporter : IProgress<FileSystemFinderProgress>
    {
        public FileSystemFinderProgressReporter(
            ProgressReportMode consoleReportMode,
            ProgressReportMode fileReportMode,
            CommonFindCommandOptions options)
        {
            ConsoleReportMode = consoleReportMode;
            FileReportMode = fileReportMode;
            Options = options;
        }

        public string BaseDirectoryPath { get; set; }

        public ProgressReportMode ConsoleReportMode { get; }

        public ProgressReportMode FileReportMode { get; }

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

                        if (ConsoleReportMode == ProgressReportMode.Path)
                        {
                            WritePath(value, verbosity: Verbosity.Diagnostic);
                        }
                        else if (FileReportMode == ProgressReportMode.Path)
                        {
                            WritePath(value, indent: null);
                        }

                        break;
                    }
                case ProgressKind.Directory:
                    {
                        DirectoryCount++;
                        WritePath(value);
                        break;
                    }
                case ProgressKind.File:
                    {
                        FileCount++;
                        WritePath(value);
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
            if (ConsoleReportMode == ProgressReportMode.Dot)
            {
                if ((FileCount + DirectoryCount) % 100 == 0)
                {
                    ConsoleOut.Write(".", Colors.Path_Progress);
                    ProgressReported = true;
                }

                if (FileReportMode == ProgressReportMode.Path)
                    WritePath(value, indent: GetIndent());
            }
            else if (ConsoleReportMode == ProgressReportMode.Path)
            {
                WritePath(value, indent: GetIndent(), verbosity: Verbosity.Diagnostic);
            }
            else if (FileReportMode == ProgressReportMode.Path)
            {
                WritePath(value, indent: GetIndent());
            }

            string GetIndent()
            {
                return (Options.DisplayRelativePath) ? Options.Indent : "";
            }
        }

        private void WritePath(in FileSystemFinderProgress value, string indent)
        {
            Out.WritePath(
                value.Path,
                BaseDirectoryPath,
                relativePath: Options.DisplayRelativePath,
                indent: indent,
                verbosity: Verbosity.Diagnostic);

            Out.WriteLine(Verbosity.Diagnostic);
        }

        private void WritePath(in FileSystemFinderProgress value, string indent = null, Verbosity verbosity = Verbosity.Quiet)
        {
            LogHelpers.WritePath(
                value.Path,
                BaseDirectoryPath,
                relativePath: Options.DisplayRelativePath,
                colors: Colors.Path_Progress,
                indent: indent,
                verbosity: verbosity);

            WriteLine(verbosity);
        }
    }
}
