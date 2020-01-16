// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Orang.CommandLine
{
    internal class ModifyOptions
    {
        public static ModifyOptions Default { get; } = new ModifyOptions();

        public ModifyOptions(
            ModifyFunctions functions = ModifyFunctions.None,
            bool aggregate = false,
            bool ignoreCase = false,
            bool cultureInvariant = false,
            ValueSortProperty sortProperty = ValueSortProperty.None,
            Func<IEnumerable<string>, IEnumerable<string>> modify = null)
        {
            Functions = functions;
            Aggregate = aggregate;
            IgnoreCase = ignoreCase;
            CultureInvariant = cultureInvariant;
            SortProperty = sortProperty;
            Modify = modify;
        }

        public ModifyFunctions Functions { get; }

        public bool Aggregate { get; }

        public bool IgnoreCase { get; }

        public bool CultureInvariant { get; }

        public ValueSortProperty SortProperty { get; }

        public Func<IEnumerable<string>, IEnumerable<string>> Modify { get; }

        public StringComparer StringComparer
        {
            get
            {
                if (IgnoreCase)
                {
                    return (CultureInvariant)
                        ? StringComparer.InvariantCultureIgnoreCase
                        : StringComparer.CurrentCultureIgnoreCase;
                }
                else
                {
                    return (CultureInvariant)
                        ? StringComparer.InvariantCulture
                        : StringComparer.CurrentCulture;
                }
            }
        }

        public bool HasAnyFunction
        {
            get
            {
                return Functions != ModifyFunctions.None
                    || Modify != null;
            }
        }
    }
}
