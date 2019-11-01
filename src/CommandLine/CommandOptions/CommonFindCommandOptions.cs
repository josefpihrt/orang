// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Orang.FileSystem;

namespace Orang.CommandLine
{
    internal abstract class CommonFindCommandOptions : CommonRegexCommandOptions
    {
        private string _doubleIndent;

        internal CommonFindCommandOptions()
        {
        }

        public ImmutableArray<string> Paths { get; internal set; }

        public Filter NameFilter { get; internal set; }

        public Filter ExtensionFilter { get; internal set; }

        public Filter ContentFilter { get; internal set; }

        public Filter DirectoryFilter { get; internal set; }

        public SearchTarget SearchTarget { get; internal set; }

        public FileAttributes Attributes { get; internal set; }

        public FileAttributes AttributesToSkip { get; internal set; }

        public bool RecurseSubdirectories { get; internal set; }

        public bool Progress { get; internal set; }

        public Encoding DefaultEncoding { get; internal set; }

        public bool? Empty { get; internal set; }

        public OutputDisplayFormat Format { get; internal set; }

        public int MaxMatchingFiles { get; internal set; }

        public ContentDisplayStyle ContentDisplayStyle => Format.ContentDisplayStyle;

        public string Indent => Format.Indent;

        internal string DoubleIndent => _doubleIndent ?? (_doubleIndent = Indent + Indent);

        internal MatchOutputInfo CreateOutputInfo(string input, Match match)
        {
            if (ContentDisplayStyle != ContentDisplayStyle.ValueDetail)
                return null;

            int groupNumber = ContentFilter.GroupNumber;

            return MatchOutputInfo.Create(
                MatchData.Create(input, ContentFilter.Regex, match),
                groupNumber,
                includeGroupNumber: groupNumber >= 0,
                includeCaptureNumber: false);
        }
    }
}
