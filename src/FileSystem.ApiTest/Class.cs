// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using System.Threading;
using Orang;
using Orang.FileSystem;

namespace N;

public class C
{
    public void M()
    {
        var regex = new Regex("pattern", RegexOptions.IgnoreCase);

        var filter = new Filter(regex);

        var fileSystemFilter = new FileSystemFilter(filter);

        var matches = FileSystemOperation.GetMatches("directoryPath", fileSystemFilter);
    }
}
