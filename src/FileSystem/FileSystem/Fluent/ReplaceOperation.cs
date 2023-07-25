// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public class ReplaceOperation
{
    public ReplaceOperation(Search search, ReplaceOptions options)
    {
        Search = search ?? throw new ArgumentNullException(nameof(search));
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Search Search { get; }

    public ReplaceOptions Options { get; }

    public IOperationResult Run(string directoryPath, string? replacement = null, CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        return Search.Replace(directoryPath, replacement, Options, cancellationToken);
    }

    public IOperationResult Run(string directoryPath, MatchEvaluator matchEvaluator, CancellationToken cancellationToken = default)
    {
        if (directoryPath is null)
            throw new ArgumentNullException(nameof(directoryPath));

        if (matchEvaluator is null)
            throw new ArgumentNullException(nameof(matchEvaluator));

        return Search.Replace(directoryPath, matchEvaluator, Options, cancellationToken);
    }

    public IOperationResult Run(
        IEnumerable<string> directoryPaths,
        string? replacement = null,
        CancellationToken cancellationToken = default)
    {
        if (directoryPaths is null)
            throw new ArgumentNullException(nameof(directoryPaths));

        var telemetry = new SearchTelemetry();

        foreach (string directoryPath in directoryPaths)
        {
            IOperationResult result = Search.Replace(directoryPath, replacement, Options, cancellationToken);
            telemetry.Add(result.Telemetry);
        }

        return new OperationResult(telemetry);
    }

    public IOperationResult Run(
        IEnumerable<string> directoryPaths,
        MatchEvaluator matchEvaluator,
        CancellationToken cancellationToken = default)
    {
        if (directoryPaths is null)
            throw new ArgumentNullException(nameof(directoryPaths));

        if (matchEvaluator is null)
            throw new ArgumentNullException(nameof(matchEvaluator));

        var telemetry = new SearchTelemetry();

        foreach (string directoryPath in directoryPaths)
        {
            IOperationResult result = Search.Replace(directoryPath, matchEvaluator, Options, cancellationToken);
            telemetry.Add(result.Telemetry);
        }

        return new OperationResult(telemetry);
    }
}
