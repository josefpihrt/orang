// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal static class ConsoleHelpers
    {
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

            switch (Console.ReadLine().Trim())
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
                //case "n":
                //case "N":
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
