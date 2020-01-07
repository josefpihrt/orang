// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal static class ConsoleHelpers
    {
        public static string ReadRedirectedInput()
        {
            if (Console.IsInputRedirected)
            {
                using (Stream stream = Console.OpenStandardInput())
                using (var streamReader = new StreamReader(stream, Console.InputEncoding))
                {
                    return streamReader.ReadToEnd();
                }
            }

            return null;
        }

        public static IEnumerable<string> ReadRedirectedInputAsLines()
        {
            if (Console.IsInputRedirected)
            {
                using (Stream stream = Console.OpenStandardInput())
                using (var streamReader = new StreamReader(stream, Console.InputEncoding))
                {
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                        yield return line;
                }
            }
        }

        public static bool Question(string question, string indent = null)
        {
            return QuestionIf(true, question, indent);
        }

        public static bool QuestionIf(bool condition, string question, string indent = null)
        {
            if (!condition)
                return true;

            ConsoleOut.Write(indent);
            ConsoleOut.Write(question);
            ConsoleOut.Write(" (Y/N/C): ");

            switch (Console.ReadLine()?.Trim())
            {
                case "y":
                case "Y":
                    {
                        return true;
                    }
                case "c":
                case "C":
                    {
                        throw new OperationCanceledException();
                    }
                case null:
                    {
                        ConsoleOut.WriteLine();
                        break;
                    }
            }

            return false;
        }
    }
}
