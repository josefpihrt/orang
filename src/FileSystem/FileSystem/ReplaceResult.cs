// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Orang.Text.RegularExpressions;

namespace Orang.FileSystem;

internal class ReplaceResult
{
    public ReplaceResult(List<ReplaceItem> items, MaxReason maxReason, string newName)
    {
        Items = items;
        MaxReason = maxReason;
        NewName = newName;
    }

    public List<ReplaceItem> Items { get; }

    public MaxReason MaxReason { get; }

    public string NewName { get; }

    public int Count => Items.Count;
}
