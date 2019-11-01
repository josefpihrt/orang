// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang
{
    internal sealed class ConsoleWriter : TextWriterWithVerbosity
    {
        public static ConsoleWriter Instance { get; } = new ConsoleWriter();

        private ConsoleWriter() : base(Console.Out, Console.Out.FormatProvider)
        {
        }

        public ConsoleColors Colors
        {
            get { return new ConsoleColors(Console.ForegroundColor, Console.BackgroundColor); }
            set
            {
                if (value.Foreground != null)
                    Console.ForegroundColor = value.Foreground.Value;

                if (value.Background != null)
                    Console.BackgroundColor = value.Background.Value;
            }
        }

        public void Write(char value, int repeatCount, in ConsoleColors colors)
        {
            if (!colors.IsDefault)
            {
                ConsoleColors tmp = Colors;
                Colors = colors;
                Write(value, repeatCount);
                Colors = tmp;
            }
            else
            {
                Write(value, repeatCount);
            }
        }

        public void Write(char value, int repeatCount, in ConsoleColors colors, Verbosity verbosity)
        {
            if (verbosity <= Verbosity)
                Write(value, repeatCount, colors);
        }

        public void Write(string value, in ConsoleColors colors)
        {
            if (!colors.IsDefault)
            {
                ConsoleColors tmp = Colors;
                Colors = colors;
                Write(value);
                Colors = tmp;
            }
            else
            {
                Write(value);
            }
        }

        public void Write(string value, in ConsoleColors colors, Verbosity verbosity)
        {
            WriteIf(verbosity <= Verbosity, value, colors);
        }

        public void Write(string value, int startIndex, int length, in ConsoleColors colors)
        {
            if (!colors.IsDefault)
            {
                ConsoleColors tmp = Colors;
                Colors = colors;
                Write(value, startIndex, length);
                Colors = tmp;
            }
            else
            {
                Write(value, startIndex, length);
            }
        }

        public void Write(string value, int startIndex, int length, in ConsoleColors colors, Verbosity verbosity)
        {
            if (verbosity <= Verbosity)
                Write(value, startIndex, length, colors);
        }

        public void WriteIf(bool condition, string value, in ConsoleColors colors)
        {
            if (condition)
                Write(value, colors);
        }

        public void WriteLine(string value, in ConsoleColors colors)
        {
            if (!colors.IsDefault)
            {
                ConsoleColors tmp = Colors;
                Colors = colors;
                WriteLine(value);
                Colors = tmp;
            }
            else
            {
                WriteLine(value);
            }
        }

        public void WriteLine(string value, in ConsoleColors colors, Verbosity verbosity)
        {
            WriteLineIf(verbosity <= Verbosity, value, colors);
        }

        public void WriteLineIf(bool condition, string value, in ConsoleColors colors)
        {
            if (condition)
                WriteLine(value, colors);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
