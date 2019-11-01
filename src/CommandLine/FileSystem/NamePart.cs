// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using static Orang.FileSystem.FileSystemHelpers;

namespace Orang.FileSystem
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct NamePart
    {
        public NamePart(string path, int index, int length, NamePartKind kind)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Index = index;
            Length = length;
            Kind = kind;
        }

        public string Path { get; }

        public int Index { get; }

        public int Length { get; }

        public NamePartKind Kind { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Kind} {Path}";

        internal int GetFileNameIndex()
        {
            if (Path == null)
                return -1;

            return (Kind == NamePartKind.Name || Kind == NamePartKind.NameWithoutExtension)
                    ? Index
                    : FileSystemHelpers.GetFileNameIndex(Path);
        }

        public static NamePart FromFile(string path, NamePartKind kind)
        {
            switch (kind)
            {
                case NamePartKind.Name:
                    {
                        int index = FileSystemHelpers.GetFileNameIndex(path);

                        return new NamePart(path, index, path.Length - index, kind);
                    }
                case NamePartKind.FullName:
                    {
                        return new NamePart(path, 0, path.Length, kind);
                    }
                case NamePartKind.NameWithoutExtension:
                    {
                        int index = FileSystemHelpers.GetFileNameIndex(path);

                        int dotIndex = GetExtensionIndex(path);

                        return new NamePart(path, index, dotIndex - index, kind);
                    }
                case NamePartKind.Extension:
                    {
                        int dotIndex = GetExtensionIndex(path);

                        if (dotIndex >= path.Length - 1)
                        {
                            return new NamePart(path, path.Length, 0, kind);
                        }

                        return new NamePart(path, dotIndex + 1, path.Length - dotIndex - 1, kind);
                    }
                default:
                    {
                        throw new ArgumentException("", nameof(kind));
                    }
            }
        }

        public static NamePart FromDirectory(string path, NamePartKind kind)
        {
            switch (kind)
            {
                case NamePartKind.Name:
                case NamePartKind.NameWithoutExtension:
                    {
                        int index = FileSystemHelpers.GetFileNameIndex(path);

                        return new NamePart(path, index, path.Length - index, kind);
                    }
                case NamePartKind.FullName:
                    {
                        return new NamePart(path, 0, path.Length, kind);
                    }
                case NamePartKind.Extension:
                    {
                        return default;
                    }
                default:
                    {
                        throw new ArgumentException("", nameof(kind));
                    }
            }
        }
    }
}
