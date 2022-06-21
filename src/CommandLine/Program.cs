// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using Orang.CommandLine.Annotations;

namespace Orang.CommandLine
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var logger = new Logger(ConsoleWriter.Instance, null);
            var parseContext = new ParseContext(logger);

#if DEBUG
            if (args.LastOrDefault() == "--debug")
            {
                WriteArgs(args.Take(args.Length - 1).ToArray(), Verbosity.Quiet, logger);
                return ExitCodes.NoMatch;
            }
#endif
            if (args?.Length > 0
                && !CommandUtility.CheckCommandName(ref args, logger))
            {
                return ExitCodes.Error;
            }

            try
            {
                Parser parser = CreateParser(ignoreUnknownArguments: true);

                if (args == null
                    || args.Length == 0)
                {
                    HelpCommand.WriteCommandsHelp(logger);
                    return ExitCodes.Match;
                }

                bool? success = null;

                ParserResult<BaseCommandLineOptions> defaultResult = parser
                    .ParseArguments<BaseCommandLineOptions>(args)
                    .WithParsed(options =>
                    {
                        if (!options.Help)
                            return;

                        string? commandName = args?.FirstOrDefault();
                        Command? command = (commandName != null)
                            ? CommandLoader.LoadCommand(typeof(Program).Assembly, commandName)
                            : null;

                        if (!ParseVerbosityAndOutput(options, parseContext))
                        {
                            success = false;
                            return;
                        }

                        WriteArgs(args, Verbosity.Diagnostic, logger);

                        if (command != null)
                        {
                            HelpCommand.WriteCommandHelp(command, logger);
                        }
                        else
                        {
                            HelpCommand.WriteCommandsHelp(logger);
                        }

                        success = true;
                    });

                if (success == false)
                    return ExitCodes.Error;

                if (success == true)
                    return ExitCodes.Match;

                parser = CreateParser();

                ParserResult<object> parserResult = parser.ParseArguments<
                    CopyCommandLineOptions,
                    DeleteCommandLineOptions,
                    FindCommandLineOptions,
                    HelpCommandLineOptions,
                    MoveCommandLineOptions,
                    RegexEscapeCommandLineOptions,
                    RegexListCommandLineOptions,
                    RegexMatchCommandLineOptions,
                    RegexSplitCommandLineOptions,
                    RenameCommandLineOptions,
                    ReplaceCommandLineOptions,
                    SpellcheckCommandLineOptions,
                    SyncCommandLineOptions
                    >(args);

                parserResult.WithNotParsed(e =>
                {
                    if (e.Any(f => f.Tag == ErrorType.VersionRequestedError))
                    {
                        Console.WriteLine(typeof(Program).GetTypeInfo().Assembly.GetName().Version);
                        success = true;
                        return;
                    }

                    UnknownOptionError oldAttributesToSkipShortName = e
                        .OfType<UnknownOptionError>()
                        .FirstOrDefault(f => f.Token == "b");

                    if (oldAttributesToSkipShortName != null)
                        logger.WriteWarning("Shortcut '-b' has been deprecated. Use '-A' instead.");

                    var helpText = new HelpText(SentenceBuilder.Create(), HelpCommand.GetHeadingText());

                    helpText = HelpText.DefaultParsingErrorsHandler(parserResult, helpText);

                    VerbAttribute? verbAttribute = parserResult.TypeInfo.Current.GetCustomAttribute<VerbAttribute>();

                    if (verbAttribute != null)
                    {
                        CommandAliasAttribute? commandAlias = parserResult.TypeInfo.Current.GetCustomAttribute<CommandAliasAttribute>();

                        string commandName = commandAlias?.Alias ?? verbAttribute.Name;

                        helpText.AddPreOptionsText(Environment.NewLine + HelpCommand.GetFooterText(commandName));
                    }

                    Console.Error.WriteLine(helpText);

                    success = false;
                });

                if (success == true)
                    return ExitCodes.Match;

                if (success == false)
                    return ExitCodes.Error;

                parserResult.WithParsed<AbstractCommandLineOptions>(options =>
                {
                    if (ParseVerbosityAndOutput(options, parseContext))
                    {
                        WriteArgs(args, Verbosity.Diagnostic, logger);
                    }
                    else
                    {
                        success = false;
                    }
                });

                if (success == false)
                    return ExitCodes.Error;

                return parserResult.MapResult(
                    (CopyCommandLineOptions options) => Copy(options, parseContext),
                    (DeleteCommandLineOptions options) => Delete(options, parseContext),
                    (FindCommandLineOptions options) => Find(options, parseContext),
                    (HelpCommandLineOptions options) => Help(options, parseContext),
                    (MoveCommandLineOptions options) => Move(options, parseContext),
                    (RegexEscapeCommandLineOptions options) => RegexEscape(options, parseContext),
                    (RegexListCommandLineOptions options) => RegexList(options, parseContext),
                    (RegexMatchCommandLineOptions options) => RegexMatch(options, parseContext),
                    (RegexSplitCommandLineOptions options) => RegexSplit(options, parseContext),
                    (RenameCommandLineOptions options) => Rename(options, parseContext),
                    (ReplaceCommandLineOptions options) => Replace(options, parseContext),
                    (SpellcheckCommandLineOptions options) => Spellcheck(options, parseContext),
                    (SyncCommandLineOptions options) => Sync(options, parseContext),
                    _ => ExitCodes.Error);
            }
            catch (Exception ex)
            {
                logger.WriteError(ex);
            }
            finally
            {
                logger.Out?.Dispose();
                logger.Out = null;
            }

            return ExitCodes.Error;
        }

        private static Parser CreateParser(bool? ignoreUnknownArguments = null)
        {
            ParserSettings defaultSettings = Parser.Default.Settings;

            return new Parser(settings =>
            {
                settings.AutoHelp = false;
                settings.AutoVersion = defaultSettings.AutoVersion;
                settings.CaseInsensitiveEnumValues = defaultSettings.CaseInsensitiveEnumValues;
                settings.CaseSensitive = defaultSettings.CaseSensitive;
                settings.EnableDashDash = true;
                settings.HelpWriter = null;
                settings.IgnoreUnknownArguments = ignoreUnknownArguments ?? defaultSettings.IgnoreUnknownArguments;
                settings.MaximumDisplayWidth = defaultSettings.MaximumDisplayWidth;
                settings.ParsingCulture = defaultSettings.ParsingCulture;
            });
        }

        private static bool ParseVerbosityAndOutput(AbstractCommandLineOptions options, ParseContext context)
        {
            var defaultVerbosity = Verbosity.Normal;

            if (options.Verbosity != null
                && !context.TryParseVerbosity(options.Verbosity, out defaultVerbosity))
            {
                return false;
            }

            context.Logger.ConsoleOut.Verbosity = defaultVerbosity;

            if (options is BaseCommandLineOptions baseOptions)
            {
                if (!context.TryParseOutputOptions(
                    baseOptions.Output,
                    OptionNames.Output,
                    out string? filePath,
                    out Verbosity fileVerbosity,
                    out Encoding? encoding,
                    out bool append))
                {
                    return false;
                }

                if (filePath != null)
                {
                    FileMode fileMode = (append)
                        ? FileMode.Append
                        : FileMode.Create;

                    var stream = new FileStream(filePath, fileMode, FileAccess.Write, FileShare.Read);
                    var writer = new StreamWriter(stream, encoding, bufferSize: 4096, leaveOpen: false);
                    context.Logger.Out = new LogWriter(writer) { Verbosity = fileVerbosity };
                }
            }

            return true;
        }

        [Conditional("DEBUG")]
        private static void WriteArgs(string[]? args, Verbosity verbosity, Logger logger)
        {
            if (args != null
                && logger.ShouldWrite(verbosity))
            {
                logger.WriteLine("--- ARGS ---", verbosity);

                foreach (string arg in args)
                    logger.WriteLine(arg, verbosity);

                logger.WriteLine("--- END OF ARGS ---", verbosity);
            }
        }

        private static int Copy(CopyCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new CopyCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new CopyCommand(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int Delete(DeleteCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new DeleteCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new DeleteCommand(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int Find(FindCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new FindCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new FindCommand<FindCommandOptions>(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int Help(HelpCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new HelpCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new HelpCommand(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int Move(MoveCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new MoveCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new MoveCommand(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int Spellcheck(SpellcheckCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new SpellcheckCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new SpellcheckCommand(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int Sync(SyncCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new SyncCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new SyncCommand(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int Rename(RenameCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new RenameCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new RenameCommand(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int Replace(ReplaceCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new ReplaceCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new ReplaceCommand(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int RegexEscape(RegexEscapeCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new RegexEscapeCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new RegexEscapeCommand(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int RegexList(RegexListCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new RegexListCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new RegexListCommand(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int RegexMatch(RegexMatchCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new RegexMatchCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new RegexMatchCommand(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int RegexSplit(RegexSplitCommandLineOptions commandLineOptions, ParseContext context)
        {
            var options = new RegexSplitCommandOptions();

            if (!commandLineOptions.TryParse(options, context))
                return ExitCodes.Error;

            return Execute(new RegexSplitCommand(options, context.Logger), commandLineOptions, context.Logger);
        }

        private static int Execute<TOptions>(
            AbstractCommand<TOptions> command,
            AbstractCommandLineOptions options,
            Logger logger) where TOptions : AbstractCommandOptions
        {
#if DEBUG
            if (logger.ShouldWrite(Verbosity.Diagnostic))
            {
                logger.WriteLine("--- RAW PARAMETERS ---", Verbosity.Diagnostic);
                var writer = new DiagnosticWriter(logger);
                writer.WriteParameters(options);
                logger.WriteLine("--- END OF RAW PARAMETERS ---", Verbosity.Diagnostic);

                logger.WriteLine("--- APP PARAMETERS ---", Verbosity.Diagnostic);
                command.Options.WriteDiagnostic(writer);
                logger.WriteLine("--- END OF APP PARAMETERS ---", Verbosity.Diagnostic);

                if (logger.ConsoleOut.Verbosity == Verbosity.Diagnostic)
                    ConsoleHelpers.WaitForKeyPress();
            }
#endif
            CancellationTokenSource? cts = null;

            try
            {
                cts = new CancellationTokenSource();

                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                CancellationToken cancellationToken = cts.Token;

                CommandResult result = command.Execute(cancellationToken);

                switch (result)
                {
                    case CommandResult.Success:
                        return ExitCodes.Match;
                    case CommandResult.NoMatch:
                        return ExitCodes.NoMatch;
                    case CommandResult.Fail:
                    case CommandResult.Canceled:
                        return ExitCodes.Error;
                    default:
                        throw new InvalidOperationException($"Unknown enum value '{result}'.");
                }
            }
            finally
            {
                cts?.Dispose();
            }
        }

        private static class ExitCodes
        {
            public const int Match = 0;
            public const int NoMatch = 1;
            public const int Error = 2;
        }
    }
}
