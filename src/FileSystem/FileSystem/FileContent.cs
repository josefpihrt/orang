// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;

namespace Orang.FileSystem;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct FileContent
{
    internal FileContent(string text, Encoding encoding, bool hasBom)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        HasBom = hasBom;
    }

    public string Text { get; }

    public Encoding Encoding { get; }

    public bool HasBom { get; }

    public override string ToString() => Text;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return (Text is not null)
                ? $"{Encoding.EncodingName}  {nameof(HasBom)} = {HasBom}"
                : "Uninitialized";
        }
    }
}
