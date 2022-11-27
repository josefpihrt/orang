// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang;

[Flags]
public enum PatternCreationOptions
{
    None = 0,
    EndsWith = 1,
    List = 1 << 1,
    Literal = 1 << 2,
    StartsWith = 1 << 3,
    WholeLine = 1 << 4,
    WholeWord = 1 << 5,
    Equals = StartsWith | EndsWith,
}
