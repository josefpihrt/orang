// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Orang.FileSystem;

namespace Orang.CommandLine
{
    internal class FilePropertyFilter
    {
        public FilePropertyFilter(
            FilterPredicate<long> sizePredicate = null,
            FilterPredicate<DateTime> creationTimePredicate = null,
            FilterPredicate<DateTime> modifiedTimePredicate = null)
        {
            SizePredicate = sizePredicate;
            CreationTimePredicate = creationTimePredicate;
            ModifiedTimePredicate = modifiedTimePredicate;
        }

        public FilterPredicate<long> SizePredicate { get; }

        public FilterPredicate<DateTime> CreationTimePredicate { get; }

        public FilterPredicate<DateTime> ModifiedTimePredicate { get; }

        internal bool IsDefault
        {
            get
            {
                return SizePredicate == null
                    && CreationTimePredicate == null
                    && ModifiedTimePredicate == null;
            }
        }

        public bool IsMatch(FileSystemFinderResult result)
        {
            return IsMatch(result.Path, result.IsDirectory);
        }

        public bool IsMatch(string path, bool isDirectory = false)
        {
            if (!isDirectory
                && SizePredicate?.Predicate.Invoke(new FileInfo(path).Length) == false)
            {
                return false;
            }

            if (CreationTimePredicate?.Predicate.Invoke(File.GetCreationTime(path)) == false)
                return false;

            if (ModifiedTimePredicate?.Predicate.Invoke(File.GetLastWriteTime(path)) == false)
                return false;

            return true;
        }
    }
}
