// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DotMarkdown;
using Orang.CommandLine;

namespace Orang.Documentation
{
    internal static class Program
    {
        private static void Main(params string[] args)
        {
            IEnumerable<Command> commands = CommandLoader.LoadCommands(typeof(CommandLoader).Assembly)
                .Select(c => c.WithOptions(c.Options.OrderBy(f => f, CommandOptionComparer.Instance)))
                .OrderBy(c => c.Name, StringComparer.InvariantCulture);

            var application = new CommandLineApplication(
                "orang",
                "Search, replace, rename and delete files and its content using the power or .NET regular expressions.",
                commands.OrderBy(f => f.Name, StringComparer.InvariantCulture));

            string rootPath = null;
#if DEBUG
            rootPath = (args.Length > 0) ? args[0] : @"..\..\..\..\..\docs\cli";
#else
            rootPath = args[0];
#endif

            string filePath = Path.GetFullPath(Path.Combine(rootPath, "README.md"));

            using (var sw = new StreamWriter(filePath, append: false, Encoding.UTF8))
            using (MarkdownWriter mw = MarkdownWriter.Create(sw))
            {
                mw.WriteHeading1("Orang Command-Line Interface");
                mw.WriteString(application.Description);
                mw.WriteLine();

                mw.WriteHeading2("Commands");

                foreach (Command command in application.Commands)
                {
                    mw.WriteStartBulletItem();
                    mw.WriteLink(command.Name, command.Name + "-command.md");
                    mw.WriteEndBulletItem();
                }

                mw.WriteLine();
                WriteFootNote(mw);

                Console.WriteLine(filePath);
            }

            foreach (Command command in application.Commands)
            {
                filePath = Path.GetFullPath(Path.Combine(rootPath, $"{command.Name}-command.md"));

                using (var sw = new StreamWriter(filePath, append: false, Encoding.UTF8))
                using (MarkdownWriter mw = MarkdownWriter.Create(sw))
                {
                    var writer = new DocumentationWriter(mw);

                    writer.WriteCommandHeading(command, application);
                    writer.WriteCommandDescription(command);
                    writer.WriteCommandSynopsis(command, application);
                    writer.WriteArguments(command.Arguments);
                    writer.WriteOptions(command.Options);

                    string sampleFilePath = Path.Combine("samples", command.Name + ".md");

                    if (File.Exists(sampleFilePath))
                    {
                        string content = File.ReadAllText(sampleFilePath);
                        mw.WriteHeading2("Samples");
                        mw.WriteRaw(content);
                    }

                    WriteFootNote(mw);

                    Console.WriteLine(filePath);
                }
            }

            Console.WriteLine("Done");

            if (Debugger.IsAttached)
                Console.ReadKey();
        }

        private static void WriteFootNote(MarkdownWriter mw)
        {
            mw.WriteLine();
            mw.WriteStartItalic();
            mw.WriteString("(Generated with ");
            mw.WriteLink("DotMarkdown", "http://github.com/JosefPihrt/DotMarkdown");
            mw.WriteString(")");
            mw.WriteEndItalic();
        }
    }
}
