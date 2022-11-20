// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Orang.Text.RegularExpressions;

namespace Orang.CommandLine;

internal class RegexEscapeCommand : AbstractCommand<RegexEscapeCommandOptions>
{
    public RegexEscapeCommand(RegexEscapeCommandOptions options, Logger logger) : base(options, logger)
    {
    }

    protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
    {
        string input = Options.Input;
        string result;

        if (Options.Replacement)
        {
            result = RegexEscape.EscapeSubstitution(input);
        }
        else
        {
            result = RegexEscape.Escape(input, Options.InCharGroup);
        }

        result = result.Replace("\"", "\\\"");

        _logger.WriteLine(result);

        return CommandResult.Success;
    }
}
