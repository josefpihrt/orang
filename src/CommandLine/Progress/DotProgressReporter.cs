// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using static Orang.Logger;

namespace Orang.FileSystem
{
    internal class DotProgressReporter : ProgressReporter
    {
        public override void Report(FileSystemFinderProgress value)
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
                        WriteProgress();
                        break;
                    }
                case ProgressKind.File:
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
            if ((FileCount + DirectoryCount) % 100 == 0)
            {
                ConsoleOut.Write(".", Colors.Path_Progress);
                ProgressReported = true;
            }
        }
    }
}
