// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using CommandLine;
using static Orang.CommandLine.ParseHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            //WriteLine($"Orang Command Line Tool version {typeof(Program).GetTypeInfo().Assembly.GetName().Version}");
            //WriteLine("Copyright (c) Josef Pihrt. All rights reserved.");
            //WriteLine();

            try
            {
                ParserSettings defaultSettings = Parser.Default.Settings;

                var parser = new Parser(settings =>
                {
                    settings.AutoHelp = false;
                    settings.AutoVersion = defaultSettings.AutoVersion;
                    settings.CaseInsensitiveEnumValues = defaultSettings.CaseInsensitiveEnumValues;
                    settings.CaseSensitive = defaultSettings.CaseSensitive;
                    settings.EnableDashDash = true;
                    settings.HelpWriter = defaultSettings.HelpWriter;
                    settings.IgnoreUnknownArguments = defaultSettings.IgnoreUnknownArguments;
                    settings.MaximumDisplayWidth = defaultSettings.MaximumDisplayWidth;
                    settings.ParsingCulture = defaultSettings.ParsingCulture;
                });

                ParserResult<object> parserResult = parser.ParseArguments<
                    DeleteCommandLineOptions,
                    EscapeCommandLineOptions,
                    FindCommandLineOptions,
                    HelpCommandLineOptions,
                    ListSyntaxCommandLineOptions,
                    MatchCommandLineOptions,
                    RenameCommandLineOptions,
                    ReplaceCommandLineOptions,
                    SplitCommandLineOptions
                    >(args);

                bool success = true;

                parserResult.WithParsed<AbstractCommandLineOptions>(options =>
                {
                    success = false;

                    var defaultVerbosity = Verbosity.Normal;

                    if (options.Verbosity == null
                        || TryParseVerbosity(options.Verbosity, out defaultVerbosity))
                    {
                        ConsoleOut.Verbosity = defaultVerbosity;

                        if (TryParseFileLogOptions(options.FileLog, OptionNames.FileLog, out string fileLogPath, out FileLogFlags fileLogFlags, out Verbosity fileLogVerbosity))
                        {
                            if (fileLogPath != null)
                            {
                                FileMode fileMode = ((fileLogFlags & FileLogFlags.Append) != 0)
                                    ? FileMode.Append
                                    : FileMode.Create;

                                var stream = new FileStream(fileLogPath, fileMode, FileAccess.Write, FileShare.Read);
                                var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 4096, leaveOpen: false);
                                Out = new TextWriterWithVerbosity(writer) { Verbosity = fileLogVerbosity };
                            }

                            success = true;
                        }
                        else
                        {
                            success = false;
                        }
                    }
                });

                if (!success)
                    return 1;

                return parserResult.MapResult(
                    (DeleteCommandLineOptions options) => Delete(options),
                    (EscapeCommandLineOptions options) => Escape(options),
                    (FindCommandLineOptions options) => Find(options),
                    (HelpCommandLineOptions options) => Help(options),
                    (ListSyntaxCommandLineOptions options) => ListSyntax(options),
                    (MatchCommandLineOptions options) => Match(options),
                    (RenameCommandLineOptions options) => Rename(options),
                    (ReplaceCommandLineOptions options) => Replace(options),
                    (SplitCommandLineOptions options) => Split(options),
                    _ => 1);
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
            finally
            {
                Out?.Dispose();
                Out = null;
#if DEBUG
                if (Debugger.IsAttached)
                    Console.ReadKey();
#endif
            }

            return 1;
        }

        private static int Delete(DeleteCommandLineOptions commandLineOptions)
        {
            var options = new DeleteCommandOptions();

            if (!commandLineOptions.TryParse(ref options))
                return 1;

            var command = new DeleteCommand(options);

            CommandResult result = command.Execute();

            return GetExitCode(result.Kind);
        }

        private static int Escape(EscapeCommandLineOptions commandLineOptions)
        {
            var options = new EscapeCommandOptions();

            if (!commandLineOptions.TryParse(ref options))
                return 1;

            var command = new EscapeCommand(options);

            CommandResult result = command.Execute();

            return GetExitCode(result.Kind);
        }

        private static int Find(FindCommandLineOptions commandLineOptions)
        {
            var options = new FindCommandOptions();

            if (!commandLineOptions.TryParse(ref options))
                return 1;

            CommandResult result;

            if (options.ContentFilter != null)
            {
                var command = new FindContentCommand(options);

                result = command.Execute();
            }
            else
            {
                var command = new FindCommand(options);

                result = command.Execute();
            }

            return GetExitCode(result.Kind);
        }

        private static int Help(HelpCommandLineOptions commandLineOptions)
        {
            var options = new HelpCommandOptions();

            if (!commandLineOptions.TryParse(ref options))
                return 1;

            var command = new HelpCommand(options);

            CommandResult result = command.Execute();

            return GetExitCode(result.Kind);
        }

        private static int ListSyntax(ListSyntaxCommandLineOptions commandLineOptions)
        {
            var options = new ListSyntaxCommandOptions();

            if (!commandLineOptions.TryParse(ref options))
                return 1;

            var command = new ListSyntaxCommand(options);

            CommandResult result = command.Execute();

            return GetExitCode(result.Kind);
        }

        private static int Match(MatchCommandLineOptions commandLineOptions)
        {
            var options = new MatchCommandOptions();

            if (!commandLineOptions.TryParse(ref options))
                return 1;

            var command = new MatchCommand(options);

            CommandResult result = command.Execute();

            return GetExitCode(result.Kind);
        }

        private static int Rename(RenameCommandLineOptions commandLineOptions)
        {
            var options = new RenameCommandOptions();

            if (!commandLineOptions.TryParse(ref options))
                return 1;

            var command = new RenameCommand(options);

            CommandResult result = command.Execute();

            return GetExitCode(result.Kind);
        }

        private static int Replace(ReplaceCommandLineOptions commandLineOptions)
        {
            var options = new ReplaceCommandOptions();

            if (!commandLineOptions.TryParse(ref options))
                return 1;

            var command = new ReplaceCommand(options);

            CommandResult result = command.Execute();

            return GetExitCode(result.Kind);
        }

        private static int Split(SplitCommandLineOptions commandLineOptions)
        {
            var options = new SplitCommandOptions();

            if (!commandLineOptions.TryParse(ref options))
                return 1;

            var command = new SplitCommand(options);

            CommandResult result = command.Execute();

            return GetExitCode(result.Kind);
        }

        private static int GetExitCode(CommandResultKind kind)
        {
            switch (kind)
            {
                case CommandResultKind.Success:
                    return 0;
                case CommandResultKind.NoMatch:
                    return 1;
                case CommandResultKind.Fail:
                    return 2;
                default:
                    throw new InvalidOperationException($"Unknown enum value '{kind}'.");
            }
        }
    }
}
