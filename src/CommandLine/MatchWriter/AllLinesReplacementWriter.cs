// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal class AllLinesReplacementWriter : AllLinesContentWriter
    {
        private readonly TextWriter _writer;
        private int _writerIndex;
        private ValueWriter _valueWriter;
        private ValueWriter _replacementValueWriter;

        public AllLinesReplacementWriter(
            string input,
            MatchEvaluator matchEvaluator,
            ContentWriterOptions options = null,
            TextWriter writer = null) : base(input, options)
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
                    if (Options.IncludeLineNumber)
                    {
                        _valueWriter = new LineNumberValueWriter(Options.Indent);
                    }
                    else
                    {
                        _valueWriter = new ValueWriter(Options.Indent);
                    }
                }

                return _valueWriter;
            }
        }

        private ValueWriter ReplacementValueWriter => _replacementValueWriter ?? (_replacementValueWriter = new ValueWriter(Options.Indent));

        protected override void WriteStartMatches()
        {
            _writerIndex = 0;

            base.WriteStartMatches();
        }

        protected override void WriteNonEmptyMatchValue(Capture capture)
        {
            if (Options.HighlightMatch)
            {
                base.WriteNonEmptyMatchValue(capture);
            }
            else if (Options.IncludeLineNumber)
            {
                ((LineNumberValueWriter)ValueWriter).LineNumber += TextHelpers.CountLines(Input, capture.Index, capture.Length);
            }
        }

        protected override void WriteNonEmptyReplacementValue(string result)
        {
            if (Options.IncludeLineNumber)
            {
                ReplacementValueWriter.Write(result, Symbols, ReplacementColors, ReplacementBoundaryColors);
            }
            else
            {
                base.WriteNonEmptyReplacementValue(result);
            }
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
