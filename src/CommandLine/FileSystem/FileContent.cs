// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

namespace Orang.FileSystem
{
    public readonly struct FileContent
    {
        public FileContent(string text, Encoding bomEncoding, Encoding currentEncoding)
        {
            Text = text;
            BomEncoding = bomEncoding;
            CurrentEncoding = currentEncoding;
        }

        public string Text { get; }

        public Encoding BomEncoding { get; }

        public Encoding CurrentEncoding { get; }

        public override string ToString() => Text;
    }
}
