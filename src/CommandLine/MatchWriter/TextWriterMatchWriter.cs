// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal class TextWriterMatchWriter : MatchWriter
    {
        private readonly TextWriter _writer;
        private int _writerIndex;

        public TextWriterMatchWriter(
            string input,
            MatchEvaluator matchEvaluator,
            TextWriter writer,
            MatchWriterOptions options = null) : base(input, options)
        {
            MatchEvaluator = matchEvaluator;
            _writer = writer;
        }

        public MatchEvaluator MatchEvaluator { get; }

        protected override ValueWriter ValueWriter => throw new NotSupportedException();

        protected override void WriteStartMatches()
        {
            _writerIndex = 0;
        }

        protected override void WriteStartMatch(Capture capture)
        {
        }

        protected override void WriteMatch(Capture capture)
        {
            _writer.Write(Input.AsSpan(_writerIndex, capture.Index - _writerIndex));

            string result = MatchEvaluator((Match)capture);

            _writer.Write(result);

            _writerIndex = capture.Index + capture.Length;
        }

        protected override void WriteEndMatch(Capture capture)
        {
        }

        protected override void WriteMatchSeparator()
        {
        }

        protected override void WriteEndMatches()
        {
            _writer.Write(Input.AsSpan(_writerIndex, Input.Length - _writerIndex));
        }

        protected override void WriteStartReplacement(Match match, string result)
        {
        }

        protected override void WriteEndReplacement(Match match, string result)
        {
        }

        public override void Dispose()
        {
            _writer.Dispose();
        }
    }
}
