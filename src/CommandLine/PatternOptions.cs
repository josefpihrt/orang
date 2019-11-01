// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.CommandLine
{
    [Flags]
    internal enum PatternOptions
    {
        None = 0,
        Compiled = 1,
        CultureInvariant = 1 << 1,
        ECMAScript = 1 << 2,
        ExplicitCapture = 1 << 3,
        FromFile = 1 << 4,
        IgnoreCase = 1 << 5,
        IgnorePatternWhitespace = 1 << 6,
        List = 1 << 7,
        Literal = 1 << 8,
        Multiline = 1 << 9,
        Negative = 1 << 10,
        RightToLeft = 1 << 11,
        Singleline = 1 << 12,
        WholeInput = 1 << 13,
        WholeLine = 1 << 14,
        WholeWord = 1 << 15,
    }
}
