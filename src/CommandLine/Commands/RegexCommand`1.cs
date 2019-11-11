// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal abstract class RegexCommand<TOptions> : AbstractCommand where TOptions : RegexCommandOptions
    {
        protected RegexCommand(TOptions options)
        {
            Options = options;
        }

        public TOptions Options { get; }

        protected sealed override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            StreamWriter writer = null;

            try
            {
                string path = Options.OutputPath;

                if (path != null)
                {
                    WriteLine($"Opening '{path}'", Verbosity.Diagnostic);

                    writer = new StreamWriter(path, false, Options.Output.Encoding);
                }

                return ExecuteCore(writer, cancellationToken);
            }
            catch (Exception ex) when (ex is IOException
                || ex is UnauthorizedAccessException)
            {
                WriteWarning(ex);
            }
            finally
            {
                writer?.Dispose();
            }

            return CommandResult.Fail;
        }

        protected abstract CommandResult ExecuteCore(TextWriter output, CancellationToken cancellationToken = default);
    }
}
