// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.FileSystem
{
    public class FilePropertyFilter
    {
        public FilePropertyFilter(
            Func<long, bool> sizePredicate = null,
            Func<DateTime, bool> creationTimePredicate = null,
            Func<DateTime, bool> modifiedTimePredicate = null)
        {
            SizePredicate = sizePredicate;
            CreationTimePredicate = creationTimePredicate;
            ModifiedTimePredicate = modifiedTimePredicate;
        }

        public Func<long, bool> SizePredicate { get; }

        public Func<DateTime, bool> CreationTimePredicate { get; }

        public Func<DateTime, bool> ModifiedTimePredicate { get; }

        internal bool IsDefault
        {
            get
            {
                return SizePredicate == null
                    && CreationTimePredicate == null
                    && ModifiedTimePredicate == null;
            }
        }
    }
}
