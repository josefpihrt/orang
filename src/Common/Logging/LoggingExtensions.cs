// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable RCS0056

namespace Orang;

internal static class LoggingExtensions
{
    public static void WriteLineIf(
        this Logger logger,
        bool condition,
        Verbosity verbosity)
    {
        if (condition)
            logger.WriteLine(verbosity);
    }

    public static void WriteIf(
        this Logger logger,
        bool condition,
        string value,
        ConsoleColors colors,
        Verbosity verbosity)
    {
        if (condition)
            logger.Write(value, colors, verbosity);
    }

    public static void Write(this LogWriter writer, string? value, int startIndex, int length, Verbosity verbosity)
    {
        writer.Write(value.AsSpan(startIndex, length), verbosity);
    }

    public static void Write(this LogWriter writer, string? value, int startIndex, int length, ConsoleColors colors, Verbosity verbosity)
    {
        writer.Write(value.AsSpan(startIndex, length), colors, verbosity);
    }

    public static void WriteIf(this LogWriter writer, bool condition, string? value)
    {
        if (condition)
            writer.Write(value);
    }

    public static void WriteIf(this LogWriter writer, bool condition, string? value, Verbosity verbosity)
    {
        if (condition)
            writer.Write(value, verbosity);
    }

    public static void WriteIf(this LogWriter writer, bool condition, string? value, in ConsoleColors colors)
    {
        if (condition)
            writer.Write(value, colors);
    }

    public static void WriteIf(this LogWriter writer, bool condition, string? value, in ConsoleColors colors, Verbosity verbosity)
    {
        if (condition)
            writer.Write(value, colors, verbosity);
    }

    public static void WriteIf(this LogWriter writer, bool condition, ReadOnlySpan<char> buffer)
    {
        if (condition)
            writer.Write(buffer);
    }

    public static void WriteIf(this LogWriter writer, bool condition, ReadOnlySpan<char> buffer, Verbosity verbosity)
    {
        if (condition)
            writer.Write(buffer, verbosity);
    }

    public static void WriteIf(this LogWriter writer, bool condition, ReadOnlySpan<char> buffer, ConsoleColors colors)
    {
        if (condition)
            writer.Write(buffer, colors);
    }

    public static void WriteIf(this LogWriter writer, bool condition, ReadOnlySpan<char> buffer, ConsoleColors colors, Verbosity verbosity)
    {
        if (condition)
            writer.Write(buffer, colors, verbosity);
    }

    public static void WriteLineIf(this LogWriter writer, bool condition)
    {
        if (condition)
            writer.WriteLine();
    }

    public static void WriteLineIf(this LogWriter writer, bool condition, Verbosity verbosity)
    {
        if (condition)
            writer.WriteLine(verbosity);
    }

    public static void WriteLineIf(this LogWriter writer, bool condition, string? value)
    {
        if (condition)
            writer.WriteLine(value);
    }

    public static void WriteLineIf(this LogWriter writer, bool condition, string value, Verbosity verbosity)
    {
        if (condition)
            writer.WriteLine(value, verbosity);
    }

    public static void WriteLineIf(this LogWriter writer, bool condition, string? value, in ConsoleColors colors)
    {
        if (condition)
            writer.WriteLine(value, colors);
    }

    public static void WriteLineIf(this LogWriter writer, bool condition, string? value, in ConsoleColors colors, Verbosity verbosity)
    {
        if (condition)
            writer.WriteLine(value, colors, verbosity);
    }

    public static void WriteLineIf(this LogWriter writer, bool condition, ReadOnlySpan<char> buffer)
    {
        if (condition)
            writer.WriteLine(buffer);
    }

    public static void WriteLineIf(this LogWriter writer, bool condition, ReadOnlySpan<char> buffer, Verbosity verbosity)
    {
        if (condition)
            writer.WriteLine(buffer, verbosity);
    }

    public static void WriteLineIf(this LogWriter writer, bool condition, ReadOnlySpan<char> buffer, in ConsoleColors colors)
    {
        if (condition)
            writer.WriteLine(buffer, colors);
    }

    public static void WriteLineIf(this LogWriter writer, bool condition, ReadOnlySpan<char> buffer, in ConsoleColors colors, Verbosity verbosity)
    {
        if (condition)
            writer.WriteLine(buffer, colors, verbosity);
    }
}
