﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine;

internal class AskLineContentWriter : LineContentWriter
{
    public AskLineContentWriter(
        string input,
        ContentTextWriter? writer,
        ContentWriterOptions options,
        IResultStorage? storage = null) : base(input, writer, options, storage)
    {
        Ask = true;
    }

    public bool Ask { get; set; }

    protected override void WriteStartMatch(ICapture capture)
    {
        ResultStorage?.Add(capture.Value);

        if (Options.IncludeLineNumber)
        {
            _lineNumber += TextHelpers.CountLines(Input, _solIndex, capture.Index - _solIndex);
            ((LineNumberValueWriter)ValueWriter).LineNumber = _lineNumber;
        }

        int solIndex = FindStartOfLine(capture);

        if (Options.BeforeContext > 0)
            WriteBeforeContext(0, solIndex, _lineNumber);

        Write(Options.Indent);

        if (Options.IncludeLineNumber)
            WriteLineNumber(_lineNumber);

        if (capture.Index >= _eolIndex)
        {
            MatchingLineCount++;

            int length = capture.Length;

            if (length > 0)
            {
                if (Input[capture.Index + length - 1] == '\n')
                    length--;

                MatchingLineCount += TextHelpers.CountLines(Input, capture.Index, length);
            }
        }

        _solIndex = solIndex;
        _eolIndex = FindEndOfLine(capture);

        WriteStartLine(_solIndex, capture.Index);
    }

    protected override void WriteEndMatch(ICapture capture)
    {
        int endIndex = capture.Index + capture.Length;

        int eolIndex = FindEndOfLine(capture);

        WriteEndLine(endIndex, eolIndex);

        if (Options.AfterContext > 0)
            WriteAfterContext(eolIndex, Input.Length, _lineNumber);

        if (Ask
            && ConsoleHelpers.AskToContinue(Options.Indent) == DialogResult.YesToAll)
        {
            Ask = false;
        }
    }

    protected override void WriteEndMatches()
    {
    }
}
