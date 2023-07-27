// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang;

[Flags]
public enum PatternOptions
{
    None = 0,
    Literal = 1,
    StartsWith = 1 << 1,
    EndsWith = 1 << 2,
    WholeWord = 1 << 3,
    WholeLine = 1 << 4,
    Equals = StartsWith | EndsWith,
}
