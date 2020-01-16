// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.CommandLine
{
    [Flags]
    internal enum ModifyFlags
    {
        None = 0,
        Distinct = 1,
        Ascending = 1 << 1,
        Descending = 1 << 2,
        Except = 1 << 3,
        Intersect = 1 << 4,
        ExceptIntersect = Except | Intersect,
        RemoveEmpty = 1 << 5,
        RemoveWhiteSpace = 1 << 6,
        TrimStart = 1 << 7,
        TrimEnd = 1 << 8,
        Trim = TrimStart | TrimEnd,
        ToUpper = 1 << 9,
        ToLower = 1 << 10,
        Aggregate = 1 << 11,
        IgnoreCase = 1 << 12,
        CultureInvariant = 1 << 13,
        AggregateOnly = 1 << 14
    }
}
