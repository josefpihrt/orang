// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Orang
{
    internal abstract class OptionValueProviderEqualityComparer : EqualityComparer<OptionValueProvider>
    {
        public static OptionValueProviderEqualityComparer ByName { get; } = new OptionValueProviderNameEqualityComparer();

        public override abstract bool Equals(OptionValueProvider x, OptionValueProvider y);

        public override abstract int GetHashCode(OptionValueProvider obj);

        private class OptionValueProviderNameEqualityComparer : OptionValueProviderEqualityComparer
        {
            public override bool Equals(OptionValueProvider x, OptionValueProvider y)
            {
                return string.Equals(x?.Name, y?.Name);
            }

            public override int GetHashCode(OptionValueProvider obj)
            {
                return StringComparer.Ordinal.GetHashCode(obj?.Name);
            }
        }
    }
}
