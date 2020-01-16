// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.FileSystem
{
    internal class ProgressReporter : IProgress<FileSystemFinderProgress>
    {
        public string BaseDirectoryPath { get; private set; }

        public int SearchedDirectoryCount { get; protected set; }

        public int DirectoryCount { get; protected set; }

        public int FileCount { get; protected set; }

        public bool ProgressReported { get; set; }

        public void SetBaseDirectoryPath(string baseDirectoryPath)
        {
            BaseDirectoryPath = baseDirectoryPath;
        }

        public virtual void Report(FileSystemFinderProgress value)
        {
            if (value.Error != null)
                return;

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
    }
}
