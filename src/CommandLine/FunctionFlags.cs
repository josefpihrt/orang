// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.CommandLine;

[Flags]
internal enum FunctionFlags
{
    None = 0,
    Distinct = 1,
    Sort = 1 << 1,
    SortDescending = 1 << 2,
    Group = 1 << 5,
}
