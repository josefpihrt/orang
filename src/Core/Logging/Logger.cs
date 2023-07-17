// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Orang;

internal class Logger : TextWriter, ILogWriter
{
    public Logger(LogWriter consoleOut, LogWriter? @out)
    {
        ConsoleOut = consoleOut;
        Out = @out;
    }

    public override Encoding Encoding => throw new NotSupportedException();

    public LogWriter ConsoleOut { get; }

    public LogWriter? Out { get; set; }

    public void Write(char value, ConsoleColors colors)
    {
        ConsoleOut.Write(value, colors);
        Out?.Write(value, colors);
    }

    public void Write(char value, Verbosity verbosity)
    {
        ConsoleOut.Write(value, verbosity);
        Out?.Write(value, verbosity);
    }

    public void Write(char value, ConsoleColors colors, Verbosity verbosity)
    {
        ConsoleOut.Write(value, colors, verbosity);
        Out?.Write(value, colors, verbosity);
    }

    public void Write(char value, int repeatCount)
    {
        ConsoleOut.Write(value, repeatCount);
        Out?.Write(value, repeatCount);
    }

    public void Write(char value, int repeatCount, ConsoleColors colors)
    {
        ConsoleOut.Write(value, repeatCount, colors);
        Out?.Write(value, repeatCount, colors);
    }

    public void Write(char value, int repeatCount, Verbosity verbosity)
    {
        ConsoleOut.Write(value, repeatCount, verbosity);
        Out?.Write(value, repeatCount, verbosity);
    }

    public void Write(char value, int repeatCount, ConsoleColors colors, Verbosity verbosity)
    {
        ConsoleOut.Write(value, repeatCount, colors, verbosity);
        Out?.Write(value, repeatCount, colors, verbosity);
    }

    public void Write(string? value, ConsoleColors colors)
    {
        ConsoleOut.Write(value, colors);
        Out?.Write(value, colors);
    }

    public void Write(string? value, Verbosity verbosity)
    {
        ConsoleOut.Write(value, verbosity);
        Out?.Write(value, verbosity);
    }

    public void Write(string? value, ConsoleColors colors, Verbosity verbosity)
    {
        ConsoleOut.Write(value, colors, verbosity);
        Out?.Write(value, colors, verbosity);
    }

    public void Write(string? value, int startIndex, int length)
    {
        ConsoleOut.Write(value.AsSpan(startIndex, length));
        Out?.Write(value.AsSpan(startIndex, length));
    }

    public void Write(string? value, int startIndex, int length, ConsoleColors colors)
    {
        ConsoleOut.Write(value.AsSpan(startIndex, length), colors);
        Out?.Write(value.AsSpan(startIndex, length), colors);
    }

    public void Write(string? value, int startIndex, int length, Verbosity verbosity)
    {
        ConsoleOut.Write(value.AsSpan(startIndex, length), verbosity);
        Out?.Write(value.AsSpan(startIndex, length), verbosity);
    }

    public void Write(string? value, int startIndex, int length, ConsoleColors colors, Verbosity verbosity)
    {
        ConsoleOut.Write(value.AsSpan(startIndex, length), colors, verbosity);
        Out?.Write(value.AsSpan(startIndex, length), colors, verbosity);
    }

    public void Write(ReadOnlySpan<char> buffer, ConsoleColors colors)
    {
        ConsoleOut.Write(buffer, colors);
        Out?.Write(buffer, colors);
    }

    public void Write(ReadOnlySpan<char> buffer, Verbosity verbosity)
    {
        ConsoleOut.Write(buffer, verbosity);
        Out?.Write(buffer, verbosity);
    }

    public void Write(ReadOnlySpan<char> buffer, ConsoleColors colors, Verbosity verbosity)
    {
        ConsoleOut.Write(buffer, colors, verbosity);
        Out?.Write(buffer, colors, verbosity);
    }

    public void WriteLine(Verbosity verbosity)
    {
        ConsoleOut.WriteLine(verbosity);
        Out?.WriteLine(verbosity);
    }

    public void WriteLine(string? value, ConsoleColors colors)
    {
        ConsoleOut.WriteLine(value, colors);
        Out?.WriteLine(value, colors);
    }

    public void WriteLine(string? value, Verbosity verbosity)
    {
        ConsoleOut.WriteLine(value, verbosity);
        Out?.WriteLine(value, verbosity);
    }

    public void WriteLine(string? value, ConsoleColors colors, Verbosity verbosity)
    {
        ConsoleOut.WriteLine(value, colors, verbosity);
        Out?.WriteLine(value, colors, verbosity);
    }

    public void WriteLine(ReadOnlySpan<char> buffer, ConsoleColors colors)
    {
        ConsoleOut.WriteLine(buffer, colors);
        Out?.WriteLine(buffer, colors);
    }

    public void WriteLine(ReadOnlySpan<char> buffer, Verbosity verbosity)
    {
        ConsoleOut.WriteLine(buffer, verbosity);
        Out?.WriteLine(buffer, verbosity);
    }

    public void WriteLine(ReadOnlySpan<char> buffer, ConsoleColors colors, Verbosity verbosity)
    {
        ConsoleOut.WriteLine(buffer, colors, verbosity);
        Out?.WriteLine(buffer, colors, verbosity);
    }

    public override void Write(bool value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
    }

    public override void Write(char value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
    }

    public override void Write(char[]? buffer)
    {
        ConsoleOut.Write(buffer);
        Out?.Write(buffer);
    }

    public override void Write(char[] buffer, int index, int count)
    {
        ConsoleOut.Write(buffer, index, count);
        Out?.Write(buffer, index, count);
    }

    public override void Write(decimal value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
    }

    public override void Write(double value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
    }

    public override void Write(int value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
    }

    public override void Write(long value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
    }

    public override void Write(object? value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
    }

    public override void Write(ReadOnlySpan<char> buffer)
    {
        ConsoleOut.Write(buffer);
        Out?.Write(buffer);
    }

    public override void Write(float value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
    }

    public override void Write(string? value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
    }

    public override void Write(string format, object? arg0)
    {
        Debug.Fail("");
        ConsoleOut.Write(format, arg0);
        Out?.Write(format, arg0);
    }

    public override void Write(string format, object? arg0, object? arg1)
    {
        Debug.Fail("");
        ConsoleOut.Write(format, arg0, arg1);
        Out?.Write(format, arg0, arg1);
    }

    public override void Write(string format, object? arg0, object? arg1, object? arg2)
    {
        Debug.Fail("");
        ConsoleOut.Write(format, arg0, arg1, arg2);
        Out?.Write(format, arg0, arg1, arg2);
    }

    public override void Write(string format, params object?[] arg)
    {
        Debug.Fail("");
        ConsoleOut.Write(format, arg);
        Out?.Write(format, arg);
    }

    public override void Write(uint value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
    }

    public override void Write(ulong value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
    }

    public override Task WriteAsync(char value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(char[] buffer, int index, int count)
    {
        ConsoleOut.Write(buffer, index, count);
        Out?.Write(buffer, index, count);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        ConsoleOut.Write(buffer);
        Out?.Write(buffer);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(string? value)
    {
        ConsoleOut.Write(value);
        Out?.Write(value);
        return Task.CompletedTask;
    }

    public override void WriteLine()
    {
        ConsoleOut.WriteLine();
        Out?.WriteLine();
    }

    public override void WriteLine(bool value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
    }

    public override void WriteLine(char value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
    }

    public override void WriteLine(char[]? buffer)
    {
        ConsoleOut.WriteLine(buffer);
        Out?.WriteLine(buffer);
    }

    public override void WriteLine(char[] buffer, int index, int count)
    {
        ConsoleOut.WriteLine(buffer, index, count);
        Out?.WriteLine(buffer, index, count);
    }

    public override void WriteLine(decimal value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
    }

    public override void WriteLine(double value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
    }

    public override void WriteLine(int value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
    }

    public override void WriteLine(long value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
    }

    public override void WriteLine(object? value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
    }

    public override void WriteLine(ReadOnlySpan<char> buffer)
    {
        ConsoleOut.WriteLine(buffer);
        Out?.WriteLine(buffer);
    }

    public override void WriteLine(float value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
    }

    public override void WriteLine(string? value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
    }

    public override void WriteLine(string format, object? arg0)
    {
        Debug.Fail("");
        ConsoleOut.WriteLine(format, arg0);
        Out?.WriteLine(format, arg0);
    }

    public override void WriteLine(string format, object? arg0, object? arg1)
    {
        Debug.Fail("");
        ConsoleOut.WriteLine(format, arg0, arg1);
        Out?.WriteLine(format, arg0, arg1);
    }

    public override void WriteLine(string format, object? arg0, object? arg1, object? arg2)
    {
        Debug.Fail("");
        ConsoleOut.WriteLine(format, arg0, arg1, arg2);
        Out?.WriteLine(format, arg0, arg1, arg2);
    }

    public override void WriteLine(string format, params object?[] arg)
    {
        Debug.Fail("");
        ConsoleOut.WriteLine(format, arg);
        Out?.WriteLine(format, arg);
    }

    public override void WriteLine(uint value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
    }

    public override void WriteLine(ulong value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
    }

    public override Task WriteLineAsync()
    {
        ConsoleOut.WriteLine();
        Out?.WriteLine();
        return Task.CompletedTask;
    }

    public override Task WriteLineAsync(char value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
        return Task.CompletedTask;
    }

    public override Task WriteLineAsync(char[] buffer, int index, int count)
    {
        ConsoleOut.WriteLine(buffer, index, count);
        Out?.WriteLine(buffer, index, count);
        return Task.CompletedTask;
    }

    public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        ConsoleOut.WriteLine(buffer);
        Out?.WriteLine(buffer);
        return Task.CompletedTask;
    }

    public override Task WriteLineAsync(string? value)
    {
        ConsoleOut.WriteLine(value);
        Out?.WriteLine(value);
        return Task.CompletedTask;
    }

    public bool ShouldWrite(Verbosity verbosity)
    {
        return verbosity <= ConsoleOut.Verbosity
            || (Out is not null && verbosity <= Out.Verbosity);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ConsoleOut.Dispose();
            Out?.Dispose();
        }
    }
}
