// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Orang
{
    internal sealed class ConsoleWriter : LogWriter
    {
        public static ConsoleWriter Instance { get; } = new ConsoleWriter(Console.Out, Console.Out.FormatProvider);

        private ConsoleWriter(TextWriter writer, IFormatProvider formatProvider) : base(writer, formatProvider)
        {
        }

        internal ConsoleColors Colors
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

        public override TextWriter ErrorWriter => Console.Error;

        public override void Write(char value, ConsoleColors colors)
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

        public override void Write(char value, ConsoleColors colors, Verbosity verbosity)
        {
            if (ShouldWrite(verbosity))
                Write(value, colors);
        }

        public override void Write(char value, int repeatCount, ConsoleColors colors)
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

        public override void Write(char value, int repeatCount, ConsoleColors colors, Verbosity verbosity)
        {
            if (ShouldWrite(verbosity))
                Write(value, repeatCount, colors);
        }

        public override void Write(string? value, ConsoleColors colors)
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

        public override void Write(string? value, ConsoleColors colors, Verbosity verbosity)
        {
            if (ShouldWrite(verbosity))
                Write(value, colors);
        }

        public override void Write(ReadOnlySpan<char> buffer, ConsoleColors colors)
        {
            if (!colors.IsDefault)
            {
                ConsoleColors tmp = Colors;
                Colors = colors;
                Write(buffer);
                Colors = tmp;
            }
            else
            {
                Write(buffer);
            }
        }

        public override void Write(ReadOnlySpan<char> buffer, ConsoleColors colors, Verbosity verbosity)
        {
            if (ShouldWrite(verbosity))
                Write(buffer, colors);
        }

        public override void WriteLine(string? value, ConsoleColors colors)
        {
            if (!colors.IsDefault)
            {
                ConsoleColors tmp = Colors;
                Colors = colors;
                Write(value);
                Colors = tmp;
                WriteLine();
            }
            else
            {
                WriteLine(value);
            }
        }

        public override void WriteLine(string? value, ConsoleColors colors, Verbosity verbosity)
        {
            if (ShouldWrite(verbosity))
                WriteLine(value, colors);
        }

        public override void WriteLine(ReadOnlySpan<char> buffer, ConsoleColors colors)
        {
            if (!colors.IsDefault)
            {
                ConsoleColors tmp = Colors;
                Colors = colors;
                Write(buffer);
                Colors = tmp;
                WriteLine();
            }
            else
            {
                WriteLine(buffer);
            }
        }

        public override void WriteLine(ReadOnlySpan<char> buffer, ConsoleColors colors, Verbosity verbosity)
        {
            if (ShouldWrite(verbosity))
                WriteLine(buffer, colors);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
