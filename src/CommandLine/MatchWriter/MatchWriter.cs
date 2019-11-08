// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Orang.CommandLine
{
    internal abstract class MatchWriter : IDisposable
    {
        protected MatchWriter(
            string input,
            MatchWriterOptions options)
        {
            Input = input;
            Options = options;
            MatchingLineCount = -1;
        }

        public string Input { get; }

        public MatchWriterOptions Options { get; }

        public int MatchCount { get; private set; }

        public int MatchingLineCount { get; protected set; }

        protected abstract ValueWriter ValueWriter { get; }

        public OutputSymbols Symbols => Options.Symbols;

        public ConsoleColors MatchColors => (Options.HighlightMatch) ? Colors.Match : default;

        public ConsoleColors ReplacementColors => (Options.HighlightReplacement) ? Colors.Replacement : default;

        public ConsoleColors MatchBoundaryColors => (Options.HighlightBoundary) ? Colors.MatchBoundary : default;

        public ConsoleColors ReplacementBoundaryColors => (Options.HighlightBoundary) ? Colors.ReplacementBoundary : default;

        public static MatchWriter CreateFind(
            ContentDisplayStyle contentDisplayStyle,
            string input,
            MatchWriterOptions options,
            IResultStorage storage,
            MatchOutputInfo outputInfo,
            bool ask = false)
        {
            if (ask)
            {
                switch (contentDisplayStyle)
                {
                    case ContentDisplayStyle.Value:
                    case ContentDisplayStyle.ValueDetail:
                        return new AskValueMatchWriter(input, options, storage, outputInfo);
                    case ContentDisplayStyle.Line:
                        return new AskLineMatchWriter(input, options, storage);
                    case ContentDisplayStyle.UnmatchedLines:
                    case ContentDisplayStyle.AllLines:
                        throw new InvalidOperationException();
                    default:
                        throw new InvalidOperationException($"Unknown enum value '{contentDisplayStyle}'.");
                }
            }
            else
            {
                switch (contentDisplayStyle)
                {
                    case ContentDisplayStyle.Value:
                    case ContentDisplayStyle.ValueDetail:
                        return new ValueMatchWriter(input, options, storage, outputInfo);
                    case ContentDisplayStyle.Line:
                        return new LineMatchWriter(input, options, storage);
                    case ContentDisplayStyle.UnmatchedLines:
                        return new UnmatchedLineWriter(input, options, storage);
                    case ContentDisplayStyle.AllLines:
                        return new AllLinesMatchWriter(input, options, storage);
                    default:
                        throw new InvalidOperationException($"Unknown enum value '{contentDisplayStyle}'.");
                }
            }
        }

        public static MatchWriter CreateReplace(
            ContentDisplayStyle contentDisplayStyle,
            string input,
            MatchEvaluator evaluator,
            MatchWriterOptions options,
            TextWriter writer = null,
            MatchOutputInfo outputInfo = null)
        {
            switch (contentDisplayStyle)
            {
                case ContentDisplayStyle.Value:
                case ContentDisplayStyle.ValueDetail:
                    return new ValueReplacementWriter(input, evaluator, options, writer, outputInfo);
                case ContentDisplayStyle.Line:
                    return new LineReplacementWriter(input, evaluator, options, writer);
                case ContentDisplayStyle.AllLines:
                    return new AllLinesReplacementWriter(input, evaluator, options, writer);
                case ContentDisplayStyle.UnmatchedLines:
                    throw new NotSupportedException($"Value '{contentDisplayStyle}' is not supported.");
                default:
                    throw new InvalidOperationException($"Unknown enum value '{contentDisplayStyle}'.");
            }
        }

        protected abstract void WriteStartMatches();

        protected abstract void WriteEndMatches();

        protected abstract void WriteStartMatch(Capture capture);

        protected abstract void WriteEndMatch(Capture capture);

        protected abstract void WriteStartReplacement(Match match, string result);

        protected abstract void WriteEndReplacement(Match match, string result);

        protected abstract void WriteMatchSeparator();

        public abstract void Dispose();

        protected virtual void Write(string value)
        {
            Logger.Write(value, Verbosity.Normal);
        }

        protected virtual void Write(string value, in ConsoleColors colors)
        {
            Logger.Write(value, colors, Verbosity.Normal);
        }

        protected virtual void Write(string value, int startIndex, int length)
        {
            Logger.Write(value, startIndex, length, Verbosity.Normal);
        }

        protected virtual void WriteLineNumber(int value)
        {
            Logger.Write(value.ToString(), Colors.LineNumber, Verbosity.Normal);
            Write(" ");
        }

        protected virtual void WriteMatchValue(Capture capture)
        {
            if (capture.Length > 0)
            {
                WriteNonEmptyMatchValue(capture);
            }
            else if (Options.HighlightEmptyMatch)
            {
                Logger.Write("|", Colors.EmptyMatch, Verbosity.Normal);
            }
        }

        protected virtual void WriteNonEmptyMatchValue(Capture capture)
        {
            ValueWriter.Write(
                Input,
                capture.Index,
                capture.Length,
                Symbols,
                MatchColors,
                MatchBoundaryColors);
        }

        protected virtual void WriteReplacementValue(Match match, string result)
        {
            if (result.Length > 0)
            {
                WriteNonEmptyReplacementValue(result);
            }
            else if (Options.HighlightEmptyReplacement)
            {
                Logger.Write("|", Colors.EmptyReplacement, Verbosity.Normal);
            }
        }

        protected virtual void WriteNonEmptyReplacementValue(string result)
        {
            ValueWriter.Write(result, Symbols, ReplacementColors, ReplacementBoundaryColors);
        }

        protected virtual void WriteLine()
        {
            Logger.WriteLine(Verbosity.Normal);
        }

        public virtual MaxReason WriteMatches(Match match, int count = 0, in CancellationToken cancellationToken = default)
        {
            MatchCount = 0;

            WriteStartMatches();

            MaxReason maxReason = WriteMatchesImpl(match, count, cancellationToken);

            WriteEndMatches();

            return maxReason;
        }

        private MaxReason WriteMatchesImpl(Match match, int count, in CancellationToken cancellationToken)
        {
            int groupNumber = Options.GroupNumber;

            if (groupNumber < 1)
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    WriteMatch(match);
                    MatchCount++;

                    match = match.NextMatch();

                    if (MatchCount == count)
                    {
                        return (match.Success)
                            ? MaxReason.CountExceedsMax
                            : MaxReason.CountEqualsMax;
                    }

                    if (match.Success)
                    {
                        WriteMatchSeparator();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                Group group = NextGroup();

                if (group != null)
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        WriteMatch(group);
                        MatchCount++;

                        group = NextGroup();

                        if (MatchCount == count)
                        {
                            return (group != null)
                                ? MaxReason.CountExceedsMax
                                : MaxReason.CountEqualsMax;
                        }

                        if (group != null)
                        {
                            WriteMatchSeparator();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return MaxReason.None;

            Group NextGroup()
            {
                while (match.Success)
                {
                    Group group = match.Groups[groupNumber];

                    match = match.NextMatch();

                    if (group.Success)
                        return group;
                }

                return null;
            }
        }

        protected virtual void WriteMatch(Capture capture)
        {
            WriteStartMatch(capture);

            WriteMatchValue(capture);

            WriteEndMatch(capture);
        }

        protected virtual void WriteReplacement(Match match, string result)
        {
            WriteStartReplacement(match, result);

            WriteReplacementValue(match, result);

            WriteEndReplacement(match, result);
        }

        protected void WriteStartLine(int index, int endIndex)
        {
            if (Options.TrimLine)
            {
                while (index < endIndex
                    && char.IsWhiteSpace(Input[index]))
                {
                    index++;
                }
            }

            if (index < endIndex)
            {
                if (Input[endIndex - 1] == '\r')
                    endIndex--;

                Write(Input, index, endIndex - index);
            }
        }

        protected void WriteEndLine(int index, int endIndex)
        {
            if (endIndex > 0
                && Input[endIndex - 1] == '\n')
            {
                endIndex--;

                if (endIndex > 0
                    && Input[endIndex - 1] == '\r')
                {
                    endIndex--;
                }
            }

            if (Options.TrimLine)
            {
                while (endIndex > index
                    && char.IsWhiteSpace(Input[endIndex - 1]))
                {
                    endIndex--;
                }
            }

            if (endIndex > index)
                Write(Input, index, endIndex - index);

            WriteLine();
        }

        protected int FindStartOfLine(Capture capture)
        {
            return FindStartOfLine(capture.Index);
        }

        protected int FindStartOfLine(int index)
        {
            while (index > 0
                && Input[index - 1] != '\n')
            {
                index--;
            }

            return index;
        }

        protected int FindEndOfLine(Capture capture)
        {
            int index = capture.Index;

            if (index > 0
                && capture.Length > 0
                && Input[index - 1] == '\n')
            {
                return index;
            }

            while (index < Input.Length)
            {
                if (Input[index] == '\n')
                    return ++index;

                index++;
            }

            return index;
        }
    }
}
