// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang
{
    [Flags]
    internal enum HighlightOptions
    {
        None = 0,
        Match = 1,
        Replacement = 1 << 1,
        Split = 1 << 2,
        EmptyMatch = 1 << 3,
        EmptyReplacement = 1 << 4,
        EmptySplit = 1 << 5,
        Empty = EmptyMatch | EmptyReplacement | EmptySplit,
        Boundary = 1 << 6,
        Tab = 1 << 7,
        CarriageReturn = 1 << 8,
        Linefeed = 1 << 9,
        NewLine = CarriageReturn | Linefeed,
        Space = 1 << 10,
        Character = Tab | CarriageReturn | Linefeed | Space
    }
}
