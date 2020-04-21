// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine.Help
{
    public class CommandItem
    {
        public CommandItem(Command command, string text)
        {
            Command = command;
            Text = text;
        }

        public Command Command { get; }

        public string Text { get; }

        public override string ToString() => Text;
    }
}
