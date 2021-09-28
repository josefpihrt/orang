// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;
using CommandLine.Text;
using Orang.CommandLine.Annotations;
using static Orang.CommandLine.ParseHelpers;
using static Orang.Logger;

namespace Orang.CommandLine
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
#if DEBUG
            if (args.LastOrDefault() == "--debug")
            {
                WriteArgs(args.Take(args.Length - 1).ToArray(), Verbosity.Quiet);
                return ExitCodes.NoMatch;
            }

            if (args?.Length > 0
                && !CommandUtility.CheckCommandName(ref args))
            {
                return ExitCodes.Error;
            }
#endif
            try
            {
                Parser parser = CreateParser(ignoreUnknownArguments: true);

                if (args == null
                    || args.Length == 0)
                {
                    HelpCommand.WriteCommandsHelp();
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

                        if (!ParseVerbosityAndOutput(options))
                        {
                            success = false;
                            return;
                        }

                        WriteArgs(args, Verbosity.Diagnostic);

                        if (command != null)
                        {
                            HelpCommand.WriteCommandHelp(command);
                        }
                        else
                        {
                            HelpCommand.WriteCommandsHelp();
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
                    RegexCreateCommandLineOptions,
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
                        WriteWarning("Shortcut '-b' has been deprecated. Use '-A' instead.");

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
                    if (ParseVerbosityAndOutput(options))
                    {
                        WriteArgs(args, Verbosity.Diagnostic);
                    }
                    else
                    {
                        success = false;
                    }
                });

                if (success == false)
                    return ExitCodes.Error;

                return parserResult.MapResult(
                    (CopyCommandLineOptions options) => Copy(options),
                    (RegexCreateCommandLineOptions options) => CreatePattern(options),
                    (DeleteCommandLineOptions options) => Delete(options),
                    (FindCommandLineOptions options) => Find(options),
                    (HelpCommandLineOptions options) => Help(options),
                    (MoveCommandLineOptions options) => Move(options),
                    (RegexEscapeCommandLineOptions options) => RegexEscape(options),
                    (RegexListCommandLineOptions options) => RegexList(options),
                    (RegexMatchCommandLineOptions options) => RegexMatch(options),
                    (RegexSplitCommandLineOptions options) => RegexSplit(options),
                    (RenameCommandLineOptions options) => Rename(options),
                    (ReplaceCommandLineOptions options) => Replace(options),
                    (SpellcheckCommandLineOptions options) => Spellcheck(options),
                    (SyncCommandLineOptions options) => Sync(options),
                    _ => ExitCodes.Error);
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
            finally
            {
                Out?.Dispose();
                Out = null;
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

        private static bool ParseVerbosityAndOutput(AbstractCommandLineOptions options)
        {
            var defaultVerbosity = Verbosity.Normal;

            if (options.Verbosity != null
                && !TryParseVerbosity(options.Verbosity, out defaultVerbosity))
            {
                return false;
            }

            ConsoleOut.Verbosity = defaultVerbosity;

            if (options is BaseCommandLineOptions baseOptions)
            {
                if (!TryParseOutputOptions(
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
                    Out = new TextWriterWithVerbosity(writer) { Verbosity = fileVerbosity };
                }
            }

            return true;
        }

        [Conditional("DEBUG")]
        private static void WriteArgs(string[]? args, Verbosity verbosity)
        {
            if (args != null
                && ShouldLog(verbosity))
            {
                WriteLine("--- ARGS ---", verbosity);

                foreach (string arg in args)
                    WriteLine(arg, verbosity);

                WriteLine("--- END OF ARGS ---", verbosity);
            }
        }

        private static int Copy(CopyCommandLineOptions commandLineOptions)
        {
            var options = new CopyCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new CopyCommand(options), commandLineOptions);
        }

        private static int CreatePattern(RegexCreateCommandLineOptions commandLineOptions)
        {
            var options = new RegexCreateCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new RegexCreateCommand(options), commandLineOptions);
        }

        private static int Delete(DeleteCommandLineOptions commandLineOptions)
        {
            var options = new DeleteCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new DeleteCommand(options), commandLineOptions);
        }

        private static int Find(FindCommandLineOptions commandLineOptions)
        {
            var options = new FindCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new FindCommand<FindCommandOptions>(options), commandLineOptions);
        }

        private static int Help(HelpCommandLineOptions commandLineOptions)
        {
            var options = new HelpCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new HelpCommand(options), commandLineOptions);
        }

        private static int Move(MoveCommandLineOptions commandLineOptions)
        {
            var options = new MoveCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new MoveCommand(options), commandLineOptions);
        }

        private static int Spellcheck(SpellcheckCommandLineOptions commandLineOptions)
        {
            var options = new SpellcheckCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new SpellcheckCommand(options), commandLineOptions);
        }

        private static int Sync(SyncCommandLineOptions commandLineOptions)
        {
            var options = new SyncCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new SyncCommand(options), commandLineOptions);
        }

        private static int Rename(RenameCommandLineOptions commandLineOptions)
        {
            var options = new RenameCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new RenameCommand(options), commandLineOptions);
        }

        private static int Replace(ReplaceCommandLineOptions commandLineOptions)
        {
            var options = new ReplaceCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new ReplaceCommand(options), commandLineOptions);
        }

        private static int RegexEscape(RegexEscapeCommandLineOptions commandLineOptions)
        {
            var options = new RegexEscapeCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new RegexEscapeCommand(options), commandLineOptions);
        }

        private static int RegexList(RegexListCommandLineOptions commandLineOptions)
        {
            var options = new RegexListCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new RegexListCommand(options), commandLineOptions);
        }

        private static int RegexMatch(RegexMatchCommandLineOptions commandLineOptions)
        {
            var options = new RegexMatchCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new RegexMatchCommand(options), commandLineOptions);
        }

        private static int RegexSplit(RegexSplitCommandLineOptions commandLineOptions)
        {
            var options = new RegexSplitCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return ExitCodes.Error;

            return Execute(new RegexSplitCommand(options), commandLineOptions);
        }

        private static int Execute<TOptions>(
            AbstractCommand<TOptions> command,
            AbstractCommandLineOptions options) where TOptions : AbstractCommandOptions
        {
#if DEBUG
            if (ShouldLog(Verbosity.Diagnostic))
            {
                WriteLine("--- RAW PARAMETERS ---", Verbosity.Diagnostic);
                DiagnosticWriter.WriteParameters(options);
                WriteLine("--- END OF RAW PARAMETERS ---", Verbosity.Diagnostic);

                WriteLine("--- APP PARAMETERS ---", Verbosity.Diagnostic);
                command.Options.WriteDiagnostic();
                WriteLine("--- END OF APP PARAMETERS ---", Verbosity.Diagnostic);
            }
#endif
            CommandResult result = command.Execute();

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

        private static class ExitCodes
        {
            public const int Match = 0;
            public const int NoMatch = 1;
            public const int Error = 2;
        }
    }
}
