// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Orang.Spelling;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct WordSequence
{
    public WordSequence(IEnumerable<string> words)
    {
        if (words is null)
            throw new ArgumentNullException(nameof(words));

        Words = words.ToImmutableArray();
    }

    public string First => Words[0];

    public int Count => Words.Length;

    public ImmutableArray<string> Words { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => (Words.IsDefault) ? "Uninitialized" : $"Count = {Count}  {ToString()}";

    public override string ToString()
    {
        return (Words.IsDefault) ? "" : string.Join(" ", Words);
    }
}
