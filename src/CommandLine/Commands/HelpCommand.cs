// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal class HelpCommand : AbstractCommand<HelpCommandOptions>
    {
        public HelpCommand(HelpCommandOptions options) : base(options)
        {
        }

        protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            try
            {
                WriteHelp(
                    commandName: Options.Command,
                    manual: Options.Manual,
                    includeValues: ConsoleOut.Verbosity > Verbosity.Normal);
            }
            catch (ArgumentException ex)
            {
                WriteError(ex);
                return CommandResult.Fail;
            }

            return CommandResult.Success;
        }

        private static void WriteHelp(
            string commandName,
            bool manual,
            bool includeValues,
            Filter filter = null)
        {
            if (commandName != null)
            {
                Command command = CommandLoader.LoadCommand(typeof(HelpCommand).Assembly, commandName);

                if (command == null)
                    throw new ArgumentException($"Command '{commandName}' does not exist.", nameof(commandName));

                WriteHelp(command, includeValues: includeValues, filter: filter);
            }
            else if (manual)
            {
                WriteManual(includeValues: includeValues, filter: filter);
            }
            else
            {
                WriteHelp(includeValues: includeValues, filter: filter);
            }
        }

        public static void WriteHelp(Command command = null, bool includeValues = false, Filter filter = null)
        {
            if (command != null)
            {
                var helpWriter = new CommandHelpWriter(
                    new HelpWriterOptions(includeValues: includeValues, filter: filter),
                    OptionValueProviders.ProvidersByName);

                command = command.WithOptions(command.Options.Sort(CompareOptions));

                CommandHelp commandHelp = CommandHelp.Create(command, OptionValueProviders.ProvidersByName.Select(f => f.Value), filter: filter);

                helpWriter.WriteCommand(commandHelp);
            }
            else
            {
                IEnumerable<Command> commands = LoadCommands();

                CommandsHelp commandsHelp = CommandsHelp.Create(commands, OptionValueProviders.ProvidersByName.Select(f => f.Value), filter: filter);

                var helpWriter = new CommandHelpWriter(
                    new HelpWriterOptions(includeValues: includeValues, filter: filter),
                    OptionValueProviders.ProvidersByName);

                helpWriter.WriteCommands(commandsHelp);
            }
        }

        private static void WriteManual(bool includeValues = false, Filter filter = null)
        {
            IEnumerable<Command> commands = LoadCommands();

            var helpWriter = new ManualHelpWriter(
                new HelpWriterOptions(includeValues: false, filter: filter),
                OptionValueProviders.ProvidersByName);

            IEnumerable<CommandHelp> commandHelps = commands.Select(f => CommandHelp.Create(f, filter: filter))
                .Where(f => f.Arguments.Any() || f.Options.Any())
                .ToImmutableArray();

            ImmutableArray<CommandShortHelp> commandShortHelps = HelpProvider.GetCommandShortHelp(commandHelps.Select(f => f.Command));

            ImmutableArray<OptionValuesHelp> values = HelpProvider.GetOptionValuesHelp(commandHelps.SelectMany(f => f.Command.Options), OptionValueProviders.ProvidersByName.Select(f => f.Value), filter);

            ImmutableArray<string> expressions = HelpProvider.GetExpressionsLines(values);

            var commandsHelp = new CommandsHelp(commandShortHelps, values, expressions);

            helpWriter.WriteCommands(commandsHelp);

            foreach (CommandHelp commandHelp in commandHelps)
            {
                WriteSeparator();
                WriteLine();
                WriteLine($"Command: {commandHelp.Name}");
                WriteLine();

                string description = commandHelp.Description;

                if (!string.IsNullOrEmpty(description))
                {
                    WriteLine(description);
                    WriteLine();
                }

                helpWriter.WriteCommand(commandHelp);
            }

            if (includeValues)
            {
                WriteSeparator();

                helpWriter.WriteValues(commandsHelp.Values, commandsHelp.Expressions);
            }

            static void WriteSeparator()
            {
                WriteLine();
                WriteLine("----------");
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

        internal static string GetHeadingText()
        {
            return $"Orang Command-Line Tool version {typeof(Program).GetTypeInfo().Assembly.GetName().Version}";
        }

        internal static string GetFooterText(string command = null)
        {
            return $"Run 'orang help {command ?? "[command]"}' for more information on a command."
                + Environment.NewLine
                + $"Run 'orang help {command ?? "[command]"} -v d' for more information on allowed values.";
        }

        private class ManualHelpWriter : HelpWriter
        {
            private ContentWriterOptions _contentWriterOptions;

            public ManualHelpWriter(
                HelpWriterOptions options = null,
                ImmutableDictionary<string, OptionValueProvider> optionValueProviders = null) : base(options, optionValueProviders?.Select(f => f.Value))
            {
            }

            public Filter Filter => Options.Filter;

            internal ContentWriterOptions ContentWriterOptions
            {
                get
                {
                    if (_contentWriterOptions == null
                        && Filter != null)
                    {
                        _contentWriterOptions = new ContentWriterOptions(
                            format: new OutputDisplayFormat(ContentDisplayStyle.AllLines),
                            (Filter.GroupNumber >= 0) ? new GroupDefinition(Filter.GroupNumber, Filter.GroupName) : default,
                            indent: "");
                    }

                    return _contentWriterOptions;
                }
            }

            public override void WriteCommands(CommandsHelp commands)
            {
                WriteLine(GetHeadingText());
                WriteLine("Usage: orang [command] [arguments]");
                WriteLine();

                base.WriteCommands(commands);
            }

            public override void WriteStartCommand(CommandHelp command)
            {
                Write("Usage: orang ");
                Write(command.Name);

                foreach (ArgumentHelp argument in command.Arguments)
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

            protected override void Write(char value)
            {
                ConsoleOut.Write(value);
            }

            protected override void Write(string value)
            {
                ConsoleOut.Write(value);
            }

            protected override void WriteLine()
            {
                ConsoleOut.WriteLine();
            }

            protected override void WriteLine(string value)
            {
                ConsoleOut.WriteLine(value);
            }

            protected override void WriteTextLine(string value)
            {
                if (Filter != null)
                {
                    Match match = Filter.Match(value);

                    if (match != null)
                    {
                        List<Capture> captures = ListCache<Capture>.GetInstance();

                        CaptureFactory.GetCaptures(ref captures, match, Filter.GroupNumber);

                        var writer = new AllLinesContentWriter(value, ContentTextWriter.Default, ContentWriterOptions);

                        writer.WriteMatches(captures);

                        ListCache<Capture>.Free(captures);
                        return;
                    }
                }

                ConsoleOut.WriteLine(value);
            }
        }

        private class CommandHelpWriter : ManualHelpWriter
        {
            public CommandHelpWriter(
                HelpWriterOptions options = null,
                ImmutableDictionary<string, OptionValueProvider> optionValueProviders = null) : base(options, optionValueProviders)
            {
            }

            public override void WriteEndOptions(CommandHelp command)
            {
                if (Options.IncludeValues)
                {
                    WriteValues(command.Values, command.Expressions);
                }
                else
                {
                    IEnumerable<string> metaValues = CommandLine.OptionValueProviders.GetProviders(command.Options.Select(f => f.Option), OptionValueProviders).Select(f => f.Name);

                    if (metaValues.Any())
                    {
                        WriteLine();
                        Write($"Run 'orang help {command.Name} -v d' to display list of allowed values for ");
                        Write(TextHelpers.Join(", ", " and ", metaValues));
                        WriteLine(".");
                    }
                }
            }

            public override void WriteEndCommands(CommandsHelp commands)
            {
                if (Options.IncludeValues)
                    WriteValues(commands.Values, commands.Expressions);

                WriteLine();
                WriteLine(GetFooterText());
            }
        }
    }
}
