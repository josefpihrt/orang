// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Text.RegularExpressions;
using Orang.FileSystem;

namespace Orang.CommandLine
{
    internal class ContentSearchResult : SearchResult
    {
        public ContentSearchResult(
            FileMatch fileMatch,
            string baseDirectoryPath,
            ContentWriterOptions writerOptions) : base(fileMatch, baseDirectoryPath)
        {
            WriterOptions = writerOptions;
        }

        public Match ContentMatch => FileMatch.ContentMatch;

        public string ContentText => FileMatch.ContentText;

        public Encoding Encoding => FileMatch.Encoding;

        public ContentWriterOptions WriterOptions { get; }
    }
}
