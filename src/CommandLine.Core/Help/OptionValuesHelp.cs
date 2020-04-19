// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;

namespace Orang
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class OptionValuesHelp
    {
        public OptionValuesHelp(string metaValue, ImmutableArray<OptionValueHelp> values)
        {
            MetaValue = metaValue;
            Values = values;
        }

        public string MetaValue { get; }

        public ImmutableArray<OptionValueHelp> Values { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{MetaValue}  Count = {Values.Length}";
    }
}
