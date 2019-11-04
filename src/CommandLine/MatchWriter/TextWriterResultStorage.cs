// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Orang.CommandLine
{
    internal class TextWriterResultStorage : IResultStorage
    {
        private readonly TextWriter _writer;

        public TextWriterResultStorage(TextWriter writer)
        {
            _writer = writer;
        }

        public void Add(string value)
        {
            _writer.WriteLine(value);
        }

        public void Add(string value, int start, int length)
        {
            _writer.WriteLine(value.AsSpan(start, length));
        }
    }
}
