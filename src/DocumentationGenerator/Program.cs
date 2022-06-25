// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DotMarkdown;
using DotMarkdown.Linq;
using Orang.CommandLine;
using Orang.CommandLine.Annotations;
using Orang.CommandLine.Help;
using static DotMarkdown.Linq.MFactory;

namespace Orang.Documentation
{
    internal static class Program
    {
        private static readonly Regex _removeNewlineRegex = new(@"\ *\r?\n\ *");

        private static void Main(params string[] args)
        {
            IEnumerable<Command> commands = CommandLoader.LoadCommands(typeof(AbstractCommandLineOptions).Assembly)
                .Select(c =>
                {
                    IEnumerable<CommandOption> options = c.Options
                        .Where(f => f.PropertyInfo.GetCustomAttribute<HideFromHelpAttribute>() == null)
                        .OrderBy(f => f, CommandOptionComparer.Name);

                    return c.WithOptions(options);
                })
                .OrderBy(c => c.Name, StringComparer.InvariantCulture);

            var application = new CommandLineApplication(
                "orang",
                "Search, replace, rename and delete files and its content using the power of .NET regular expressions.",
                commands.OrderBy(f => f.Name, StringComparer.InvariantCulture));

            string? destinationDirectoryPath = null;
            string? dataDirectoryPath = null;

            if (Debugger.IsAttached)
            {
                destinationDirectoryPath = (args.Length > 0) ? args[0] : @"..\..\..\..\..\docs\cli";
                dataDirectoryPath = @"..\..\..\data";
            }
            else
            {
                destinationDirectoryPath = args[0];
                dataDirectoryPath = @"..\src\DocumentationGenerator\data";
            }

            string readmeFilePath = Path.GetFullPath(Path.Combine(destinationDirectoryPath, "index.md"));

            var markdownFormat = new MarkdownFormat(
                tableOptions: MarkdownFormat.Default.TableOptions | TableOptions.FormatHeaderAndContent,
                angleBracketEscapeStyle: AngleBracketEscapeStyle.EntityRef);

            var settings = new MarkdownWriterSettings(
                markdownFormat);

            using (var sw = new StreamWriter(readmeFilePath, append: false, Encoding.UTF8))
            using (MarkdownWriter mw = MarkdownWriter.Create(sw, settings))
            {
                mw.WriteRaw("---");
                mw.WriteLine();
                mw.WriteRaw("sidebar_position: 0");
                mw.WriteLine();
                mw.WriteRaw("sidebar_label: Orang Command-line Tool");
                mw.WriteLine();
                mw.WriteRaw("---");
                mw.WriteLine();

                mw.WriteStartHeading(1);
                mw.WriteString("Orang Command-line Tool");
                mw.WriteRaw(" <img align=\"left\" src=\"../../images/icon48.png\" />");
                mw.WriteEndHeading();
                mw.WriteString(application.Description);
                mw.WriteLine();

                mw.WriteHeading2("Commands");

                Table(
                    TableRow("Command", "Description"),
                    application
                        .Commands
                        .Select(f => TableRow(Link(f.DisplayName, $"Commands/{f.Name}.md"), f.Description)))
                    .WriteTo(mw);

                mw.WriteLine();

                string readmeLinksFilePath = Path.Combine(dataDirectoryPath, "readme_bottom.md");

                if (File.Exists(readmeLinksFilePath))
                    mw.WriteRaw(File.ReadAllText(readmeLinksFilePath));

                WriteFootNote(mw);

                Console.WriteLine(readmeFilePath);
            }

            string valuesFilePath = Path.GetFullPath(Path.Combine(destinationDirectoryPath, "OptionValues.md"));

            ImmutableArray<OptionValueProvider> providers = OptionValueProvider.GetProviders(
                commands.SelectMany(f => f.Options),
                OptionValueProviders.ProvidersByName.Select(f => f.Value))
                .ToImmutableArray();

            MDocument document = Document(
                Heading1("List of Option Values"),
                //BulletList(providers.Select(f => Link(
                //    f.Name,
                //    MarkdownHelpers.CreateGitHubHeadingLink(f.Name)))),
                providers.Select(provider =>
                {
                    return new MObject[]
                    {
                        Heading2(provider.Name),
                        Table(
                            TableRow("Value", " ", "Description"),
                            provider.Values.Select(f => CreateTableRow(f, providers)))
                    };
                }));

            document.Add(
                Heading2("Expression Syntax"),
                Table(
                    TableRow("Expression", "Description"),
                    HelpProvider.GetExpressionItems(includeDate: true)
                        .Select(f => TableRow(InlineCode(f.expression), f.description))));

            AddFootnote(document);

            File.WriteAllText(valuesFilePath, document.ToString(markdownFormat));

            foreach (Command command in application.Commands)
            {
                readmeFilePath = Path.GetFullPath(Path.Combine(destinationDirectoryPath, "Commands", $"{command.Name}.md"));

                using (var sw = new StreamWriter(readmeFilePath, append: false, Encoding.UTF8))
                using (MarkdownWriter mw = MarkdownWriter.Create(sw, settings))
                {
                    var writer = new DocumentationWriter(mw);

                    mw.WriteRaw("---");
                    mw.WriteLine();
                    mw.WriteRaw($"sidebar_label: {command.DisplayName}");
                    mw.WriteLine();
                    mw.WriteRaw("---");
                    mw.WriteLine();

                    writer.WriteCommandHeading(command, application);
                    writer.WriteCommandDescription(command);

                    //mw.WriteLink("Home", "index.md#readme");

                    //foreach (string section in new[] { "Synopsis", "Arguments", "Options", "Samples" })
                    //{
                    //    mw.WriteString(" ");
                    //    mw.WriteCharEntity((char)0x2022);
                    //    mw.WriteString(" ");
                    //    mw.WriteLink(section, "#" + section);
                    //}

                    //mw.WriteLine();

                    writer.WriteCommandSynopsis(command, application);
                    writer.WriteArguments(command.Arguments);
                    writer.WriteOptions(command.Options);

                    string samplesFilePath = Path.Combine(dataDirectoryPath, command.Name + "_bottom.md");

                    if (File.Exists(samplesFilePath))
                    {
                        string content = File.ReadAllText(samplesFilePath);
                        mw.WriteRaw(content);
                    }

                    WriteFootNote(mw);

                    Console.WriteLine(readmeFilePath);
                }
            }
        }

        private static MTableRow CreateTableRow(
            OptionValue optionValue,
            ImmutableArray<OptionValueProvider> providers)
        {
            object? value = null;
            string? shortValue = null;

            switch (optionValue)
            {
                case SimpleOptionValue simpleOptionValue:
                    {
                        value = simpleOptionValue.Value;
                        shortValue = simpleOptionValue.ShortValue;
                        break;
                    }
                case KeyValuePairOptionValue keyValuePairOptionValue:
                    {
                        string value2 = keyValuePairOptionValue.Value;

                        if (OptionValueProvider.MetaValueRegex.IsMatch(value2)
                            && providers.Any(f => f.Name == value2))
                        {
                            value = Inline(
                                $"{keyValuePairOptionValue.Key}=",
                                Link(value2, MarkdownHelpers.CreateGitHubHeadingLink(value2)));
                        }
                        else
                        {
                            value = $"{keyValuePairOptionValue.Key}={value2}";
                        }

                        shortValue = keyValuePairOptionValue.ShortKey;
                        break;
                    }
            }

            string description = _removeNewlineRegex.Replace(optionValue.Description ?? "", " ");

            return TableRow(
                value,
                (string.IsNullOrEmpty(shortValue)) ? " " : shortValue,
                (string.IsNullOrEmpty(description)) ? " " : description);
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

        private static void AddFootnote(MDocument document)
        {
            document.Add(Italic("(Generated with ", Link("DotMarkdown", "http://github.com/JosefPihrt/DotMarkdown"), ")"));
        }
    }
}
