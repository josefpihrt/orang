// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal class EmptyMatchWriter : MatchWriter
    {
        public EmptyMatchWriter(
            string input,
            MatchWriterOptions options,
            List<string> values = null) : base(input, options)
        {
            Values = values;
        }

        protected override ValueWriter ValueWriter => throw new NotSupportedException();

        public List<string> Values { get; }

        protected override void WriteMatch(Capture capture)
        {
            Values?.Add(capture.Value);
        }

        protected override void WriteStartMatches()
        {
        }

        protected override void WriteStartMatch(Capture capture) => throw new NotSupportedException();

        protected override void WriteEndMatch(Capture capture) => throw new NotSupportedException();

        protected override void WriteMatchSeparator()
        {
        }

        protected override void WriteEndMatches()
        {
        }

        protected override void WriteStartReplacement(Match match, string result) => throw new NotSupportedException();

        protected override void WriteEndReplacement(Match match, string result) => throw new NotSupportedException();

        public override void Dispose()
        {
        }
    }
}
