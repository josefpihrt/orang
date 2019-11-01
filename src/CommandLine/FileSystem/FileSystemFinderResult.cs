// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Orang.FileSystem
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct FileSystemFinderResult
    {
        internal FileSystemFinderResult(in NamePart part, Match match, bool isDirectory = false)
        {
            Match = match;
            Part = part;
            IsDirectory = isDirectory;
        }

        public string Path => Part.Path;

        public NamePart Part { get; }

        public Match Match { get; }

        public bool IsDirectory { get; }

        public int Index => (Match?.Success == true) ? Match.Index : -1;

        public int Length => (Match?.Success == true) ? Match.Length : -1;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Path}";
    }
}
