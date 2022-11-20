// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using Orang;
using Orang.FileSystem;
using Orang.FileSystem.Operations;

namespace N;

public class C
{
    public void M()
    {
        var regex = new Regex("pattern", RegexOptions.IgnoreCase);

        var nameFilter = new Filter(regex);

        var fileSystemFilter = new FileSystemFilter(nameFilter);
    }

    public void M2()
    {
        var nameFilter = new Filter("pattern", RegexOptions.IgnoreCase);

        var fileSystemFilter = new FileSystemFilter(nameFilter);

        var renameOperation = new RenameOperation("", fileSystemFilter);
    }

    public static void SM()
    {
    }
}
