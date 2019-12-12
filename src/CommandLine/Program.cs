// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using CommandLine;
using CommandLine.Text;
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

            if (args != null)
            {
                if (args.Length == 1)
                {
                    if (IsHelpOption(args[0]))
                    {
                        Console.Write(HelpProvider.GetHelpText());
#if DEBUG
                        if (Debugger.IsAttached)
                            Console.ReadKey();
#endif
                        return 0;
                    }
                }
                else if (args.Length == 2)
                {
                    if (args?.Length == 2
                        && IsHelpOption(args[1]))
                    {
                        Command command = CommandLoader.LoadCommand(typeof(HelpCommand).Assembly, args[0]);

                        if (command != null)
                        {
                            Console.Write(HelpProvider.GetHelpText(command));
#if DEBUG
                            if (Debugger.IsAttached)
                                Console.ReadKey();
#endif
                            return 0;
                        }
                    }
                }
            }

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
                    settings.HelpWriter = null;
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

                bool help = false;
                bool success = true;

                parserResult.WithNotParsed(_ =>
                {
                    var helpText = new HelpText(SentenceBuilder.Create(), HelpProvider.GetHeadingText());

                    helpText = HelpText.DefaultParsingErrorsHandler(parserResult, helpText);

                    VerbAttribute verbAttribute = parserResult.TypeInfo.Current.GetCustomAttribute<VerbAttribute>();

                    if (verbAttribute != null)
                    {
                        helpText.AddPreOptionsText(Environment.NewLine + HelpProvider.GetFooterText(verbAttribute.Name));
                    }

                    Console.Error.WriteLine(helpText);

                    success = false;
                });

                parserResult.WithParsed<AbstractCommandLineOptions>(options =>
                {
                    if (options.Help)
                    {
                        string commandName = options.GetType().GetCustomAttribute<VerbAttribute>().Name;

                        Console.WriteLine(HelpProvider.GetHelpText(commandName));

                        help = true;
                        return;
                    }

                    success = false;

                    var defaultVerbosity = Verbosity.Normal;

                    if (options.Verbosity == null
                        || TryParseVerbosity(options.Verbosity, out defaultVerbosity))
                    {
                        ConsoleOut.Verbosity = defaultVerbosity;

                        if (TryParseOutputOptions(options.Output, OptionNames.Output, out string filePath, out Verbosity fileVerbosity, out Encoding encoding, out bool append))
                        {
                            if (filePath != null)
                            {
                                FileMode fileMode = (append)
                                    ? FileMode.Append
                                    : FileMode.Create;

                                var stream = new FileStream(filePath, fileMode, FileAccess.Write, FileShare.Read);
                                var writer = new StreamWriter(stream, encoding, bufferSize: 4096, leaveOpen: false);
                                Out = new TextWriterWithVerbosity(writer) { Verbosity = fileVerbosity };
                            }

                            success = true;
                        }
                        else
                        {
                            success = false;
                        }
                    }
                });

                if (help)
                    return 0;

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

            bool IsHelpOption(string value)
            {
                if (value.StartsWith("--"))
                    return string.Compare(value, 2, OptionNames.Help, 0, OptionNames.Help.Length, StringComparison.Ordinal) == 0;

                return value.Length == 2
                    && value[0] == '-'
                    && value[1] == OptionShortNames.Help;
            }
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
