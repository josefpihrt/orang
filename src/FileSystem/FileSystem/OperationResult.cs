// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.FileSystem;

internal class OperationResult : IOperationResult
{
    public OperationResult(SearchTelemetry telemetry)
    {
        Telemetry = telemetry;
    }

    public SearchTelemetry Telemetry { get; }
}
