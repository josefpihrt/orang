// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine.Help;

public class OptionItem : HelpItem
{
    public OptionItem(CommandOption option, string syntax, string description) : base(syntax, description)
    {
        Option = option;
    }

    public CommandOption Option { get; }
}
