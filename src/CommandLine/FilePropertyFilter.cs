// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Orang.FileSystem;

namespace Orang.CommandLine
{
    internal class FilePropertyFilter
    {
        public FilePropertyFilter(
            Func<long, bool> sizePredicate = null,
            Func<DateTime, bool> creationTimePredicate = null,
            Func<DateTime, bool> modifiedTimePredicate = null)
        {
            SizePredicate = sizePredicate;
            CreationTimePredicate = creationTimePredicate;
            ModifiedTimePredicate = modifiedTimePredicate;
        }

        public Func<long, bool> SizePredicate { get; }

        public Func<DateTime, bool> CreationTimePredicate { get; }

        public Func<DateTime, bool> ModifiedTimePredicate { get; }

        public bool IsMatch(FileSystemFinderResult result)
        {
            return IsMatch(result.Path, result.IsDirectory);
        }

        public bool IsMatch(string path, bool isDirectory = false)
        {
            if (!isDirectory
                && SizePredicate?.Invoke(new FileInfo(path).Length) == false)
            {
                return false;
            }

            if (CreationTimePredicate?.Invoke(File.GetCreationTime(path)) == false)
                return false;

            if (ModifiedTimePredicate?.Invoke(File.GetLastWriteTime(path)) == false)
                return false;

            return true;
        }
    }
}
