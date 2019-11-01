// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Orang.CommandLine
{
    internal class HelpCommand : AbstractCommand
    {
        private static readonly Regex _metaValueRegex = new Regex(@"\<\w+\>");

        public HelpCommand(HelpCommandOptions options)
        {
            Options = options;
        }

        public HelpCommandOptions Options { get; }

        protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            using (var stringWriter = new StringWriter())
            {
                var helpWriter = new HelpCommandHelpWriter(
                    stringWriter,
                    new HelpWriterOptions(includeValues: Options.IncludeValues),
                    OptionValueProviders.ProvidersByName);

                if (Options.Command != null)
                {
                    Command command = CommandLoader.LoadCommand(typeof(HelpCommand).Assembly, Options.Command);

                    if (command == null)
                    {
                        Logger.WriteLine($"Command '{Options.Command}' does not exist.");
                        return CommandResult.Fail;
                    }

                    command = command.WithOptions(command.Options.Sort(CompareOptions));

                    helpWriter.WriteCommand(command);
                }
                else
                {
                    IEnumerable<Command> commands = CommandLoader.LoadCommands(typeof(HelpCommand).Assembly)
                        .OrderBy(f => f.Name, StringComparer.CurrentCulture);

                    helpWriter.WriteCommands(commands);
                }

                Logger.Write(stringWriter.ToString());
            }

            return CommandResult.Success;
        }

        private static int CompareOptions(CommandOption x, CommandOption y)
        {
            if (IsFileLogOption(x.Name))
            {
                if (IsFileLogOption(y.Name))
                {
                    return StringComparer.InvariantCulture.Compare(x.Name, y.Name);
                }
                else
                {
                    return 1;
                }
            }
            else if (IsFileLogOption(y.Name))
            {
                return -1;
            }
            else
            {
                return StringComparer.InvariantCulture.Compare(x.Name, y.Name);
            }

            bool IsFileLogOption(string name)
            {
                switch (name)
                {
                    case OptionNames.FileLog:
                        return true;
                }

                return false;
            }
        }

        private class HelpCommandHelpWriter : HelpWriter
        {
            private readonly ImmutableDictionary<string, OptionValueProvider> _providersByName;

            public HelpCommandHelpWriter(
                TextWriter writer,
                HelpWriterOptions options = null,
                ImmutableDictionary<string, OptionValueProvider> optionValueProviders = null) : base(writer, options, optionValueProviders.Select(f => f.Value))
            {
                _providersByName = optionValueProviders;
            }

            public override void WriteEndOptions(Command command)
            {
                if (Options.IncludeValues)
                    return;

                IEnumerable<string> metaValues = GetProviders(command.Options).Select(f => f.Name);

                if (!metaValues.Any())
                    return;

                WriteLine();
                Write($"Run 'orang help {command.Name} -{OptionShortNames.Values}' to display list of allowed values for ");
                Write(TextHelpers.Join(", ", " and ", metaValues));
                WriteLine(".");
            }

            public override void WriteCommands(IEnumerable<Command> commands)
            {
                WriteLine($"Orang Command-Line Tool version {typeof(HelpCommand).GetTypeInfo().Assembly.GetName().Version}");
                WriteLine("Usage: orang [command] [arguments]");
                WriteLine();

                base.WriteCommands(commands);
            }

            public override void WriteEndCommands()
            {
                WriteLine();
                WriteLine("Run 'orang help [command]' for more information on a command.");
            }

            protected override void WriteValues(IEnumerable<CommandOption> options)
            {
                ImmutableArray<OptionValueProvider> providers = GetProviders(options).ToImmutableArray();

                providers = providers
                    .SelectMany(f => f.Values)
                    .OfType<KeyValuePairOptionValue>()
                    .Select(f => f.Value)
                    .Distinct()
                    .Join(OptionValueProviders, f => f, f => f.Name, (_, f) => f)
                    .OrderBy(f => f.Name)
                    .Concat(providers)
                    .Distinct()
                    .ToImmutableArray();

                WriteValues(providers);
            }

            private IEnumerable<OptionValueProvider> GetProviders(IEnumerable<CommandOption> options)
            {
                IEnumerable<string> metaValues = options
                    .SelectMany(f2 => _metaValueRegex.Matches(f2.Description).Select(m => m.Value))
                    .Concat(options.Select(f3 => f3.MetaValue))
                    .Distinct();

                return metaValues
                    .Join(OptionValueProviders, f => f, f => f.Name, (_, f) => f)
                    .OrderBy(f => f.Name);
            }
        }
    }
}
