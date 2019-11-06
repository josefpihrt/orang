// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal class AskValueMatchWriter : ValueMatchWriter
    {
        public AskValueMatchWriter(
            string input,
            MatchWriterOptions options = null,
            IResultStorage storage = null,
            MatchOutputInfo outputInfo = null) : base(input, options, storage, outputInfo)
        {
            Ask = true;
        }

        public bool Ask { get; set; }

        protected override void WriteStartMatch(Capture capture)
        {
            ResultStorage?.Add(capture.Value);

            Write(Options.Indent);

            if (OutputInfo != null)
                Write(OutputInfo.GetText(capture, MatchCount + 1, groupName: Options.GroupName, captureNumber: -1));
        }

        protected override void WriteEndMatch(Capture capture)
        {
            WriteLine();

            if (ConsoleHelpers.QuestionIf(Ask, "Continue without asking?", Options.Indent))
                Ask = false;
        }

        protected override void WriteMatchSeparator()
        {
        }

        protected override void WriteEndMatches()
        {
        }
    }
}
