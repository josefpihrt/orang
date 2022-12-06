// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.FileSystem.Fluent;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

public class RenameBuilder
{
    private readonly string directoryPath;

    internal RenameBuilder(string directoryPath)
    {
        this.directoryPath = directoryPath;
    }
}
