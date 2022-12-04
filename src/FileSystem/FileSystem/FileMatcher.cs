// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

#pragma warning disable RCS1223

namespace Orang.FileSystem;

public class FileMatcher
{
    public Matcher? Name { get; set; }

    public FileNamePart Part { get; set; }

    public Matcher? Extension { get; set; }

    public Matcher? Content { get; set; }

    public Func<DateTime, bool>? CreationTimePredicate { get; set; }

    public Func<DateTime, bool>? ModifiedTimePredicate { get; set; }

    public Func<long, bool>? SizePredicate { get; set; }

    public FileAttributes Attributes { get; set; }

    public FileAttributes AttributesToSkip { get; set; }

    public FileEmptyOption FileEmptyOption { get; set; }

    internal bool IsMatch(FileInfo fileInfo)
    {
        return SizePredicate?.Invoke(fileInfo.Length) != false
            && CreationTimePredicate?.Invoke(fileInfo.CreationTime) != false
            && ModifiedTimePredicate?.Invoke(fileInfo.LastWriteTime) != false;
    }

    internal bool IsMatch(DirectoryInfo directoryInfo)
    {
        return CreationTimePredicate?.Invoke(directoryInfo.CreationTime) != false
            && ModifiedTimePredicate?.Invoke(directoryInfo.LastWriteTime) != false;
    }
}
