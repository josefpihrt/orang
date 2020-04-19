// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Orang
{
    public class CommandsHelp
    {
        public CommandsHelp(
            ImmutableArray<CommandShortHelp> commands,
            ImmutableArray<OptionValuesHelp> values,
            ImmutableArray<string> expressions)
        {
            Commands = commands;
            Values = values;
            Expressions = expressions;
        }

        public ImmutableArray<CommandShortHelp> Commands { get; }

        public ImmutableArray<OptionValuesHelp> Values { get; }

        public ImmutableArray<string> Expressions { get; }

        public static CommandsHelp Create(
            IEnumerable<Command> commands,
            IEnumerable<OptionValueProvider> providers = null,
            Filter filter = null)
        {
            ImmutableArray<CommandShortHelp> commandsHelp = HelpProvider.GetCommandShortHelp(commands, filter);

            ImmutableArray<OptionValuesHelp> values = HelpProvider.GetOptionValuesHelp(commands.SelectMany(f => f.Options), providers ?? ImmutableArray<OptionValueProvider>.Empty, filter);

            ImmutableArray<string> expressions = HelpProvider.GetExpressionsLines(values);

            return new CommandsHelp(commandsHelp, values, expressions);
        }
    }
}
