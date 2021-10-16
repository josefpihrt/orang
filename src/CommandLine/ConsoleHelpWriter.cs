// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orang.CommandLine.Help;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine
{
    internal class ConsoleHelpWriter : HelpWriter
    {
        private ContentWriterOptions? _contentWriterOptions;
        private readonly Logger _logger;

        public ConsoleHelpWriter(Logger logger, HelpWriterOptions? options = null) : base(options)
        {
            _logger = logger;
            ContentWriter = new ContentTextWriter(_logger);
        }


        public ContentTextWriter ContentWriter { get; }

        public Filter? Filter => Options.Filter;

        internal ContentWriterOptions? ContentWriterOptions
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
            WriteLine(HelpCommand.GetHeadingText());
            WriteLine("Usage: orang [command] [arguments]");
            WriteLine();

            if (commands.Commands.Any())
            {
                using (IEnumerator<IGrouping<string, CommandItem>>? en = commands.Commands
                    .OrderBy(f => f.Command.Group.Ordinal)
                    .GroupBy(f => f.Command.Group.Name)
                    .GetEnumerator())
                {
                    if (en.MoveNext())
                    {
                        while (true)
                        {
                            WriteHeading((string.IsNullOrEmpty(en.Current.Key)) ? "Commands" : $"{en.Current.Key} commands");

                            int width = commands.Commands.Max(f => f.Command.DisplayName.Length) + 1;

                            foreach (CommandItem command in en.Current)
                            {
                                Write(Options.Indent);
                                WriteTextLine(command);
                            }

                            if (en.MoveNext())
                            {
                                WriteLine();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                WriteEndCommands(commands);
            }
            else if (Options.Filter != null)
            {
                WriteLine("No command found");
            }
        }

        public override void WriteStartCommand(CommandHelp commandHelp)
        {
            Write("Usage: orang ");
            Write(commandHelp.DisplayName);

            Command command = commandHelp.Command;

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

        protected override void Write(char value)
        {
            _logger.ConsoleOut.Write(value);
        }

        protected override void Write(string value)
        {
            _logger.ConsoleOut.Write(value);
        }

        protected override void WriteLine()
        {
            _logger.ConsoleOut.WriteLine();
        }

        protected override void WriteLine(string value)
        {
            _logger.ConsoleOut.WriteLine(value);
        }

        protected override void WriteTextLine(HelpItem helpItem)
        {
            string value = helpItem.Text;

            if (Filter != null)
            {
                Match? match = Filter.Match(value);

                if (match != null)
                {
                    List<Capture> captures = ListCache<Capture>.GetInstance();

                    CaptureFactory.GetCaptures(ref captures, match, Filter.GroupNumber);

                    var writer = new AllLinesContentWriter(value, ContentWriter, ContentWriterOptions!);

                    writer.WriteMatches(captures);

                    ListCache<Capture>.Free(captures);
                    return;
                }
            }

            _logger.ConsoleOut.WriteLine(value);
        }
    }
}
