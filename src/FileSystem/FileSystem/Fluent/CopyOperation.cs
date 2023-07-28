// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class CopyOperation
{
    internal CopyOperation(Search search, CopyOptions options)
    {
        Search = search ?? throw new ArgumentNullException(nameof(search));
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Search Search { get; }

    public CopyOptions Options { get; }

    public IOperationResult Run(string directoryPath, string destinationPath, CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (destinationPath is null)
            throw new ArgumentNullException(nameof(destinationPath));

        return Search.Copy(directoryPath, destinationPath, Options, cancellationToken);
    }
}
