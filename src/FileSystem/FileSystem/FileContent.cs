// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

namespace Orang.FileSystem
{
    public readonly struct FileContent
    {
        public FileContent(string text, Encoding encoding, bool hasBom)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            HasBom = hasBom;
        }

        public string Text { get; }

        public Encoding Encoding { get; }

        public bool HasBom { get; }

        public override string ToString() => Text;
    }
}
