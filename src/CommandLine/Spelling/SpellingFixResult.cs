// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Orang.Spelling;

namespace Orang.CommandLine
{
    internal readonly struct SpellingFixResult
    {
        public SpellingFixResult(
            string oldValue,
            string newValue,
            SpellingFixKind kind)
        {
            OldValue = oldValue;
            NewValue = newValue;
            Kind = kind;
        }

        public string OldValue { get; }

        public string NewValue { get; }

        public SpellingFixKind Kind { get; }
    }
}
