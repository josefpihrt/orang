// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.FileSystem
{
    internal enum PathOrigin
    {
        Argument = 0,
        File = 1,
        RedirectedInput = 2,
        CurrentDirectory = 3,
    }
}
