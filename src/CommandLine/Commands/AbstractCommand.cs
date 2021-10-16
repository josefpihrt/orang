// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;

namespace Orang.CommandLine
{
    internal abstract class AbstractCommand<TOptions> where TOptions : AbstractCommandOptions
    {
        private ContentTextWriter? _contentWriter;
        protected Logger _logger;

        protected AbstractCommand(TOptions options, Logger logger)
        {
            Options = options;
            _logger = logger;
        }

        public TOptions Options { get; }

        protected ContentTextWriter ContentWriter
        {
            get
            {
                if (_contentWriter == null)
                    Interlocked.CompareExchange(ref _contentWriter, new ContentTextWriter(_logger), null);

                return _contentWriter;
            }
        }

        protected abstract CommandResult ExecuteCore(CancellationToken cancellationToken = default);

        public CommandResult Execute(CancellationToken cancellationToken = default)
        {
            try
            {
                return ExecuteCore(cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                OperationCanceled(ex);
            }
            catch (AggregateException ex)
            {
                OperationCanceledException? operationCanceledException = ex.GetOperationCanceledException();

                if (operationCanceledException != null)
                {
                    OperationCanceled(operationCanceledException);
                }
                else
                {
                    throw;
                }
            }

            return CommandResult.Canceled;
        }

        protected virtual void OperationCanceled(OperationCanceledException ex)
        {
            OperationCanceled();
        }

        protected virtual void OperationCanceled()
        {
            _logger.WriteLine();
            _logger.WriteLine("Operation was canceled.");
        }
    }
}
