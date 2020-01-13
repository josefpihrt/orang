// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.CommandLine
{
    [Flags]
    internal enum PatternOptions
    {
        None = 0,
        CaseSensitive = 1,
        Compiled = 1 << 1,
        CultureInvariant = 1 << 2,
        ECMAScript = 1 << 3,
        EndsWith = 1 << 4,
        ExplicitCapture = 1 << 5,
        FromFile = 1 << 6,
        IgnoreCase = 1 << 7,
        IgnorePatternWhitespace = 1 << 8,
        List = 1 << 9,
        Literal = 1 << 10,
        Multiline = 1 << 11,
        Negative = 1 << 12,
        RightToLeft = 1 << 13,
        Singleline = 1 << 14,
        StartsWith = 1 << 15,
        WholeLine = 1 << 16,
        WholeWord = 1 << 17,
        Equals = StartsWith | EndsWith,
    }
}
