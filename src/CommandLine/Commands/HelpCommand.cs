// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Orang.CommandLine.Annotations;
using Orang.CommandLine.Help;

namespace Orang.CommandLine;

internal class HelpCommand : AbstractCommand<HelpCommandOptions>
{
    public HelpCommand(HelpCommandOptions options, Logger logger) : base(options, logger)
    {
    }

    protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
    {
        try
        {
            WriteHelp(
                commandName: Options.Command,
                online: Options.Online,
                manual: Options.Manual,
                verbose: _logger.ConsoleOut.Verbosity > Verbosity.Normal,
                matcher: Options.Matcher);
        }
        catch (ArgumentException ex)
        {
            _logger.WriteError(ex);
            return CommandResult.Fail;
        }

        return CommandResult.Success;
    }

    private void WriteHelp(
        string[] commandName,
        bool online,
        bool manual,
        bool verbose,
        Matcher? matcher = null)
    {
        var isCompoundCommand = false;
        string? commandName2 = commandName.FirstOrDefault();

        if (commandName2 is not null)
            isCompoundCommand = CommandUtility.IsCompoundCommand(commandName2);

        if (commandName.Length > 0)
            CommandUtility.CheckCommandName(ref commandName, _logger, showErrorMessage: false);

        commandName2 = commandName.FirstOrDefault();

        if (online)
        {
            OpenHelpInBrowser(commandName2);
        }
        else if (commandName2 is not null)
        {
            Command? command = null;

            if (!isCompoundCommand)
                command = CommandLoader.LoadCommand(typeof(HelpCommand).Assembly, commandName2);

            if (command is null)
                throw new InvalidOperationException($"Command '{string.Join(' ', commandName)}' does not exist.");

            WriteCommandHelp(command, _logger, verbose: verbose, matcher: matcher);
        }
        else if (manual)
        {
            WriteManual(_logger, verbose: verbose, matcher: matcher);
        }
        else
        {
            WriteCommandsHelp(_logger, verbose: verbose, matcher: matcher);
        }
    }

    // https://github.com/dotnet/corefx/issues/10361
    private static void OpenHelpInBrowser(string? commandName)
    {
        var url = "https://josefpihrt.github.io/docs/orang/cli";

        if (commandName is not null)
            url += $"/commands/{commandName}";

        try
        {
            Process.Start(url);
        }
        catch
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var psi = new ProcessStartInfo()
                {
                    FileName = url,
                    UseShellExecute = true
                };

                Process.Start(psi);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

    public static void WriteCommandHelp(Command command, Logger logger, bool verbose = false, Matcher? matcher = null)
    {
        var writer = new ConsoleHelpWriter(logger, new HelpWriterOptions(matcher: matcher));

        IEnumerable<CommandOption> options = command.Options;

        if (!verbose)
        {
            options = options.Where(f => !f.PropertyInfo
                .GetCustomAttributes()
                .Any(f => f is HideFromHelpAttribute || f is HideFromConsoleHelpAttribute));
        }

        options = options.OrderBy(f => f, CommandOptionComparer.Name);

        command = command.WithOptions(options);

        CommandHelp commandHelp = CommandHelp.Create(command, OptionValueProviders.Providers, matcher: matcher);

        writer.WriteCommand(commandHelp);

        if (verbose)
        {
            writer.WriteValues(commandHelp.Values);
        }
        else
        {
            IEnumerable<string> metaValues = OptionValueProviders.GetProviders(
                commandHelp.Options.Select(f => f.Option),
                OptionValueProviders.Providers)
                .Select(f => f.Name);

            if (metaValues.Any())
            {
                logger.WriteLine();
                logger.Write($"Run 'orang help {command.DisplayName} -v d' to display list of option values for ");
                logger.Write(TextHelpers.Join(", ", " and ", metaValues));
                logger.WriteLine(".");
            }
        }
    }

    public static void WriteCommandsHelp(Logger logger, bool verbose = false, Matcher? matcher = null)
    {
        IEnumerable<Command> commands = LoadCommands().Where(f => f.Name != "help");

        CommandsHelp commandsHelp = CommandsHelp.Create(commands, OptionValueProviders.Providers, matcher: matcher);

        var writer = new ConsoleHelpWriter(logger, new HelpWriterOptions(matcher: matcher));

        writer.WriteCommands(commandsHelp);

        if (verbose)
            writer.WriteValues(commandsHelp.Values);

        logger.WriteLine();
        logger.WriteLine(GetFooterText());
    }

    private static void WriteManual(Logger logger, bool verbose = false, Matcher? matcher = null)
    {
        IEnumerable<Command> commands = LoadCommands();

        var writer = new ConsoleHelpWriter(logger, new HelpWriterOptions(matcher: matcher));

        IEnumerable<CommandHelp> commandHelps = commands.Select(f => CommandHelp.Create(f, matcher: matcher))
            .Where(f => f.Arguments.Any() || f.Options.Any())
            .ToImmutableArray();

        ImmutableArray<CommandItem> commandItems = HelpProvider.GetCommandItems(commandHelps.Select(f => f.Command));

        ImmutableArray<OptionValueItemList> values = ImmutableArray<OptionValueItemList>.Empty;

        if (commandItems.Any())
        {
            values = HelpProvider.GetOptionValues(
                commandHelps.SelectMany(f => f.Command.Options),
                OptionValueProviders.Providers,
                matcher);

            var commandsHelp = new CommandsHelp(commandItems, values);

            writer.WriteCommands(commandsHelp);

            foreach (CommandHelp commandHelp in commandHelps)
            {
                WriteSeparator();
                logger.WriteLine();
                logger.WriteLine($"Command: {commandHelp.DisplayName}");
                logger.WriteLine();

                string description = commandHelp.Description;

                if (!string.IsNullOrEmpty(description))
                {
                    logger.WriteLine(description);
                    logger.WriteLine();
                }

                writer.WriteCommand(commandHelp);
            }

            if (verbose)
                WriteSeparator();
        }
        else
        {
            logger.WriteLine();
            logger.WriteLine("No command found");

            if (verbose)
            {
                values = HelpProvider.GetOptionValues(
                    commands.Select(f => CommandHelp.Create(f)).SelectMany(f => f.Command.Options),
                    OptionValueProviders.Providers,
                    matcher);
            }
        }

        if (verbose)
            writer.WriteValues(values);

        void WriteSeparator()
        {
            logger.WriteLine();
            logger.WriteLine("----------");
        }
    }

    private static IEnumerable<Command> LoadCommands()
    {
        return CommandLoader.LoadCommands(typeof(HelpCommand).Assembly)
            .OrderBy(f => f.Name, StringComparer.CurrentCulture);
    }

    internal static string GetHeadingText()
    {
        return $"Orang Command-Line Tool version {typeof(Program).GetTypeInfo().Assembly.GetName().Version}";
    }

    internal static string GetFooterText(string? command = null)
    {
        return $"Run 'orang help {command ?? "[command]"}' for more information on a command."
            + Environment.NewLine
            + $"Run 'orang help {command ?? "[command]"} -v d' for more information on option values.";
    }
}
