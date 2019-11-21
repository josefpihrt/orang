// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine
{
    internal abstract class CommonRegexCommandOptions
    {
        internal CommonRegexCommandOptions()
        {
        }

        public OutputOptions Output { get; internal set; }

        public OutputDisplayFormat Format { get; internal set; }

        public bool IncludeSummary => Format.Includes(DisplayParts.Summary);

        public bool IncludeCount => Format.Includes(DisplayParts.Count);

        public string OutputPath => Output?.Path;

        public HighlightOptions HighlightOptions { get; internal set; }

        public bool HighlightMatch => (HighlightOptions & HighlightOptions.Match) != 0;

        public bool HighlightReplacement => (HighlightOptions & HighlightOptions.Replacement) != 0;
    }
}
