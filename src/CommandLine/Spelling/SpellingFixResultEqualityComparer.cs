// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Orang.CommandLine
{
    internal abstract class SpellingFixResultEqualityComparer : IEqualityComparer<SpellingFixResult>
    {
        public static SpellingFixResultEqualityComparer OldValueAndNewValue { get; } = new ValueAndFixedValueComparer();

        public abstract bool Equals(SpellingFixResult x, SpellingFixResult y);

        public abstract int GetHashCode(SpellingFixResult obj);

        private class ValueAndFixedValueComparer : SpellingFixResultEqualityComparer
        {
            public override bool Equals(SpellingFixResult x, SpellingFixResult y)
            {
                return StringComparer.CurrentCulture.Equals(x.OldValue, y.OldValue)
                    && StringComparer.CurrentCulture.Equals(x.NewValue, y.NewValue);
            }

            public override int GetHashCode(SpellingFixResult obj)
            {
                return HashCode.Combine(
                    StringComparer.CurrentCulture.GetHashCode(obj.OldValue),
                    StringComparer.CurrentCulture.GetHashCode(obj.NewValue));
            }
        }
    }
}
