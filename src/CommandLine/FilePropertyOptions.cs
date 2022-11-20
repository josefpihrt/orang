// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang;

internal class FilePropertyOptions
{
    public FilePropertyOptions(
        bool includeCreationTime = false,
        bool includeModifiedTime = false,
        bool includeSize = false,
        Func<DateTime, bool>? creationTimePredicate = null,
        Func<DateTime, bool>? modifiedTimePredicate = null,
        Func<long, bool>? sizePredicate = null)
    {
        IncludeCreationTime = includeCreationTime || creationTimePredicate != null;
        IncludeModifiedTime = includeModifiedTime || modifiedTimePredicate != null;
        IncludeSize = includeSize || sizePredicate != null;
        CreationTimePredicate = creationTimePredicate;
        ModifiedTimePredicate = modifiedTimePredicate;
        SizePredicate = sizePredicate;
    }

    public bool IncludeCreationTime { get; }

    public bool IncludeModifiedTime { get; }

    public bool IncludeSize { get; }

    public Func<DateTime, bool>? CreationTimePredicate { get; }

    public Func<DateTime, bool>? ModifiedTimePredicate { get; }

    public Func<long, bool>? SizePredicate { get; }

    public bool IsEmpty
    {
        get
        {
            return !IncludeCreationTime
                && !IncludeModifiedTime
                && !IncludeSize
                && CreationTimePredicate == null
                && ModifiedTimePredicate == null
                && SizePredicate == null;
        }
    }

    public FilePropertyOptions WithIncludeCreationTime(bool includeCreationTime)
    {
        return new FilePropertyOptions(
            includeCreationTime: includeCreationTime,
            includeModifiedTime: IncludeModifiedTime,
            includeSize: IncludeSize,
            creationTimePredicate: CreationTimePredicate,
            modifiedTimePredicate: ModifiedTimePredicate,
            sizePredicate: SizePredicate);
    }

    public FilePropertyOptions WithIncludeModifiedTime(bool includeModifiedTime)
    {
        return new FilePropertyOptions(
            includeCreationTime: IncludeCreationTime,
            includeModifiedTime: includeModifiedTime,
            includeSize: IncludeSize,
            creationTimePredicate: CreationTimePredicate,
            modifiedTimePredicate: ModifiedTimePredicate,
            sizePredicate: SizePredicate);
    }

    public FilePropertyOptions WithIncludeSize(bool includeSize)
    {
        return new FilePropertyOptions(
            includeCreationTime: IncludeCreationTime,
            includeModifiedTime: IncludeModifiedTime,
            includeSize: includeSize,
            creationTimePredicate: CreationTimePredicate,
            modifiedTimePredicate: ModifiedTimePredicate,
            sizePredicate: SizePredicate);
    }
}
