// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang
{
    internal interface ILogWriter : IDisposable
    {
        void Write(char value, ConsoleColors colors);

        void Write(char value, Verbosity verbosity);

        void Write(char value, ConsoleColors colors, Verbosity verbosity);

        void Write(char value, int repeatCount, ConsoleColors colors);

        void Write(char value, int repeatCount, Verbosity verbosity);

        void Write(char value, int repeatCount, ConsoleColors colors, Verbosity verbosity);

        void Write(string? value, ConsoleColors colors);

        void Write(string? value, Verbosity verbosity);

        void Write(string? value, ConsoleColors colors, Verbosity verbosity);

        void Write(ReadOnlySpan<char> buffer, ConsoleColors colors);

        void Write(ReadOnlySpan<char> buffer, Verbosity verbosity);

        void Write(ReadOnlySpan<char> buffer, ConsoleColors colors, Verbosity verbosity);

        void WriteLine(Verbosity verbosity);

        void WriteLine(string? value, ConsoleColors colors);

        void WriteLine(string? value, Verbosity verbosity);

        void WriteLine(string? value, ConsoleColors colors, Verbosity verbosity);

        void WriteLine(ReadOnlySpan<char> buffer, ConsoleColors colors);

        void WriteLine(ReadOnlySpan<char> buffer, Verbosity verbosity);

        void WriteLine(ReadOnlySpan<char> buffer, ConsoleColors colors, Verbosity verbosity);
    }
}
