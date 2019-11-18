// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Text.RegularExpressions;
using Orang.FileSystem;

namespace Orang.CommandLine
{
    internal class ContentSearchResult : SearchResult
    {
        public ContentSearchResult(
            FileSystemFinderResult result,
            string baseDirectoryPath,
            Match match,
            string input,
            Encoding encoding,
            ContentWriterOptions writerOptions) : base(result, baseDirectoryPath)
        {
            Match = match;
            Input = input;
            Encoding = encoding;
            WriterOptions = writerOptions;
        }

        public Match Match { get; }

        public string Input { get; }

        public Encoding Encoding { get; }

        public ContentWriterOptions WriterOptions { get; }
    }
}
