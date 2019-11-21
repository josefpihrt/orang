// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Orang.CommandLine
{
    internal static class SortHelpers
    {
        public static IEnumerable<SearchResult> SortResults(List<SearchResult> results, ImmutableArray<SortDescriptor> descriptors)
        {
            if (!descriptors.Any())
                return results;

            IComparer<SearchResult> comparer = SearchResultComparer.GetInstance(descriptors[0].Property);

            if (descriptors.Length == 1)
            {
                if (descriptors[0].Direction == SortDirection.Ascending)
                {
                    results.Sort(comparer);
                }
                else
                {
                    results.Sort((x, y) => -comparer.Compare(x, y));
                }

                return results;
            }
            else
            {
                IOrderedEnumerable<SearchResult> sorted = (descriptors[0].Direction == SortDirection.Ascending)
                    ? results.OrderBy(f => f, comparer)
                    : results.OrderByDescending(f => f, comparer);

                for (int i = 1; i < descriptors.Length; i++)
                {
                    comparer = SearchResultComparer.GetInstance(descriptors[i].Property);

                    sorted = (descriptors[i].Direction == SortDirection.Ascending)
                        ? sorted.ThenBy(f => f, comparer)
                        : sorted.ThenByDescending(f => f, comparer);
                }

                return sorted;
            }
        }
    }
}
