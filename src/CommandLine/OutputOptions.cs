// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Text;

namespace Orang
{
    internal class OutputOptions
    {
        public OutputOptions(
            string path,
            Encoding encoding = null,
            bool includeContent = false,
            bool includePath = false)
        {
            Debug.Assert(includeContent || includePath);

            Path = path;
            Encoding = encoding ?? Encoding.UTF8;
            IncludeContent = includeContent;
            IncludePath = includePath;
        }

        public string Path { get; }

        public Encoding Encoding { get; }

        public bool IncludeContent { get; }

        public bool IncludePath { get; }
    }
}
