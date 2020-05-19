// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.FileSystem
{
    public readonly struct OperationProgress
    {
        internal OperationProgress(FileMatch fileMatch, OperationKind kind, Exception exception = null)
            : this(fileMatch, null, kind, exception)
        {
        }

        internal OperationProgress(FileMatch fileMatch, string newPath, OperationKind kind, Exception exception = null)
        {
            FileMatch = fileMatch;
            NewPath = newPath;
            Kind = kind;
            Exception = exception;
        }

        public FileMatch FileMatch { get; }

        public string NewPath { get; }

        public OperationKind Kind { get; }

        public Exception Exception { get; }
    }
}
