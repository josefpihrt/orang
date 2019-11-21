// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Orang.CommandLine
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal readonly struct SortDescriptor
    {
        public SortDescriptor(SortProperty property, SortDirection direction = SortDirection.Ascending)
        {
            Property = property;
            Direction = direction;
        }

        public SortProperty Property { get; }

        public SortDirection Direction { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Property} {Direction}";
    }
}
