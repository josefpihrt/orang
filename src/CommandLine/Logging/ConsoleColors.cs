// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Orang
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal readonly struct ConsoleColors
    {
        public ConsoleColors(ConsoleColor? foreground)
            : this(foreground, null)
        {
        }

        public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
        {
            Foreground = foreground;
            Background = background;
        }

        public ConsoleColor? Foreground { get; }

        public ConsoleColor? Background { get; }

        public bool IsDefault => Foreground == null && Background == null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{((Foreground != null) ? Foreground.ToString() : "None")}  {((Background != null) ? Background.ToString() : "None")}";
    }
}
