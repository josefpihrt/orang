// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang.FileSystem.Operations;

internal abstract class DeleteOrRenameOperation : OperationBase, INotifyDirectoryChanged
{
    protected DeleteOrRenameOperation()
    {
    }

    public event EventHandler<DirectoryChangedEventArgs>? DirectoryChanged;

    protected virtual void OnDirectoryChanged(DirectoryChangedEventArgs e)
    {
        DirectoryChanged?.Invoke(this, e);
    }
}
