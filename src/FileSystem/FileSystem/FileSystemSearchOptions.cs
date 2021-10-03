// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Text;
using Orang.Text;

namespace Orang.FileSystem
{
    //TODO: FileSearchOptions, SearchOptions
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FileSystemSearchOptions
    {
        public static FileSystemSearchOptions Default { get; } = new FileSystemSearchOptions();

        public FileSystemSearchOptions(
            SearchTarget searchTarget = SearchTarget.Files,
            bool recurseSubdirectories = true,
            bool ignoreInaccessible = true,
            int minDirectoryDepth = -1,
            int maxDirectoryDepth = -1,
            Encoding? defaultEncoding = null)
        {
            SearchTarget = searchTarget;
            RecurseSubdirectories = recurseSubdirectories;
            IgnoreInaccessible = ignoreInaccessible;
            MinDirectoryDepth = minDirectoryDepth;
            MaxDirectoryDepth = maxDirectoryDepth;
            DefaultEncoding = defaultEncoding ?? EncodingHelpers.UTF8NoBom;
        }

        public SearchTarget SearchTarget { get; }

        public bool RecurseSubdirectories { get; }

        public bool IgnoreInaccessible { get; }

        public int MinDirectoryDepth { get; }

        public int MaxDirectoryDepth { get; }

        public Encoding DefaultEncoding { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{SearchTarget}  {nameof(RecurseSubdirectories)} = {RecurseSubdirectories}";
    }
}
