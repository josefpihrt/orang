// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.CommandLine
{
    internal class OutputDisplayFormat
    {
        internal static readonly string DefaultIndent = "  ";

        public OutputDisplayFormat(
            ContentDisplayStyle contentDisplayStyle,
            LineDisplayOptions lineOptions = LineDisplayOptions.None,
            MiscellaneousDisplayOptions miscellaneousOptions = MiscellaneousDisplayOptions.None,
            string indent = null,
            string separator = null)
        {
            ContentDisplayStyle = contentDisplayStyle;
            LineOptions = lineOptions;
            MiscellaneousOptions = miscellaneousOptions;
            Indent = indent ?? DefaultIndent;
            Separator = separator ?? Environment.NewLine;
        }

        public ContentDisplayStyle ContentDisplayStyle { get; }

        public LineDisplayOptions LineOptions { get; }

        public MiscellaneousDisplayOptions MiscellaneousOptions { get; }

        public string Indent { get; }

        public string Separator { get; }

        public bool Includes(LineDisplayOptions options) => (LineOptions & options) == options;

        public bool Includes(MiscellaneousDisplayOptions options) => (MiscellaneousOptions & options) == options;
    }
}
