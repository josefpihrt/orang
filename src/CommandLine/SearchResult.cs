// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.IO;
using Orang.FileSystem;

namespace Orang.CommandLine
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal class SearchResult
    {
        private FileSystemInfo _fileSystemInfo;
        private long? _size;

        public SearchResult(
            FileSystemFinderResult result,
            string baseDirectoryPath)
        {
            Result = result;
            BaseDirectoryPath = baseDirectoryPath;
        }

        public FileSystemFinderResult Result { get; }

        public string BaseDirectoryPath { get; }

        public bool IsDirectory => Result.IsDirectory;

        public string Path => Result.Path;

        public FileSystemInfo FileSystemInfo
        {
            get
            {
                if (_fileSystemInfo == null)
                {
                    if (IsDirectory)
                    {
                        _fileSystemInfo = new DirectoryInfo(Path);
                    }
                    else
                    {
                        _fileSystemInfo = new FileInfo(Path);
                    }
                }

                return _fileSystemInfo;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Path}";

        public long GetSize()
        {
            return _size ?? (_size = (IsDirectory)
                ? FileSystemHelpers.GetDirectorySize(Path)
                : ((FileInfo)FileSystemInfo).Length).Value;
        }
    }
}
