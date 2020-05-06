// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.FileSystem
{
    public readonly struct FileSystemFilterProgress
    {
        public FileSystemFilterProgress(string path, ProgressKind kind, Exception error = null)
        {
            Path = path;
            Kind = kind;
            Error = error;
        }

        public string Path { get; }

        public ProgressKind Kind { get; }

        public Exception Error { get; }
    }
}
