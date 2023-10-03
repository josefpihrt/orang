// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Orang.FileSystem;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class FileMatch
{
    private FileSystemInfo? _fileSystemInfo;

    internal FileMatch(
        FileNameSpan nameSpan,
        Match? name,
        FileSystemInfo? fileSystemInfo = null,
        bool isDirectory = false)
        : this(name, nameSpan, default(Match), default(FileContent), fileSystemInfo, isDirectory)
    {
    }

    internal FileMatch(
        Match? name,
        FileNameSpan nameSpan,
        Match? content,
        FileContent fileContent,
        FileSystemInfo? fileSystemInfo = null,
        bool isDirectory = false)
    {
        Name = name;
        NameSpan = nameSpan;
        FileContent = fileContent;
        Content = content;
        IsDirectory = isDirectory;
        _fileSystemInfo = fileSystemInfo;
    }

    public FileNameSpan NameSpan { get; }

    public Match? Name { get; }

    internal FileContent FileContent { get; set; }

    public Match? Content { get; }

    public bool IsDirectory { get; }

    internal bool IsSubmatch { get; set; }

    internal FileSystemInfo FileSystemInfo
    {
        get
        {
            if (_fileSystemInfo is null)
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

    public string Path => NameSpan.Path;

    internal int Index => (Name?.Success == true) ? Name.Index : -1;

    internal int Length => (Name?.Success == true) ? Name.Length : -1;

    internal string ContentText => FileContent.Text;

    internal Encoding Encoding => FileContent.Encoding;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{Path}";
}
