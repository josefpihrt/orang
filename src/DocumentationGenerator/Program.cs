﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DotMarkdown;
using DotMarkdown.Docusaurus;
using DotMarkdown.Linq;
using Orang.CommandLine;
using Orang.CommandLine.Annotations;
using Orang.CommandLine.Help;
using static DotMarkdown.Linq.MFactory;

namespace Orang.Documentation;

internal static class Program
{
    private static readonly Regex _removeNewlineRegex = new(@"\ *\r?\n\ *");

    private static void Main(params string[] args)
    {
        IEnumerable<Command> commands = CommandLoader.LoadCommands(typeof(AbstractCommandLineOptions).Assembly)
            .Select(c =>
            {
                IEnumerable<CommandOption> options = c.Options
                    .Where(f => f.PropertyInfo.GetCustomAttribute<HideFromHelpAttribute>() is null)
                    .OrderBy(f => f, CommandOptionComparer.Name);

                return c.WithOptions(options);
            })
            .OrderBy(c => c.Name, StringComparer.InvariantCulture);

        var application = new CommandLineApplication(
            "orang",
            "Search, replace, rename and delete files and its content using the power of .NET regular expressions.",
            commands.OrderBy(f => f.Name, StringComparer.InvariantCulture));

        if (args.Length < 2)
        {
            Console.WriteLine("Invalid number of arguments");
            return;
        }

        string? destinationDirectoryPath = args[0];
        string? dataDirectoryPath = args[1];

        string filePath = Path.GetFullPath(Path.Combine(destinationDirectoryPath, "cli.md"));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var markdownFormat = new MarkdownFormat(
            bulletListStyle: BulletListStyle.Minus,
            tableOptions: MarkdownFormat.Default.TableOptions | TableOptions.FormatHeaderAndContent,
            angleBracketEscapeStyle: AngleBracketEscapeStyle.EntityRef);

        var settings = new MarkdownWriterSettings(markdownFormat);

        using (var sw = new StreamWriter(filePath, append: false, Encoding.UTF8))
        using (MarkdownWriter mw2 = MarkdownWriter.Create(sw, settings))
        using (var mw = new DocusaurusMarkdownWriter(mw2))
        {
            WriteFrontMatter(mw, 0, "Orang CLI");

            mw.WriteStartHeading(1);
            mw.WriteString("Orang Command Line Tool");
            mw.WriteEndHeading();
            mw.WriteString(application.Description);
            mw.WriteLine();

            mw.WriteHeading2("Commands");

            foreach (Command command in commands)
            {
                mw.WriteStartBulletItem();
                mw.WriteLink(command.DisplayName, $"cli/commands/{command.Name}");
                mw.WriteEndBulletItem();
            }

            mw.WriteLine();

            string readmeLinksFilePath = Path.Combine(dataDirectoryPath, "readme_bottom.md");

            if (File.Exists(readmeLinksFilePath))
                mw.WriteRaw(File.ReadAllText(readmeLinksFilePath));

            WriteFootNote(mw);

            Console.WriteLine(filePath);
        }

        GenerateCommands(commands, destinationDirectoryPath, settings);

        GenerateOptionValues(commands, destinationDirectoryPath, settings);

        GenerateExpressionSyntax(destinationDirectoryPath, settings);

        foreach (Command command in application.Commands)
        {
            filePath = Path.GetFullPath(Path.Combine(destinationDirectoryPath, "cli/commands", $"{command.Name}.md"));

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using (var sw = new StreamWriter(filePath, append: false, Encoding.UTF8))
            using (MarkdownWriter mw2 = MarkdownWriter.Create(sw, settings))
            using (var mw = new DocusaurusMarkdownWriter(mw2))
            {
                var writer = new DocumentationWriter(mw);

                WriteFrontMatter(mw, label: command.DisplayName);

                writer.WriteCommandHeading(command, application);
                writer.WriteCommandDescription(command);

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

                Console.WriteLine(filePath);
            }
        }
    }

    private static void GenerateCommands(IEnumerable<Command> commands, string destinationDirectoryPath, MarkdownWriterSettings settings)
    {
        string filePath = Path.Combine(destinationDirectoryPath, "cli/commands.md");

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        using (var sw = new StreamWriter(filePath, append: false, Encoding.UTF8))
        using (MarkdownWriter mw2 = MarkdownWriter.Create(sw, settings))
        using (var mw = new DocusaurusMarkdownWriter(mw2))
        {
            WriteFrontMatter(mw, 0, "Commands");

            mw.WriteHeading1("Commands");

            Table(
                TableRow("Command", "Description"),
                commands.Select(f => TableRow(Link(f.DisplayName, $"commands/{f.Name}"), f.Description)))
                .WriteTo(mw);

            WriteFootNote(mw);

            Console.WriteLine(filePath);
        }
    }

    private static void GenerateOptionValues(
        IEnumerable<Command> commands,
        string destinationDirectoryPath,
        MarkdownWriterSettings settings)
    {
        string filePath = Path.GetFullPath(Path.Combine(destinationDirectoryPath, "cli/options-values.md"));

        ImmutableArray<OptionValueProvider> providers = OptionValueProvider.GetProviders(
            commands.SelectMany(f => f.Options),
            OptionValueProviders.ProvidersByName.Select(f => f.Value))
            .ToImmutableArray();

        MDocument document = Document(
            Heading1("List of Option Values"),
            providers.Select(provider =>
            {
                return new MObject[]
                {
                    Heading2(provider.Name),
                    Table(
                        TableRow("Value", "Shortcut", "Description"),
                        provider.Values.Select(f => CreateTableRow(f, providers))),
                };
            }));

        AddFootnote(document);

        File.WriteAllText(filePath, document.ToString(settings));
    }

    private static void GenerateExpressionSyntax(
        string destinationDirectoryPath,
        MarkdownWriterSettings settings)
    {
        string filePath = Path.GetFullPath(Path.Combine(destinationDirectoryPath, "cli/expression-syntax.md"));

        MDocument document = Document(
            Heading1("Expression Syntax"),
            Table(
                TableRow("Expression", "Description"),
                HelpProvider.GetExpressionItems(includeDate: true)
                    .Select(f => TableRow(InlineCode(f.expression), f.description))));

        AddFootnote(document);

        File.WriteAllText(filePath, document.ToString(settings));
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
            value!,
            (string.IsNullOrEmpty(shortValue)) ? " " : shortValue,
            (string.IsNullOrEmpty(description)) ? " " : description);
    }

#pragma warning disable IDE0060, RCS1163
    private static void WriteFootNote(MarkdownWriter mw)
    {
    }

    private static void AddFootnote(MDocument document)
    {
    }
#pragma warning restore IDE0060, RCS1163

    private static void WriteFrontMatter(DocusaurusMarkdownWriter mw, int? position = null, string? label = null)
    {
        mw.WriteDocusaurusFrontMatter(GetItems());

        IEnumerable<(string, object)> GetItems()
        {
            if (position is not null)
                yield return ("sidebar_position", position);

            if (label is not null)
                yield return ("sidebar_label", label);
        }
    }
}
