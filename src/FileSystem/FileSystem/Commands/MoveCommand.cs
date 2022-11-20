// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace Orang.FileSystem.Commands;

internal class MoveCommand : CommonCopyCommand
{
    protected override void ExecuteOperation(string sourcePath, string destinationPath)
    {
        File.Move(sourcePath, destinationPath);
    }
}
