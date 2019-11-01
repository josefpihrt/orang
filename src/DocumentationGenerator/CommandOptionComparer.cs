// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Orang.CommandLine;

namespace Orang.Documentation
{
    internal class CommandOptionComparer : IComparer<CommandOption>
    {
        public static CommandOptionComparer Instance { get; } = new CommandOptionComparer();

        public int Compare(CommandOption x, CommandOption y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            if (IsFileLogOption(x.Name))
            {
                if (IsFileLogOption(y.Name))
                {
                    return StringComparer.InvariantCulture.Compare(x.Name, y.Name);
                }
                else
                {
                    return 1;
                }
            }
            else if (IsFileLogOption(y.Name))
            {
                return -1;
            }
            else
            {
                return StringComparer.InvariantCulture.Compare(x.Name, y.Name);
            }

            bool IsFileLogOption(string name)
            {
                switch (name)
                {
                    case OptionNames.FileLog:
                        return true;
                }

                return false;
            }
        }
    }
}
