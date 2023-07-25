// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class DeleteOperation
{
    public DeleteOperation(Search search, DeleteOptions options)
    {
        Search = search ?? throw new ArgumentNullException(nameof(search));
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Search Search { get; }

    public DeleteOptions Options { get; }

    public IOperationResult Run(string directoryPath, CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        return Search.Delete(directoryPath, Options, cancellationToken);
    }

    public IOperationResult Run(IEnumerable<string> directoryPaths, CancellationToken cancellationToken = default)
    {
        if (directoryPaths is null)
            throw new ArgumentNullException(nameof(directoryPaths));

        var telemetry = new SearchTelemetry();

        foreach (string directoryPath in directoryPaths)
        {
            IOperationResult result = Search.Delete(directoryPath, Options, cancellationToken);
            telemetry.Add(result.Telemetry);
        }

        return new OperationResult(telemetry);
    }
}
