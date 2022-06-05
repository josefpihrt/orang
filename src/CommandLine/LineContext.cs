// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine
{
    internal readonly struct LineContext
    {
        public LineContext(int count)
            : this(count, count)
        {
        }

        public LineContext(int before, int after)
        {
            Before = before;
            After = after;
        }

        public int Before { get; }

        public int After { get; }

        public LineContext WithBefore(int before) => new(before, After);

        public LineContext WithAfter(int after) => new(Before, after);
    }
}
