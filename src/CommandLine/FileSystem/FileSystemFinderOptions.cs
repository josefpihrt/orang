// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Orang.CommandLine;

namespace Orang.FileSystem
{
    public class FileSystemFinderOptions
    {
        public static FileSystemFinderOptions Default { get; } = new FileSystemFinderOptions();

        public FileSystemFinderOptions(
            SearchTarget searchTarget = SearchTarget.Files,
            bool recurseSubdirectories = true,
            bool returnSpecialDirectories = false,
            bool ignoreInaccessible = true,
            FileAttributes attributes = default,
            FileAttributes attributesToSkip = default,
            MatchType matchType = MatchType.Simple,
            MatchCasing matchCasing = MatchCasing.PlatformDefault,
            bool? empty = null,
            bool canEnumerate = true,
            bool partOnly = false,
            bool saveBomEncoding = false,
            Encoding encoding = null)
        {
            SearchTarget = searchTarget;
            RecurseSubdirectories = recurseSubdirectories;
            ReturnSpecialDirectories = returnSpecialDirectories;
            IgnoreInaccessible = ignoreInaccessible;
            Attributes = attributes;
            AttributesToSkip = attributesToSkip;
            MatchType = matchType;
            MatchCasing = matchCasing;
            Empty = empty;
            CanEnumerate = canEnumerate;
            PartOnly = partOnly;
            SaveBomEncoding = saveBomEncoding;
            Encoding = encoding ?? EncodingHelpers.UTF8NoBom;
        }

        public SearchTarget SearchTarget { get; }

        public bool RecurseSubdirectories { get; }

        public bool ReturnSpecialDirectories { get; }

        public bool IgnoreInaccessible { get; }

        public FileAttributes Attributes { get; }

        public FileAttributes AttributesToSkip { get; }

        public MatchType MatchType { get; }

        public MatchCasing MatchCasing { get; }

        public bool? Empty { get; }

        public bool CanEnumerate { get; }

        public bool PartOnly { get; }

        public bool SaveBomEncoding { get; }

        public Encoding Encoding { get; }
    }
}
