// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal static class HelpProvider
    {
        private static readonly Regex _metaValueRegex = new Regex(@"\<\w+\>");

        public static string GetHeadingText()
        {
            return $"Orang Command-Line Tool version {typeof(Program).GetTypeInfo().Assembly.GetName().Version}";
        }

        public static string GetFooterText(string command = null)
        {
            return $"Run 'orang {command ?? "[command]"} -h' for more information on a command."
                + Environment.NewLine
                + $"Run 'orang help {command ?? "[command]"} -v' for more information on allowed values.";
        }

        public static string GetHelpText(bool includeValues = false)
        {
            using (var stringWriter = new StringWriter())
            {
                IEnumerable<Command> commands = LoadCommands();

                var helpWriter = new CommandHelpWriter(
                    stringWriter,
                    new HelpWriterOptions(includeValues: includeValues),
                    OptionValueProviders.ProvidersByName);

                helpWriter.WriteCommands(commands);

                return stringWriter.ToString();
            }
        }

        public static string GetHelpText(string commandName, bool includeValues = false)
        {
            Command command = CommandLoader.LoadCommand(typeof(HelpCommand).Assembly, commandName);

            if (command == null)
                throw new ArgumentException($"Command '{commandName}' does not exist.", nameof(commandName));

            return GetHelpText(command, includeValues);
        }

        public static string GetHelpText(Command command, bool includeValues = false)
        {
            using (var stringWriter = new StringWriter())
            {
                var helpWriter = new CommandHelpWriter(
                    stringWriter,
                    new HelpWriterOptions(includeValues: includeValues),
                    OptionValueProviders.ProvidersByName);

                command = command.WithOptions(command.Options.Sort(CompareOptions));

                helpWriter.WriteCommand(command);

                return stringWriter.ToString();
            }
        }

        public static string GetManual(bool includeValues = false)
        {
            using (var stringWriter = new StringWriter())
            {
                IEnumerable<Command> commands = LoadCommands();

                var helpWriter = new ManualHelpWriter(
                    stringWriter,
                    new HelpWriterOptions(includeValues: false),
                    OptionValueProviders.ProvidersByName);

                helpWriter.WriteCommands(commands);

                TextWriter writer = helpWriter.Writer;

                foreach (Command command in commands.Select(f => f.WithOptions(f.Options.Sort(CompareOptions))))
                {
                    WriteSeparator(writer);

                    writer.WriteLine();
                    writer.WriteLine($"Command: {command.Name}");
                    writer.WriteLine();

                    string description = command.Description;

                    if (!string.IsNullOrEmpty(description))
                    {
                        writer.WriteLine(description);
                        writer.WriteLine();
                    }

                    helpWriter.WriteCommand(command);
                }

                if (includeValues)
                {
                    WriteSeparator(writer);
                    helpWriter.WriteValues(commands);
                }

                return stringWriter.ToString();
            }

            void WriteSeparator(TextWriter writer)
            {
                writer.WriteLine();
                writer.WriteLine("-----");
            }
        }

        private static IEnumerable<Command> LoadCommands()
        {
            return CommandLoader.LoadCommands(typeof(HelpCommand).Assembly).OrderBy(f => f.Name, StringComparer.CurrentCulture);
        }

        private static int CompareOptions(CommandOption x, CommandOption y)
        {
            return StringComparer.InvariantCulture.Compare(x.Name, y.Name);
        }

        public static IEnumerable<OptionValueProvider> GetProviders(IEnumerable<Command> commands, IEnumerable<OptionValueProvider> allProviders = null)
        {
            return GetProviders2(commands.SelectMany(f => f.Options), allProviders);
        }

        private static IEnumerable<OptionValueProvider> GetProviders2(IEnumerable<CommandOption> options, IEnumerable<OptionValueProvider> allProviders = null)
        {
            ImmutableArray<OptionValueProvider> providers = GetProviders(options, allProviders).ToImmutableArray();

            return providers
                .SelectMany(f => f.Values)
                .OfType<KeyValuePairOptionValue>()
                .Select(f => f.Value)
                .Distinct()
                .Join(allProviders ?? OptionValueProviders.ProvidersByName.Select(f => f.Value), f => f, f => f.Name, (_, f) => f)
                .OrderBy(f => f.Name)
                .Concat(providers)
                .Distinct()
                .OrderBy(f => f.Name);
        }

        private static IEnumerable<OptionValueProvider> GetProviders(IEnumerable<CommandOption> options, IEnumerable<OptionValueProvider> allProviders = null)
        {
            IEnumerable<string> metaValues = options
                .SelectMany(f => _metaValueRegex.Matches(f.Description).Select(m => m.Value))
                .Concat(options.Select(f => f.MetaValue))
                .Distinct();

            return metaValues
                .Join(allProviders ?? OptionValueProviders.ProvidersByName.Select(f => f.Value), f => f, f => f.Name, (_, f) => f)
                .OrderBy(f => f.Name);
        }

        private class ManualHelpWriter : HelpWriter
        {
            public ManualHelpWriter(
                TextWriter writer,
                HelpWriterOptions options = null,
                ImmutableDictionary<string, OptionValueProvider> optionValueProviders = null) : base(writer, options, optionValueProviders?.Select(f => f.Value))
            {
            }

            public override void WriteCommands(IEnumerable<Command> commands)
            {
                WriteLine(GetHeadingText());
                WriteLine("Usage: orang [command] [arguments]");
                WriteLine();

                base.WriteCommands(commands);
            }

            public override void WriteStartCommand(Command command)
            {
                Write("Usage: orang ");
                Write(command.Name);

                foreach (CommandArgument argument in command.Arguments)
                {
                    Write(" ");

                    if (!argument.IsRequired)
                        Write("[");

                    Write(argument.Name);

                    if (!argument.IsRequired)
                        Write("]");
                }

                if (command.Options.Any())
                    Write(" [options]");

                WriteLine();
            }

            protected override void WriteValues(IEnumerable<CommandOption> options)
            {
                ImmutableArray<OptionValueProvider> providers = GetProviders2(options, OptionValueProviders).ToImmutableArray();

                WriteValues(providers);
            }
        }

        private class CommandHelpWriter : ManualHelpWriter
        {
            public CommandHelpWriter(
                TextWriter writer,
                HelpWriterOptions options = null,
                ImmutableDictionary<string, OptionValueProvider> optionValueProviders = null) : base(writer, options, optionValueProviders)
            {
            }

            public override void WriteEndOptions(Command command)
            {
                if (Options.IncludeValues)
                {
                    WriteValues(command.Options);
                }
                else
                {
                    IEnumerable<string> metaValues = GetProviders(command.Options, OptionValueProviders).Select(f => f.Name);

                    if (!metaValues.Any())
                        return;

                    WriteLine();
                    Write($"Run 'orang help {command.Name} -{OptionShortNames.Values}' to display list of allowed values for ");
                    Write(TextHelpers.Join(", ", " and ", metaValues));
                    WriteLine(".");
                }
            }

            public override void WriteEndCommands(IEnumerable<Command> commands)
            {
                WriteLine();
                WriteLine(GetFooterText());

                if (Options.IncludeValues)
                    WriteValues(commands);
            }
        }
    }
}
