// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.Spelling
{
    public class SpellcheckerOptions
    {
        public static SpellcheckerOptions Default { get; } = new(SplitMode.CaseAndHyphen);

        public SpellcheckerOptions(
            SplitMode splitMode = SplitMode.CaseAndHyphen,
            int minWordLength = 3,
            int maxWordLength = int.MaxValue)
        {
            SplitMode = splitMode;
            MinWordLength = minWordLength;
            MaxWordLength = maxWordLength;
        }

        public SplitMode SplitMode { get; }

        public int MinWordLength { get; }

        public int MaxWordLength { get; }
    }
}
