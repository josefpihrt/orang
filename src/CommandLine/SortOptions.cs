// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;

namespace Orang.CommandLine
{
    internal class SortOptions
    {
        public SortOptions(ImmutableArray<SortDescriptor> descriptors, int maxCount = 0)
        {
            Descriptors = descriptors;
            MaxCount = maxCount;
        }

        public ImmutableArray<SortDescriptor> Descriptors { get; }

        public int MaxCount { get; }
    }
}
