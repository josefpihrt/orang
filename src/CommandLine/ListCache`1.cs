// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Orang.CommandLine
{
    internal static class ListCache<T>
    {
        private const int MaxSize = 256;
        private const int DefaultCapacity = 16;

        [ThreadStatic]
        private static List<T> _cachedInstance;

        public static List<T> GetInstance(int capacity = DefaultCapacity)
        {
            if (capacity <= MaxSize)
            {
                List<T> list = _cachedInstance;

                if (list != null
                    && capacity <= list.Capacity)
                {
                    _cachedInstance = null;
                    list.Clear();
                    return list;
                }
            }

            return new List<T>(capacity);
        }

        public static void Free(List<T> list)
        {
            if (list.Capacity <= MaxSize)
            {
                list.Clear();
                _cachedInstance = list;
            }
        }
    }
}
