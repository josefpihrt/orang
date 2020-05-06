// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Orang.CommandLine;

namespace Orang.FileSystem
{
    public class FileSystemFilterOptions
    {
        public static FileSystemFilterOptions Default { get; } = new FileSystemFilterOptions();

        public FileSystemFilterOptions(
            SearchTarget searchTarget = SearchTarget.Files,
            bool recurseSubdirectories = true,
            bool returnSpecialDirectories = false,
            bool ignoreInaccessible = true,
            MatchType matchType = MatchType.Simple,
            MatchCasing matchCasing = MatchCasing.PlatformDefault,
            bool canEnumerate = true,
            bool partOnly = false,
            bool saveBomEncoding = false,
            Encoding encoding = null)
        {
            SearchTarget = searchTarget;
            RecurseSubdirectories = recurseSubdirectories;
            ReturnSpecialDirectories = returnSpecialDirectories;
            IgnoreInaccessible = ignoreInaccessible;
            MatchType = matchType;
            MatchCasing = matchCasing;
            CanEnumerate = canEnumerate;
            PartOnly = partOnly;
            SaveBomEncoding = saveBomEncoding;
            Encoding = encoding ?? EncodingHelpers.UTF8NoBom;
        }

        public SearchTarget SearchTarget { get; }

        public bool RecurseSubdirectories { get; }

        public bool ReturnSpecialDirectories { get; }

        public bool IgnoreInaccessible { get; }

        public MatchType MatchType { get; }

        public MatchCasing MatchCasing { get; }

        public bool CanEnumerate { get; }

        public bool PartOnly { get; }

        public bool SaveBomEncoding { get; }

        public Encoding Encoding { get; }
    }
}
