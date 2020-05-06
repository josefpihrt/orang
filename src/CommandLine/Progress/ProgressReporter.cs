// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Orang.CommandLine;

namespace Orang.FileSystem
{
    internal class ProgressReporter : IProgress<FileSystemFilterProgress>
    {
        public ProgressReporter(string indent)
        {
            Indent = indent;
        }

        public string Indent { get; }

        public string BaseDirectoryPath { get; private set; }

        public int SearchedDirectoryCount { get; protected set; }

        public int DirectoryCount { get; protected set; }

        public int FileCount { get; protected set; }

        public bool ProgressReported { get; set; }

        public void SetBaseDirectoryPath(string baseDirectoryPath)
        {
            BaseDirectoryPath = baseDirectoryPath;
        }

        public virtual void Report(FileSystemFilterProgress value)
        {
            if (value.Error != null)
            {
                WriteError(value);
                return;
            }

            switch (value.Kind)
            {
                case ProgressKind.SearchedDirectory:
                    {
                        SearchedDirectoryCount++;
                        break;
                    }
                case ProgressKind.Directory:
                    {
                        DirectoryCount++;
                        break;
                    }
                case ProgressKind.File:
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

        protected void WriteError(FileSystemFilterProgress value)
        {
            LogHelpers.WriteFileError(
                value.Error,
                value.Path,
                basePath: BaseDirectoryPath,
                colors: Colors.Message_Warning,
                indent: Indent,
                verbosity: Verbosity.Detailed);
        }
    }
}
