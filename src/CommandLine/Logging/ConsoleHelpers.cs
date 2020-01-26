// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal static class ConsoleHelpers
    {
        private static readonly ImmutableDictionary<string, DialogResult> _dialogResultMap = CreateDialogResultMap();
        private static readonly ImmutableDictionary<string, DialogResult> _yesNoCancelMap = CreateYesNoCancelMap();

        private static ImmutableDictionary<string, DialogResult> CreateDialogResultMap()
        {
            ImmutableDictionary<string, DialogResult>.Builder builder = ImmutableDictionary.CreateBuilder<string, DialogResult>();

            builder.Add("y", DialogResult.Yes);
            builder.Add("yes", DialogResult.Yes);
            builder.Add("ya", DialogResult.YesToAll);
            builder.Add("yes to all", DialogResult.YesToAll);
            builder.Add("n", DialogResult.No);
            builder.Add("no", DialogResult.No);
            builder.Add("na", DialogResult.NoToAll);
            builder.Add("no to all", DialogResult.NoToAll);
            builder.Add("c", DialogResult.Cancel);
            builder.Add("cancel", DialogResult.Cancel);

            return builder.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        }

        private static ImmutableDictionary<string, DialogResult> CreateYesNoCancelMap()
        {
            ImmutableDictionary<string, DialogResult>.Builder builder = ImmutableDictionary.CreateBuilder<string, DialogResult>();

            builder.Add("y", DialogResult.Yes);
            builder.Add("yes", DialogResult.Yes);
            builder.Add("n", DialogResult.No);
            builder.Add("no", DialogResult.No);
            builder.Add("c", DialogResult.Cancel);
            builder.Add("cancel", DialogResult.Cancel);

            return builder.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
        }

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

            switch (QuestionWithResult(question, " (Y/N/C): ", "Y (Yes), N (No), C (Cancel)", _yesNoCancelMap, indent))
            {
                case DialogResult.Yes:
                    return true;
                case DialogResult.No:
                case DialogResult.None:
                    return false;
                case DialogResult.Cancel:
                    throw new OperationCanceledException();
                default:
                    throw new InvalidOperationException();
            }
        }

        public static DialogResult QuestionWithResult(string question, string indent = null)
        {
            return QuestionWithResult(question, " (Y[A]/N[A]/C): ", "Y (Yes), YA (Yes to All), N (No), NA (No to All), C (Cancel)", _dialogResultMap, indent);
        }

        private static DialogResult QuestionWithResult(
            string question,
            string suffix,
            string helpText,
            ImmutableDictionary<string, DialogResult> map,
            string indent)
        {
            while (true)
            {
                ConsoleOut.Write(indent);
                ConsoleOut.Write(question);
                ConsoleOut.Write(suffix);

                string s = Console.ReadLine()?.Trim();

                if (s != null)
                {
                    if (s.Length == 0)
                        return DialogResult.None;

                    if (map.TryGetValue(s, out DialogResult dialogResult))
                        return dialogResult;
                }
                else
                {
                    ConsoleOut.WriteLine();
                    return DialogResult.None;
                }

                ConsoleOut.Write(indent);
                ConsoleOut.WriteLine($"Value '{s}' is invalid. Allowed values are: {helpText}");
            }
        }
    }
}
