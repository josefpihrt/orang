// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine
{
    internal readonly struct CommandResult
    {
        public static CommandResult Success { get; } = new CommandResult(CommandResultKind.Success);

        public static CommandResult NoMatch { get; } = new CommandResult(CommandResultKind.NoMatch);

        public static CommandResult Fail { get; } = new CommandResult(CommandResultKind.Fail);

        public static CommandResult Canceled { get; } = new CommandResult(CommandResultKind.Canceled);

        public CommandResult(CommandResultKind kind)
        {
            Kind = kind;
        }

        public CommandResultKind Kind { get; }
    }
}
