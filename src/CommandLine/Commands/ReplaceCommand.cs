﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Orang.CommandLine;

internal class ReplaceCommand : CommonReplaceCommand<ReplaceCommandOptions>
{
    public ReplaceCommand(ReplaceCommandOptions options, Logger logger) : base(options, logger)
    {
        Debug.Assert(!options.ContentFilter!.Invert);
    }
}
