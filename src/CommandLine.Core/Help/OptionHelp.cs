// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang
{
    public class OptionHelp
    {
        public OptionHelp(CommandOption option, string text)
        {
            Option = option;
            Text = text;
        }

        public CommandOption Option { get; }

        public string Text { get; }

        public override string ToString() => Text;
    }
}
