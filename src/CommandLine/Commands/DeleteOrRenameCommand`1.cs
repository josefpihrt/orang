// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Orang.FileSystem;

namespace Orang.CommandLine
{
    internal abstract class DeleteOrRenameCommand<TOptions> : FileSystemCommand<TOptions>, INotifyDirectoryChanged where TOptions : DeleteOrRenameCommandOptions
    {
        protected DeleteOrRenameCommand(TOptions options) : base(options)
        {
        }

        public event EventHandler<DirectoryChangedEventArgs> DirectoryChanged;

        protected abstract void ProcessMatch(FileMatch fileMatch, SearchContext context, string baseDirectoryPath = null);

        protected virtual void OnDirectoryChanged(DirectoryChangedEventArgs e)
        {
            DirectoryChanged?.Invoke(this, e);
        }

        protected sealed override void ExecuteFile(string filePath, SearchContext context)
        {
            FileMatch fileMatch = MatchFile(filePath, context.Progress);

            if (fileMatch != null)
                ProcessMatch(fileMatch, context);
        }

        protected sealed override void ExecuteDirectory(string directoryPath, SearchContext context)
        {
            foreach (FileMatch fileMatch in Find(directoryPath, context, notifyDirectoryChanged: this))
            {
                Debug.Assert(fileMatch.Path.StartsWith(directoryPath, FileSystemHelpers.Comparison), $"{directoryPath}\r\n{fileMatch.Path}");

                ProcessMatch(fileMatch, context, directoryPath);

                if (context.TerminationReason == TerminationReason.Canceled)
                    break;

                if (context.TerminationReason == TerminationReason.MaxReached)
                    break;
            }
        }

        protected sealed override void ExecuteResult(SearchResult result, SearchContext context, ColumnWidths columnWidths)
        {
            ExecuteMatch(result.FileMatch, context, result.BaseDirectoryPath, columnWidths);
        }

        protected bool AskToExecute(SearchContext context, string question, string indent)
        {
            DialogResult result = ConsoleHelpers.Ask((Options.DryRun) ? "Continue?" : question, indent);

            switch (result)
            {
                case DialogResult.Yes:
                    {
                        return true;
                    }
                case DialogResult.YesToAll:
                    {
                        Options.Ask = false;
                        return true;
                    }
                case DialogResult.No:
                case DialogResult.None:
                    {
                        return false;
                    }
                case DialogResult.NoToAll:
                case DialogResult.Cancel:
                    {
                        context.TerminationReason = TerminationReason.Canceled;
                        return false;
                    }
                default:
                    {
                        throw new InvalidOperationException($"Unknown enum value '{result}'.");
                    }
            }
        }
    }
}
