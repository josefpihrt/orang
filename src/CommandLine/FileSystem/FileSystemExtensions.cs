// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.FileSystem
{
    internal static class FileSystemExtensions
    {
        public static void Report(this IProgress<FileSystemFinderProgress> progress, string path, ProgressKind kind, Exception ex = null)
        {
            progress.Report(new FileSystemFinderProgress(path, kind, ex));
        }
    }
}
