// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;

namespace Orang.CommandLine.Help;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class OptionValueItemList
{
    public OptionValueItemList(string metaValue, ImmutableArray<OptionValueItem> values)
    {
        MetaValue = metaValue;
        Values = values;
    }

    public string MetaValue { get; }

    public ImmutableArray<OptionValueItem> Values { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{MetaValue}  Count = {Values.Length}";
}
