// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.CommandLine
{
    [Flags]
    internal enum ReplaceFlags
    {
        None = 0,
        TrimStart = 1,
        TrimEnd = 1 << 1,
        Trim = TrimStart | TrimEnd,
        ToUpper = 1 << 2,
        ToLower = 1 << 3,
        CultureInvariant = 1 << 4,
    }
}
