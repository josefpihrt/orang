// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Orang.FileSystem
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FileMatch
    {
        private FileSystemInfo _fileSystemInfo;

        internal FileMatch(in NamePart part, Match nameMatch, FileSystemInfo fileSystemInfo = null, bool isDirectory = false)
            : this(part, nameMatch, default(FileContent), default(Match), fileSystemInfo, isDirectory)
        {
        }

        internal FileMatch(in NamePart part, Match nameMatch, in FileContent content, Match contentMatch, FileSystemInfo fileSystemInfo = null, bool isDirectory = false)
        {
            NameMatch = nameMatch;
            Part = part;
            Content = content;
            ContentMatch = contentMatch;
            IsDirectory = isDirectory;
            _fileSystemInfo = fileSystemInfo;
        }

        public NamePart Part { get; }

        public Match NameMatch { get; }

        public FileContent Content { get; }

        public Match ContentMatch { get; }

        public bool IsDirectory { get; }

        public FileSystemInfo FileSystemInfo
        {
            get
            {
                if (_fileSystemInfo == null)
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

        public string Path => Part.Path;

        public int Index => (NameMatch?.Success == true) ? NameMatch.Index : -1;

        public int Length => (NameMatch?.Success == true) ? NameMatch.Length : -1;

        public string ContentText => Content.Text;

        public Encoding Encoding => Content.BomEncoding ?? Content.CurrentEncoding;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Path}";
    }
}
