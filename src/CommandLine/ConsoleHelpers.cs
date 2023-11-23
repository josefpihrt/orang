// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orang.CommandLine;

internal static class ConsoleHelpers
{
    public static void WaitForKeyPress(string? message = null)
    {
        if (Console.IsInputRedirected)
            return;

        Console.Write(message ?? "Press any key to continue...");
        Console.ReadKey();
        Console.WriteLine();
    }

    public static string? ReadRedirectedInput()
    {
        if (Console.IsInputRedirected)
        {
            using (Stream stream = Console.OpenStandardInput())
            using (var streamReader = new StreamReader(stream, Console.InputEncoding))
            {
                Task<string> readToEndTask = streamReader.ReadToEndAsync();

                // https://github.com/dotnet/runtime/issues/95079
                if (!readToEndTask.Wait(TimeSpan.FromMilliseconds(500)))
                    return null;

                return readToEndTask.Result;
            }
        }

        return null;
    }

    public static ImmutableArray<string> ReadRedirectedInputAsLines()
    {
        if (Console.IsInputRedirected)
        {
            ImmutableArray<string>.Builder lines = ImmutableArray.CreateBuilder<string>();

            using (Stream stream = Console.OpenStandardInput())
            using (var streamReader = new StreamReader(stream, Console.InputEncoding))
            {
                Task<string?> readLineTask = streamReader.ReadLineAsync();

                // https://github.com/dotnet/runtime/issues/95079
                if (!readLineTask.Wait(TimeSpan.FromMilliseconds(500)))
                    return default;

                if (readLineTask.Result is null)
                    return ImmutableArray<string>.Empty;

                string? line;

                while ((line = streamReader.ReadLine()) is not null)
                    lines.Add(line);
            }

            return lines.ToImmutableArray();
        }

        return ImmutableArray<string>.Empty;
    }

    public static bool AskToExecute(string text, string? indent = null)
    {
        switch (Ask(
            text,
            " (Y/N/C): ",
            "Y (Yes), N (No), C (Cancel)",
            DialogResultMap.Yes_No_Cancel,
            indent,
            throwOnCancel: true))
        {
            case DialogResult.Yes:
                return true;
            case DialogResult.No:
            case DialogResult.None:
                return false;
            default:
                throw new InvalidOperationException();
        }
    }

    public static DialogResult AskToContinue(string indent)
    {
        return Ask(
            question: "Continue?",
            suffix: " (Y[A]/C): ",
            helpText: "Y (Yes), YA (Yes to All), C (Cancel)",
            map: DialogResultMap.Yes_YesToAll_Cancel,
            indent: indent,
            throwOnCancel: true);
    }

    public static DialogResult Ask(string question, string? indent = null)
    {
        return Ask(
            question,
            " (Y[A]/N[A]/C): ",
            "Y (Yes), YA (Yes to All), N (No), NA (No to All), C (Cancel)",
            DialogResultMap.All,
            indent);
    }

    private static DialogResult Ask(
        string question,
        string suffix,
        string helpText,
        ImmutableDictionary<string, DialogResult> map,
        string? indent,
        bool throwOnCancel = false)
    {
        while (true)
        {
            Console.Write(indent);
            Console.Write(question);
            Console.Write(suffix);

            string? s = null;
            var isCanceled = false;
            ConsoleCancelEventHandler? handler = null;

            try
            {
                if (throwOnCancel)
                {
                    handler = (s, e) => isCanceled = true;
                    Console.CancelKeyPress += handler;
                }

                s = Console.ReadLine()?.Trim();
            }
            finally
            {
                if (handler is not null)
                    Console.CancelKeyPress -= handler;
            }

            Thread.Sleep(50);

            if (throwOnCancel
                && isCanceled)
            {
                Console.WriteLine();
                throw new OperationCanceledException();
            }

            if (s is not null)
            {
                if (s.Length == 0)
                    return DialogResult.None;

                if (map.TryGetValue(s, out DialogResult dialogResult))
                {
                    if (dialogResult == DialogResult.Cancel
                        && throwOnCancel)
                    {
                        throw new OperationCanceledException();
                    }

                    return dialogResult;
                }
            }
            else
            {
                Console.WriteLine();
                return DialogResult.None;
            }

            Console.Write(indent);
            Console.WriteLine($"Value '{s}' is invalid. Allowed values are: {helpText}");
        }
    }

    public static string ReadUserInput(string defaultValue, string? prompt = null)
    {
        bool treatControlCAsInput = Console.TreatControlCAsInput;

        try
        {
            Console.TreatControlCAsInput = true;

            return ReadUserInput(defaultValue, prompt ?? "", -1);
        }
        finally
        {
            Console.TreatControlCAsInput = treatControlCAsInput;
        }
    }

    public static string ReadUserInput(string defaultValue, string prompt, int position)
    {
        prompt ??= "";

        if (position > prompt.Length)
            throw new ArgumentOutOfRangeException(nameof(position), position, "");

        Console.Write(prompt);

        int initTop = Console.CursorTop;
        int initLeft = Console.CursorLeft;

        List<char> buffer = defaultValue.ToList();

        Console.Write(defaultValue);

        if (position >= 0)
            MoveCursorLeft(defaultValue.Length - position);

        ConsoleKeyInfo keyInfo = Console.ReadKey(true);

        while (keyInfo.Key != ConsoleKey.Enter)
        {
#if DEBUG
            if (keyInfo.KeyChar == '\0')
            {
                Debug.Write(keyInfo.Key);
            }
            else
            {
                Debug.Write((int)keyInfo.KeyChar);

                if (keyInfo.KeyChar >= 32)
                {
                    Debug.Write(" ");
                    Debug.Write(keyInfo.KeyChar);
                }
            }

            if (keyInfo.Modifiers != 0)
            {
                Debug.Write(" ");
                Debug.Write(keyInfo.Modifiers);
            }

            Debug.WriteLine("");
#endif
            switch (keyInfo.Key)
            {
                case ConsoleKey.LeftArrow:
                    {
                        int index = GetIndex();

                        if (index == 0)
                            break;

                        if (keyInfo.Modifiers == ConsoleModifiers.Control)
                        {
                            int i = index - 1;
                            while (i > 0)
                            {
                                if (char.IsLetterOrDigit(buffer[i])
                                    && !char.IsLetterOrDigit(buffer[i - 1]))
                                {
                                    break;
                                }

                                i--;
                            }

                            MoveCursorLeft(index - i);
                            break;
                        }

                        if (Console.CursorLeft == 0)
                        {
                            Console.SetCursorPosition(Console.WindowWidth - 1, Console.CursorTop - 1);
                        }
                        else
                        {
                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        }

                        break;
                    }
                case ConsoleKey.RightArrow:
                    {
                        int index = GetIndex();

                        if (index == buffer.Count)
                            break;

                        if (keyInfo.Modifiers == ConsoleModifiers.Control)
                        {
                            int i = index + 1;
                            while (i < buffer.Count - 1)
                            {
                                if (char.IsLetterOrDigit(buffer[i])
                                    && !char.IsLetterOrDigit(buffer[i + 1]))
                                {
                                    break;
                                }

                                i++;
                            }

                            MoveCursorRight(i + 1 - index);
                            break;
                        }

                        if (Console.CursorLeft == Console.WindowWidth - 1)
                        {
                            Console.SetCursorPosition(0, Console.CursorTop + 1);
                        }
                        else
                        {
                            Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                        }

                        break;
                    }
                case ConsoleKey.Home:
                    {
                        Console.SetCursorPosition(initLeft, initTop);
                        break;
                    }
                case ConsoleKey.End:
                    {
                        MoveCursorRight(buffer.Count - GetIndex());
                        break;
                    }
                case ConsoleKey.Backspace:
                    {
                        if (buffer.Count == 0)
                            break;

                        int index = GetIndex();

                        if (index == 0)
                            break;

                        buffer.RemoveAt(index - 1);

                        int left = Console.CursorLeft - 1;
                        int top = Console.CursorTop;

                        if (Console.CursorLeft == 0)
                        {
                            left = Console.WindowWidth - 1;
                            top--;
                        }

                        Console.SetCursorPosition(left, top);

                        char[] text = buffer.Skip(index - 1).Append(' ').ToArray();
                        Console.Write(text);

                        Console.SetCursorPosition(left, top);
                        break;
                    }
                case ConsoleKey.Delete:
                    {
                        if (buffer.Count == 0)
                            break;

                        int index = GetIndex();

                        if (buffer.Count == index)
                            break;

                        buffer.RemoveAt(index);

                        int left = Console.CursorLeft;
                        int top = Console.CursorTop;

                        char[] text = buffer.Skip(index).Append(' ').ToArray();
                        Console.Write(text);

                        Console.SetCursorPosition(left, top);

                        break;
                    }
                case ConsoleKey.Escape:
                    {
                        Reset(useDefaultValue: buffer.Count == 0);
                        break;
                    }
                case ConsoleKey.PageDown:
                    {
                        if (keyInfo.Modifiers != ConsoleModifiers.Control)
                            Reset();

                        break;
                    }
                case ConsoleKey.UpArrow:
                    {
                        Reset();
                        break;
                    }
                default:
                    {
                        char ch = keyInfo.KeyChar;

                        // ctrl+c
                        if (keyInfo.Modifiers == ConsoleModifiers.Control
                            && ch == 3)
                        {
                            Console.WriteLine();
                            throw new OperationCanceledException();
                        }

                        if (ch < 32)
                            break;

                        int index = GetIndex();

                        buffer.Insert(index, ch);

                        int left = Console.CursorLeft;
                        int top = Console.CursorTop;

                        char[] text = buffer.Skip(index).ToArray();
                        Console.Write(text);

                        if (left == Console.WindowWidth - 1)
                        {
                            Console.SetCursorPosition(0, top + 1);
                        }
                        else
                        {
                            Console.SetCursorPosition(left + 1, top);
                        }

                        break;
                    }
            }

            keyInfo = Console.ReadKey(true);
        }

        Console.WriteLine();

        return new string(buffer.ToArray());

        int GetIndex()
        {
            if (Console.CursorTop == initTop)
                return Console.CursorLeft - initLeft;

            int index = Console.WindowWidth - initLeft;
            index += (Console.CursorTop - initTop - 1) * Console.WindowWidth;
            index += Console.CursorLeft;

            return index;
        }

        static void MoveCursorLeft(int count)
        {
            int left = Console.CursorLeft;
            int top = Console.CursorTop;

            while (true)
            {
                if (count < left)
                {
                    left -= count;
                    break;
                }
                else
                {
                    count -= left;
                    left = Console.WindowWidth;
                    top--;
                }
            }

            Console.SetCursorPosition(left, top);
        }

        static void MoveCursorRight(int offset)
        {
            int left = Console.CursorLeft;
            int top = Console.CursorTop;

            while (true)
            {
                int right = Console.WindowWidth - left;

                if (offset < right)
                {
                    left += offset;
                    break;
                }
                else
                {
                    offset -= right;
                    left = 0;
                    top++;
                }
            }

            Console.SetCursorPosition(left, top);
        }

        void Reset(bool useDefaultValue = false)
        {
            Console.SetCursorPosition(initLeft, initTop);
            Console.Write(new string(' ', buffer.Count));
            Console.SetCursorPosition(initLeft, initTop);

            if (useDefaultValue)
            {
                Console.Write(defaultValue);
                buffer = defaultValue.ToList();

                if (position >= 0)
                    MoveCursorLeft(defaultValue.Length - position);
            }
            else
            {
                buffer.Clear();
            }
        }
    }
}
