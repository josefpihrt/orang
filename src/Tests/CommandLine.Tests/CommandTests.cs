// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Orang.CommandLine.Tests
{
    public static class CommandTests
    {
        private static readonly Regex _orangNameRegex = new Regex(@"\Aorang ");

        [Fact]
        public static void Test()
        {
            foreach (string commandPath in Directory.EnumerateFiles(@"..\..\..\Commands", "*.cmd"))
            {
                string command = File.ReadAllText(commandPath);

                command = _orangNameRegex.Replace(command, "");

                using (var sw = new StringWriter())
                {
                    string[] s = CommandLineHelpers.SplitArgs(command);

                    TextWriter @out = Console.Out;

                    Console.SetOut(sw);

                    Program.Main(s);

                    Console.SetOut(@out);

                    string expected = File.ReadAllText(Path.ChangeExtension(commandPath, "txt"));

                    string actual = sw.ToString();

                    Assert.Equal(expected, actual);
                }
            }
        }
    }
}
