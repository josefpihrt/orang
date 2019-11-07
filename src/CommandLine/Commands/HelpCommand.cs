// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;

namespace Orang.CommandLine
{
    internal class HelpCommand : AbstractCommand
    {
        public HelpCommand(HelpCommandOptions options)
        {
            Options = options;
        }

        public HelpCommandOptions Options { get; }

        protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            try
            {
                string text = GetHelpText(commandName: Options.Command, includeValues: Options.IncludeValues);

                Logger.Write(text);
            }
            catch (ArgumentException ex)
            {
                Logger.WriteError(ex);
                return CommandResult.Fail;
            }

            return CommandResult.Success;
        }

        private static string GetHelpText(string commandName = null, bool includeValues = false)
        {
            if (commandName != null)
            {
                Command command = CommandLoader.LoadCommand(typeof(HelpCommand).Assembly, commandName);

                if (command == null)
                    throw new ArgumentException($"Command '{commandName}' does not exist.", nameof(commandName));

                return HelpProvider.GetHelpText(command, includeValues);
            }
            else
            {
                return HelpProvider.GetHelpText(includeValues);
            }
        }
    }
}
