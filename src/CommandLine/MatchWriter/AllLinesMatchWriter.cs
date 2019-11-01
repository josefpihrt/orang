// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal class AllLinesMatchWriter : MatchWriter
    {
        private int _lastPos;
        private ValueWriter _valueWriter;

        public AllLinesMatchWriter(
            string input,
            MatchWriterOptions options = null,
            List<string> values = null) : base(input, options)
        {
            Values = values;
        }

        public List<string> Values { get; }

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

        protected override void WriteStartMatches()
        {
            MatchCount = 0;
            _lastPos = 0;

            Write(Options.Indent);

            if (Options.IncludeLineNumber)
                WriteLineNumber(1);
        }

        protected override void WriteStartMatch(Capture capture)
        {
            Values?.Add(capture.Value);

            int index = capture.Index;

            ValueWriter.Write(Input, _lastPos, index - _lastPos, symbols: null);
        }

        protected override void WriteEndMatch(Capture capture)
        {
            _lastPos = capture.Index + capture.Length;
        }

        protected override void WriteMatchSeparator()
        {
        }

        protected override void WriteStartReplacement(Match match, string result)
        {
        }

        protected override void WriteEndReplacement(Match match, string result)
        {
        }

        protected override void WriteEndMatches()
        {
            ValueWriter.Write(Input, _lastPos, Input.Length - _lastPos, symbols: null);
            WriteLine();
        }

        public override void Dispose()
        {
        }
    }
}
