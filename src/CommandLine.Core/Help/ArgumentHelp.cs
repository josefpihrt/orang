// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang
{
    public class ArgumentHelp
    {
        public ArgumentHelp(CommandArgument argument, string text)
        {
            Argument = argument;
            Text = text;
        }

        public CommandArgument Argument { get; }

        public string Text { get; }

        public bool IsRequired => Argument.IsRequired;

        public string Name => Argument.Name;

        public override string ToString() => Text;
    }
}
