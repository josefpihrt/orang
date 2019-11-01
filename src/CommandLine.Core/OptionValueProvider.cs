// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Orang
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class OptionValueProvider
    {
        public OptionValueProvider(string name, params OptionValue[] values)
        {
            Name = name;
            Values = values.ToImmutableArray();
        }

        public OptionValueProvider(string name, IEnumerable<OptionValue> values)
        {
            Name = name;
            Values = values.ToImmutableArray();
        }

        public string Name { get; }

        public ImmutableArray<OptionValue> Values { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => Name;

        public bool TryParseEnum<TEnum>(string value, out TEnum result) where TEnum : struct
        {
            foreach (OptionValue optionValue in Values)
            {
                if (optionValue is SimpleOptionValue enumOptionValue)
                {
                    if (enumOptionValue.Value == value
                        || enumOptionValue.ShortValue == value)
                    {
                        return Enum.TryParse(optionValue.Name, out result);
                    }
                }
            }

            result = default;
            return false;
        }

        public OptionValue GetValue(string name)
        {
            foreach (OptionValue value in Values)
            {
                if (string.Equals(value.Name, name, StringComparison.Ordinal))
                    return value;
            }

            return default;
        }

        public string GetHelpText(Func<OptionValue, bool> predicate = null)
        {
            IEnumerable<string> values = (predicate != null)
                ? Values.Where(predicate).Select(f => f.HelpValue)
                : Values.Select(f => f.HelpValue);

            return TextHelpers.Join(", ", " and ", values);
        }
    }
}
