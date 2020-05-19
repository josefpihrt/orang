// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.FileSystem
{
    public readonly struct SearchProgress
    {
        internal SearchProgress(string path, SearchProgressKind kind, bool isDirectory, Exception exception = null)
        {
            Path = path;
            Kind = kind;
            Exception = exception;
            IsDirectory = isDirectory;
        }

        public string Path { get; }

        public SearchProgressKind Kind { get; }

        public Exception Exception { get; }

        public bool IsDirectory { get; }
    }
}
