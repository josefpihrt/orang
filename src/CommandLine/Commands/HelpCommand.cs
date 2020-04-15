// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
                WriteHelpText(ConsoleOut);

                if (Out != null)
                    WriteHelpText(Out);
            }
            catch (ArgumentException ex)
            {
                WriteError(ex);
                return CommandResult.Fail;
            }

            return CommandResult.Success;

            void WriteHelpText(TextWriterWithVerbosity writer)
            {
                string text = GetHelpText(commandName: Options.Command, manual: Options.Manual, includeValues: writer.Verbosity > Verbosity.Normal);

                writer.Write(text);
            }
        }

        private static string GetHelpText(string commandName = null, bool manual = false, bool includeValues = false)
        {
            if (commandName != null)
            {
                Command command = CommandLoader.LoadCommand(typeof(HelpCommand).Assembly, commandName);

                if (command == null)
                    throw new ArgumentException($"Command '{commandName}' does not exist.", nameof(commandName));

                return HelpProvider.GetHelpText(command, includeValues);
            }
            else if (manual)
            {
                return HelpProvider.GetManual(includeValues: includeValues);
            }
            else
            {
                return HelpProvider.GetHelpText(includeValues: includeValues);
            }
        }
    }
}
