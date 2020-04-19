// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Orang
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CommandHelp
    {
        public CommandHelp(
            Command command,
            ImmutableArray<ArgumentHelp> arguments,
            ImmutableArray<OptionHelp> options,
            ImmutableArray<OptionValuesHelp> values,
            ImmutableArray<string> expressions)
        {
            Command = command;
            Arguments = arguments;
            Options = options;
            Values = values;
            Expressions = expressions;
        }

        public Command Command { get; }

        public string Name => Command.Name;

        public string Description => Command.Description;

        public ImmutableArray<ArgumentHelp> Arguments { get; }

        public ImmutableArray<OptionHelp> Options { get; }

        public ImmutableArray<OptionValuesHelp> Values { get; }

        public ImmutableArray<string> Expressions { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Name}  {Description}";

        public static CommandHelp Create(Command command, IEnumerable<OptionValueProvider> providers = null, Filter filter = null)
        {
            ImmutableArray<ArgumentHelp> arguments = (command.Arguments.Any())
                ? HelpProvider.GetArgumentsHelp(command.Arguments, filter)
                : ImmutableArray<ArgumentHelp>.Empty;

            ImmutableArray<OptionHelp> options = HelpProvider.GetOptionsHelp(command.Options, filter);

            ImmutableArray<OptionValuesHelp> values = HelpProvider.GetOptionValuesHelp(command.Options, providers ?? ImmutableArray<OptionValueProvider>.Empty, filter);

            ImmutableArray<string> expressions = HelpProvider.GetExpressionsLines(values);

            return new CommandHelp(command, arguments, options, values, expressions);
        }
    }
}
