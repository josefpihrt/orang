// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Orang.Syntax
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SyntaxItem
    {
        public SyntaxItem(string text, SyntaxSection section, string description)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Section = section;
            Description = description ?? "";
        }

        public string Text { get; }

        public SyntaxSection Section { get; }

        public string Description { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Section} {Text}";
    }
}
