// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang;

[Flags]
internal enum DisplayParts
{
    None = 0,
    Summary = 1,
    Count = 1 << 1,
}
