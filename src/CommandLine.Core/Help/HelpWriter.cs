// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Orang
{
    public abstract class HelpWriter
    {
        protected HelpWriter(HelpWriterOptions options = null)
        {
            Options = options ?? HelpWriterOptions.Default;
        }

        public HelpWriterOptions Options { get; }

        public virtual void WriteCommand(CommandHelp commandHelp)
        {
            WriteStartCommand(commandHelp);

            ImmutableArray<ArgumentHelp> arguments = commandHelp.Arguments;

            if (arguments.Any())
            {
                WriteStartArguments(commandHelp);
                WriteArguments(arguments);
                WriteEndArguments(commandHelp);
            }
            else if (Options.Filter != null
                && commandHelp.Command.Arguments.Any())
            {
                WriteLine();
                WriteLine("No argument found");
            }

            ImmutableArray<OptionHelp> options = commandHelp.Options;

            if (options.Any())
            {
                WriteStartOptions(commandHelp);
                WriteOptions(options);
                WriteEndOptions(commandHelp);
            }
            else if (Options.Filter != null
                && commandHelp.Command.Options.Any())
            {
                WriteLine();
                WriteLine("No option found");
            }

            WriteEndCommand(commandHelp);
        }

        public virtual void WriteStartCommand(CommandHelp commandHelp)
        {
        }

        public virtual void WriteEndCommand(CommandHelp commandHelp)
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

        public virtual void WriteStartOptions(CommandHelp commandHelp)
        {
            WriteLine();
            WriteHeading("Options");
        }

        public virtual void WriteEndOptions(CommandHelp commandHelp)
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

        public virtual void WriteStartArguments(CommandHelp commandHelp)
        {
            WriteLine();
            WriteHeading("Arguments");
        }

        public virtual void WriteEndArguments(CommandHelp commandHelp)
        {
        }

        public virtual void WriteCommands(CommandsHelp commandsHelp)
        {
            if (commandsHelp.Commands.Any())
            {
                WriteStartCommands(commandsHelp);

                int width = commandsHelp.Commands.Max(f => f.Command.Name.Length) + 1;

                foreach (CommandShortHelp command in commandsHelp.Commands)
                {
                    Write(Options.Indent);
                    WriteTextLine(command.Text);
                }

                WriteEndCommands(commandsHelp);
            }
            else if (Options.Filter != null)
            {
                WriteLine("No command found");
            }
        }

        public virtual void WriteStartCommands(CommandsHelp commandsHelp)
        {
            WriteHeading("Commands");
        }

        public virtual void WriteEndCommands(CommandsHelp commandsHelp)
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

        private void WriteValues(ImmutableArray<OptionValueHelp> values)
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
