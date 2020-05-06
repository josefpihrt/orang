// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.FileSystem
{
    internal static class FileSystemExtensions
    {
        public static void Report(this IProgress<FileSystemFilterProgress> progress, string path, ProgressKind kind, Exception ex = null)
        {
            progress.Report(new FileSystemFilterProgress(path, kind, ex));
        }

        public static bool IsFileMatch(this Filter filter, string path, NamePartKind namePartKind)
        {
            NamePart part = NamePart.FromFile(path, namePartKind);

            return IsMatch(filter, part);
        }

        public static bool IsDirectoryMatch(this Filter filter, string path, NamePartKind namePartKind)
        {
            NamePart part = NamePart.FromDirectory(path, namePartKind);

            return IsMatch(filter, part);
        }

        private static bool IsMatch(this Filter filter, in NamePart part)
        {
            return filter.IsMatch(filter.Regex.Match(part.Path, part.Index, part.Length));
        }
    }
}
