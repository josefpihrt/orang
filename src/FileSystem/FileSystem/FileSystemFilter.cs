// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

#pragma warning disable RCS1223

namespace Orang.FileSystem;

//TODO: JP rename to MatchOptions, FileSystemMatcher, FileMatcher
public class FileSystemFilter
{
    public Filter? Name { get; set; }

    public FileNamePart Part { get; set; }

    public Filter? Extension { get; set; }

    public Filter? Content { get; set; }

    public FilePropertyFilter? Properties { get; set; }

    public FileAttributes Attributes { get; set; }

    public FileAttributes AttributesToSkip { get; set; }

    public FileEmptyOption FileEmptyOption { get; set; }
}
