// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Orang
{
    public abstract class HelpWriter
    {
        protected HelpWriter(HelpWriterOptions options = null, IEnumerable<OptionValueProvider> optionValueProviders = default)
        {
            Options = options ?? HelpWriterOptions.Default;
            OptionValueProviders = optionValueProviders?.ToImmutableArray() ?? ImmutableArray<OptionValueProvider>.Empty;
        }

        public HelpWriterOptions Options { get; }

        public ImmutableArray<OptionValueProvider> OptionValueProviders { get; }

        public virtual void WriteCommand(CommandHelp command)
        {
            WriteStartCommand(command);

            ImmutableArray<ArgumentHelp> arguments = command.Arguments;

            if (arguments.Any())
            {
                WriteStartArguments(command);
                WriteArguments(arguments);
                WriteEndArguments(command);
            }

            ImmutableArray<OptionHelp> options = command.Options;

            if (options.Any())
            {
                WriteStartOptions(command);
                WriteOptions(options);
                WriteEndOptions(command);
            }

            WriteEndCommand(command);
        }

        public virtual void WriteStartCommand(CommandHelp command)
        {
        }

        public virtual void WriteEndCommand(CommandHelp command)
        {
        }

        private void WriteOptions(ImmutableArray<OptionHelp> options)
        {
            foreach (OptionHelp option in options)
            {
                Write(Options.Indent);
                WriteTextLine(option.Text);
            }
        }

        public virtual void WriteStartOptions(CommandHelp command)
        {
            WriteLine();
            WriteHeading("Options");
        }

        public virtual void WriteEndOptions(CommandHelp command)
        {
        }

        private void WriteArguments(ImmutableArray<ArgumentHelp> arguments)
        {
            foreach (ArgumentHelp argument in arguments)
            {
                Write(Options.Indent);
                WriteTextLine(argument.Text);
            }
        }

        public virtual void WriteStartArguments(CommandHelp command)
        {
            WriteLine();
            WriteHeading("Arguments");
        }

        public virtual void WriteEndArguments(CommandHelp command)
        {
        }

        public virtual void WriteCommands(CommandsHelp commands)
        {
            WriteStartCommands(commands);

            if (commands.Commands.Any())
            {
                int width = commands.Commands.Max(f => f.Command.Name.Length) + 1;

                foreach (CommandShortHelp command in commands.Commands)
                {
                    Write(Options.Indent);
                    WriteTextLine(command.Text);
                }
            }
            else
            {
                Write(Options.Indent);
                WriteLine("No command found");
            }

            WriteEndCommands(commands);
        }

        public virtual void WriteStartCommands(CommandsHelp commands)
        {
            WriteHeading("Commands");
        }

        public virtual void WriteEndCommands(CommandsHelp commands)
        {
        }

        public void WriteValues(IEnumerable<OptionValuesHelp> optionValues, IEnumerable<string> expressions = null)
        {
            using (IEnumerator<OptionValuesHelp> en = optionValues.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    WriteStartValues();

                    while (true)
                    {
                        WriteTextLine(en.Current.MetaValue);
                        WriteValues(en.Current.Values);

                        if (en.MoveNext())
                        {
                            WriteLine();
                        }
                        else
                        {
                            break;
                        }
                    }

                    WriteEndValues();

                    if (expressions?.Any() == true)
                    {
                        WriteLine();
                        WriteLine("Expression syntax:");

                        foreach (string expression in expressions)
                        {
                            Write(Options.Indent);
                            WriteLine(expression);
                        }
                    }
                }
            }
        }

        public void WriteValues(ImmutableArray<OptionValueHelp> values)
        {
            foreach (OptionValueHelp value in values)
            {
                Write(Options.Indent);
                WriteTextLine(value.Text);
            }
        }

        public virtual void WriteStartValues()
        {
            WriteLine();
            WriteHeading("Values");
        }

        public virtual void WriteEndValues()
        {
        }

        protected void WriteHeading(string value)
        {
            Write(value);
            WriteLine(":");
        }

        protected void WriteSpaces(int count)
        {
            for (int i = 0; i < count; i++)
                Write(' ');
        }

        protected abstract void Write(char value);

        protected abstract void Write(string value);

        protected abstract void WriteLine();

        protected abstract void WriteLine(string value);

        protected abstract void WriteTextLine(string value);
    }
}
