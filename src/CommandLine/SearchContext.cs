// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Orang.FileSystem;

namespace Orang.CommandLine
{
    internal class SearchContext
    {
        public SearchContext(
            SearchTelemetry telemetry = null,
            FileSystemFinderProgressReporter progress = null,
            TextWriter output = null,
            List<SearchResult> results = null,
            in CancellationToken cancellationToken = default)
        {
            Telemetry = telemetry ?? new SearchTelemetry();
            Progress = progress;
            Output = output;
            Results = results;
            CancellationToken = cancellationToken;
        }

        public SearchTelemetry Telemetry { get; }

        public FileSystemFinderProgressReporter Progress { get; }

        public TextWriter Output { get; }

        public List<SearchResult> Results { get; }

        public CancellationToken CancellationToken { get; }

        public SearchState State { get; set; }

        public void AddResult(FileSystemFinderResult result, string baseDirectoryPath)
        {
            var searchResult = new SearchResult(result, baseDirectoryPath);

            Results.Add(searchResult);
        }

        public void AddResult(
            FileSystemFinderResult result,
            string baseDirectoryPath,
            Match match,
            string input,
            Encoding encoding,
            ContentWriterOptions writerOptions)
        {
            var searchResult = new ContentSearchResult(
                result: result,
                baseDirectoryPath: baseDirectoryPath,
                match: match,
                input: input,
                encoding: encoding,
                writerOptions: writerOptions);

            Results.Add(searchResult);
        }
    }
}
