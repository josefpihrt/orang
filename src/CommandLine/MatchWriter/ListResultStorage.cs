// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Orang.CommandLine
{
    internal class ListResultStorage : IResultStorage
    {
        private readonly List<string> _list;

        public ListResultStorage(List<string> list)
        {
            _list = list;
        }

        public void Add(string value)
        {
            _list.Add(value);
        }

        public void Add(string value, int start, int length)
        {
            _list.Add(value.Substring(start, length));
        }
    }
}
