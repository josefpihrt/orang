// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

namespace Orang.CommandLine
{
    internal sealed class OutputOptions
    {
        public OutputOptions(string path, Encoding? encoding, bool append, bool delay)
        {
            Path = path;
            Encoding = encoding ?? Encoding.UTF8;
            Append = append;
            Delay = delay;
        }

        public string Path { get; }

        public Encoding Encoding { get; }

        public bool Append { get; }

        public bool Delay { get; }
    }
}
