// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem.Fluent;

public static class SearchBuilderExtensions
{
    public static IEnumerable<FileMatch> Matches(
        this SearchBuilder builder,
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        return builder.ToSearch().Matches(directoryPath, cancellationToken);
    }

    public static DeleteOperation Delete(this SearchBuilder builder, Action<DeleteOperationBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var deleteBuilder = new DeleteOperationBuilder();
        configure(deleteBuilder);

        return new DeleteOperation(builder.ToSearch(), deleteBuilder.CreateOptions());
    }

    public static CopyOperation Copy(this SearchBuilder builder, Action<CopyOperationBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var copyBuilder = new CopyOperationBuilder();
        configure(copyBuilder);

        return new CopyOperation(builder.ToSearch(), copyBuilder.CreateOptions());
    }

    public static MoveOperation Move(this SearchBuilder builder, Action<MoveOperationBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var moveBuilder = new MoveOperationBuilder();
        configure(moveBuilder);

        return new MoveOperation(builder.ToSearch(), moveBuilder.CreateOptions());
    }

    public static RenameOperation Rename(this SearchBuilder builder, Action<RenameOperationBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var renameBuilder = new RenameOperationBuilder();
        configure(renameBuilder);

        return new RenameOperation(builder.ToSearch(), renameBuilder.CreateOptions());
    }

    public static ReplaceOperation Replace(this SearchBuilder builder, Action<ReplaceOperationBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var replaceBuilder = new ReplaceOperationBuilder();
        configure(replaceBuilder);

        return new ReplaceOperation(builder.ToSearch(), replaceBuilder.CreateOptions());
    }
}
