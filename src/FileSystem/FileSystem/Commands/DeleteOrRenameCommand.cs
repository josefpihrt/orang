// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.FileSystem.Commands
{
    internal abstract class DeleteOrRenameCommand : Command, INotifyDirectoryChanged
    {
        protected DeleteOrRenameCommand()
        {
        }

        //TODO: ?
        public event EventHandler<DirectoryChangedEventArgs>? DirectoryChanged;

        protected virtual void OnDirectoryChanged(DirectoryChangedEventArgs e)
        {
            DirectoryChanged?.Invoke(this, e);
        }
    }
}
