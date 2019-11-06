// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang
{
    internal static class Logger
    {
        public static ConsoleWriter ConsoleOut { get; } = ConsoleWriter.Instance;

        public static TextWriterWithVerbosity Out { get; set; }

        public static void Write(char value)
        {
            ConsoleOut.Write(value);
            Out?.Write(value);
        }

        public static void Write(char value, Verbosity verbosity)
        {
            ConsoleOut.Write(value, verbosity);
            Out?.Write(value, verbosity);
        }

        public static void Write(char value, int repeatCount)
        {
            Write(value, repeatCount, Verbosity.Quiet);
        }

        public static void Write(char value, int repeatCount, Verbosity verbosity)
        {
            ConsoleOut.Write(value, repeatCount, verbosity);
            Out?.Write(value, repeatCount, verbosity);
        }

        public static void Write(char value, int repeatCount, in ConsoleColors colors, Verbosity verbosity)
        {
            ConsoleOut.Write(value, repeatCount, colors, verbosity);
            Out?.Write(value, repeatCount, verbosity);
        }

        public static void Write(string value)
        {
            ConsoleOut.Write(value);
            Out?.Write(value);
        }

        public static void Write(string value, in ConsoleColors colors)
        {
            ConsoleOut.Write(value, colors);
            Out?.Write(value);
        }

        public static void Write(string value, Verbosity verbosity)
        {
            ConsoleOut.Write(value, verbosity: verbosity);
            Out?.Write(value, verbosity: verbosity);
        }

        public static void Write(string value, in ConsoleColors colors, Verbosity verbosity)
        {
            ConsoleOut.Write(value, colors, verbosity);
            Out?.Write(value, verbosity: verbosity);
        }

        public static void Write(string value, int startIndex, int length)
        {
            ConsoleOut.Write(value, startIndex, length);
            Out?.Write(value, startIndex, length);
        }

        public static void Write(string value, int startIndex, int length, in ConsoleColors colors)
        {
            ConsoleOut.Write(value, startIndex, length, colors);
            Out?.Write(value, startIndex, length);
        }

        public static void Write(string value, int startIndex, int length, Verbosity verbosity)
        {
            ConsoleOut.Write(value, startIndex, length, verbosity);
            Out?.Write(value, startIndex, length, verbosity);
        }

        public static void Write(string value, int startIndex, int length, in ConsoleColors colors, Verbosity verbosity)
        {
            ConsoleOut.Write(value, startIndex, length, colors, verbosity);
            Out?.Write(value, startIndex, length, verbosity);
        }

        public static void WriteIf(bool condition, string value)
        {
            ConsoleOut.WriteIf(condition, value);
            Out?.WriteIf(condition, value);
        }

        public static void WriteIf(bool condition, string value, in ConsoleColors colors)
        {
            ConsoleOut.WriteIf(condition, value, colors);
            Out?.WriteIf(condition, value);
        }

        public static void WriteIf(bool condition, string value, in ConsoleColors colors, Verbosity verbosity)
        {
            ConsoleOut.WriteIf(condition, value, colors, verbosity);
            Out?.WriteIf(condition, value, verbosity);
        }

        public static void Write(object value)
        {
            ConsoleOut.Write(value);
            Out?.Write(value);
        }

        public static void WriteLine()
        {
            ConsoleOut.WriteLine();
            Out?.WriteLine();
        }

        public static void WriteLine(Verbosity verbosity)
        {
            ConsoleOut.WriteLine(verbosity);
            Out?.WriteLine(verbosity);
        }

        public static void WriteLineIf(bool condition)
        {
            ConsoleOut.WriteLineIf(condition);
            Out?.WriteLineIf(condition);
        }

        public static void WriteLineIf(bool condition, Verbosity verbosity)
        {
            ConsoleOut.WriteLineIf(condition, verbosity);
            Out?.WriteLineIf(condition, verbosity);
        }

        public static void WriteLine(char value)
        {
            ConsoleOut.WriteLine(value);
            Out?.WriteLine(value);
        }

        public static void WriteLine(string value)
        {
            ConsoleOut.WriteLine(value);
            Out?.WriteLine(value);
        }

        public static void WriteLine(string value, in ConsoleColors colors)
        {
            ConsoleOut.WriteLine(value, colors);
            Out?.WriteLine(value);
        }

        public static void WriteLine(string value, Verbosity verbosity)
        {
            ConsoleOut.WriteLine(value, verbosity: verbosity);
            Out?.WriteLine(value, verbosity: verbosity);
        }

        public static void WriteLine(string value, in ConsoleColors colors, Verbosity verbosity)
        {
            ConsoleOut.WriteLine(value, colors, verbosity: verbosity);
            Out?.WriteLine(value, verbosity: verbosity);
        }

        public static void WriteLineIf(bool condition, string value)
        {
            ConsoleOut.WriteLineIf(condition, value);
            Out?.WriteLineIf(condition, value);
        }

        public static void WriteLineIf(bool condition, string value, in ConsoleColors colors)
        {
            ConsoleOut.WriteLineIf(condition, value, colors);
            Out?.WriteLineIf(condition, value);
        }

        public static void WriteLine(object value)
        {
            ConsoleOut.WriteLine(value);
            Out?.WriteLine(value);
        }

        public static void WriteWarning(
            Exception exception,
            string message = null,
            in ConsoleColors colors = default,
            Verbosity verbosity = Verbosity.Minimal)
        {
            WriteWarning(message ?? exception.Message, colors, verbosity);

            WriteException(exception);
        }

        public static void WriteWarning(
            string message,
            ConsoleColors colors = default,
            Verbosity verbosity = Verbosity.Minimal)
        {
            if (colors.IsDefault)
                colors = Colors.Message_Warning;

            WriteLine(message, colors, verbosity);
        }

        public static void WriteError(
            Exception exception,
            string message = null)
        {
            WriteError(message ?? exception.Message);

            WriteException(exception);
        }

        public static void WriteError(string message)
        {
            Console.Error.WriteLine(message);
        }

        private static void WriteException(Exception exception)
        {
            var verbosity = Verbosity.Diagnostic;
#if DEBUG
            verbosity = Verbosity.Quiet;
#endif
            if (ConsoleOut.Verbosity >= verbosity)
                Console.Error.WriteLine(exception.ToString());

            if (Out?.Verbosity >= verbosity)
                Out.WriteLine(exception.ToString());
        }

        public static bool ShouldLog(Verbosity verbosity)
        {
            return verbosity <= ConsoleOut.Verbosity
                || (Out != null && verbosity <= Out.Verbosity);
        }

        //public static void Write(char[] buffer)
        //{
        //    ConsoleOut.Write(buffer);
        //    Out?.Write(buffer);
        //}

        //public static void Write(char[] buffer, int index, int count)
        //{
        //    ConsoleOut.Write(buffer, index, count);
        //    Out?.Write(buffer, index, count);
        //}

        //public static void Write(bool value)
        //{
        //    ConsoleOut.Write(value);
        //    Out?.Write(value);
        //}

        //public static void Write(int value)
        //{
        //    ConsoleOut.Write(value);
        //    Out?.Write(value);
        //}

        //public static void Write(uint value)
        //{
        //    ConsoleOut.Write(value);
        //    Out?.Write(value);
        //}

        //public static void Write(long value)
        //{
        //    ConsoleOut.Write(value);
        //    Out?.Write(value);
        //}

        //public static void Write(ulong value)
        //{
        //    ConsoleOut.Write(value);
        //    Out?.Write(value);
        //}

        //public static void Write(float value)
        //{
        //    ConsoleOut.Write(value);
        //    Out?.Write(value);
        //}

        //public static void Write(double value)
        //{
        //    ConsoleOut.Write(value);
        //    Out?.Write(value);
        //}

        //public static void Write(decimal value)
        //{
        //    ConsoleOut.Write(value);
        //    Out?.Write(value);
        //}

        //public static void Write(string format, object arg0)
        //{
        //    ConsoleOut.Write(format, arg0);
        //    Out?.Write(format, arg0);
        //}

        //public static void Write(string format, object arg0, object arg1)
        //{
        //    ConsoleOut.Write(format, arg0, arg1);
        //    Out?.Write(format, arg0, arg1);
        //}

        //public static void Write(string format, object arg0, object arg1, object arg2)
        //{
        //    ConsoleOut.Write(format, arg0, arg1, arg2);
        //    Out?.Write(format, arg0, arg1, arg2);
        //}

        //public static void Write(string format, params object[] arg)
        //{
        //    ConsoleOut.Write(format, arg);
        //    Out?.Write(format, arg);
        //}

        //public static void WriteLine(char[] buffer)
        //{
        //    ConsoleOut.WriteLine(buffer);
        //    Out?.WriteLine(buffer);
        //}

        //public static void WriteLine(char[] buffer, int index, int count)
        //{
        //    ConsoleOut.WriteLine(buffer, index, count);
        //    Out?.WriteLine(buffer, index, count);
        //}

        //public static void WriteLine(bool value)
        //{
        //    ConsoleOut.WriteLine(value);
        //    Out?.WriteLine(value);
        //}

        //public static void WriteLine(int value)
        //{
        //    ConsoleOut.WriteLine(value);
        //    Out?.WriteLine(value);
        //}

        //public static void WriteLine(uint value)
        //{
        //    ConsoleOut.WriteLine(value);
        //    Out?.WriteLine(value);
        //}

        //public static void WriteLine(long value)
        //{
        //    ConsoleOut.WriteLine(value);
        //    Out?.WriteLine(value);
        //}

        //public static void WriteLine(ulong value)
        //{
        //    ConsoleOut.WriteLine(value);
        //    Out?.WriteLine(value);
        //}

        //public static void WriteLine(float value)
        //{
        //    ConsoleOut.WriteLine(value);
        //    Out?.WriteLine(value);
        //}

        //public static void WriteLine(double value)
        //{
        //    ConsoleOut.WriteLine(value);
        //    Out?.WriteLine(value);
        //}

        //public static void WriteLine(decimal value)
        //{
        //    ConsoleOut.WriteLine(value);
        //    Out?.WriteLine(value);
        //}

        //public static void WriteLine(string format, object arg0)
        //{
        //    ConsoleOut.WriteLine(format, arg0);
        //    Out?.WriteLine(format, arg0);
        //}

        //public static void WriteLine(string format, object arg0, object arg1)
        //{
        //    ConsoleOut.WriteLine(format, arg0, arg1);
        //    Out?.WriteLine(format, arg0, arg1);
        //}

        //public static void WriteLine(string format, object arg0, object arg1, object arg2)
        //{
        //    ConsoleOut.WriteLine(format, arg0, arg1, arg2);
        //    Out?.WriteLine(format, arg0, arg1, arg2);
        //}

        //public static void WriteLine(string format, params object[] arg)
        //{
        //    ConsoleOut.WriteLine(format, arg);
        //    Out?.WriteLine(format, arg);
        //}
    }
}
