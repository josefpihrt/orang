// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal class ValueMatchWriter : MatchWriter
    {
        private ValueWriter _valueWriter;

        public ValueMatchWriter(
            string input,
            MatchWriterOptions options = null,
            List<string> values = null,
            MatchOutputInfo outputInfo = null) : base(input, options)
        {
            Values = values;
            OutputInfo = outputInfo;
        }

        public List<string> Values { get; }

        public MatchOutputInfo OutputInfo { get; }

        protected override ValueWriter ValueWriter
        {
            get
            {
                if (_valueWriter == null)
                {
                    string infoIndent = Options.Indent + new string(' ', OutputInfo?.Width ?? 0);

                    _valueWriter = new ValueWriter(infoIndent, includeEndingIndent: false);
                }

                return _valueWriter;
            }
        }

        protected override void WriteStartMatches()
        {
            MatchCount = 0;
        }

        protected override void WriteStartMatch(Capture capture)
        {
            Values?.Add(capture.Value);

            if (MatchCount == 0
                || Options.Separator == "\n"
                || Options.Separator == "\r\n")
            {
                Write(Options.Indent);
            }

            if (OutputInfo != null)
                Write(OutputInfo.GetText(capture, MatchCount + 1, groupName: Options.GroupName, captureNumber: -1));
        }

        protected override void WriteEndMatch(Capture capture)
        {
        }

        protected override void WriteMatchSeparator()
        {
            if (Options.Separator != null)
            {
                Write(Options.Separator);
            }
            else
            {
                WriteLine();
                Write(Options.Indent);
            }
        }

        protected override void WriteStartReplacement(Match match, string result)
        {
        }

        protected override void WriteEndReplacement(Match match, string result)
        {
        }

        protected override void WriteEndMatches()
        {
            WriteLine();
        }

        public override void Dispose()
        {
        }
    }
}
