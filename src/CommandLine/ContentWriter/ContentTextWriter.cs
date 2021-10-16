// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.CommandLine
{
    internal class ContentTextWriter
    {
        private readonly Logger _logger;

        public ContentTextWriter(Logger logger, Verbosity verbosity = Verbosity.Normal)
        {
            Verbosity = verbosity;
            _logger = logger;
        }

        public Verbosity Verbosity { get; }

        public void Write(string? value)
        {
            _logger.Write(value, Verbosity);
        }

        public void Write(string? value, in ConsoleColors colors)
        {
            _logger.Write(value, colors, Verbosity);
        }

        public void Write(string? value, int startIndex, int length)
        {
            _logger.Write(value, startIndex, length, Verbosity);
        }

        public void Write(string? value, int startIndex, int length, in ConsoleColors colors)
        {
            _logger.Write(value, startIndex, length, colors, Verbosity);
        }

        public void WriteLine()
        {
            _logger.WriteLine(Verbosity);
        }
    }
}
