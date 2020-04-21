// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine.Help
{
    public class OptionValueItem
    {
        public OptionValueItem(OptionValue value, string text)
        {
            Value = value;
            Text = text;
        }

        public OptionValue Value { get; }

        public string Text { get; }

        public override string ToString() => Text;
    }
}
