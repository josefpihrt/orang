// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine
{
    internal enum CommandResultKind
    {
        Success = 0,
        NoMatch = 1,
        Fail = 2,
        Canceled = 3,
    }
}
