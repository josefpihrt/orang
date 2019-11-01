// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal class ValueReplacementWriter : ValueMatchWriter
    {
        private readonly TextWriter _writer;
        private int _writerIndex;
        private ValueWriter _valueWriter;

        public ValueReplacementWriter(
            string input,
            MatchEvaluator matchEvaluator,
            MatchWriterOptions options = null,
            TextWriter writer = null,
            MatchOutputInfo outputInfo = null) : base(input, options, outputInfo: outputInfo)
        {
            MatchEvaluator = matchEvaluator;
            _writer = writer;
        }

        public MatchEvaluator MatchEvaluator { get; }

        protected override ValueWriter ValueWriter
        {
            get
            {
                if (_valueWriter == null)
                {
                    string infoIndent = Options.Indent + new string(' ', OutputInfo?.Width ?? 0);

                    _valueWriter = new ValueWriter(infoIndent);
                }

                return _valueWriter;
            }
        }

        protected override void WriteStartMatches()
        {
            _writerIndex = 0;

            base.WriteStartMatches();
        }

        protected override void WriteNonEmptyMatchValue(Capture capture)
        {
            if (Options.HighlightMatch)
                base.WriteNonEmptyMatchValue(capture);
        }

        protected override void WriteEndMatch(Capture capture)
        {
            var match = (Match)capture;

            string result = MatchEvaluator(match);

            WriteReplacement(match, result);

            base.WriteEndMatch(capture);
        }

        protected override void WriteEndReplacement(Match match, string result)
        {
            _writer?.Write(Input.AsSpan(_writerIndex, match.Index - _writerIndex));
            _writer?.Write(result);

            _writerIndex = match.Index + match.Length;

            base.WriteEndReplacement(match, result);
        }

        protected override void WriteEndMatches()
        {
            _writer?.Write(Input.AsSpan(_writerIndex, Input.Length - _writerIndex));

            base.WriteEndMatches();
        }

        public override void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
