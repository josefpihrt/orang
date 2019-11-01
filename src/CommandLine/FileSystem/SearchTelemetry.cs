// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.FileSystem
{
    public class SearchTelemetry
    {
        internal SearchTelemetry()
        {
        }

        public int SearchedDirectoryCount { get; internal set; }

        public int FileCount { get; internal set; }

        public int DirectoryCount { get; internal set; }

        public int MatchingFileCount { get; internal set; }

        public int MatchingDirectoryCount { get; internal set; }

        public int ProcessedFileCount { get; internal set; }

        public int ProcessedDirectoryCount { get; internal set; }

        public int MatchCount { get; internal set; }

        public int ProcessedMatchCount { get; internal set; }

        public int MatchingLineCount { get; internal set; }

        public TimeSpan Elapsed { get; internal set; }
    }
}
