// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;
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

    public static DeleteOperationBuilder WithDelete(this SearchBuilder builder, string directoryPath)
    {
        return new DeleteOperationBuilder(builder.ToSearch(), directoryPath);
    }

    public static CopyOperationBuilder WithCopy(this SearchBuilder builder, string directoryPath, string destinationPath)
    {
        return new CopyOperationBuilder(builder.ToSearch(), directoryPath, destinationPath);
    }

    public static MoveOperationBuilder WithMove(this SearchBuilder builder, string directoryPath, string destinationPath)
    {
        return new MoveOperationBuilder(builder.ToSearch(), directoryPath, destinationPath);
    }

    public static RenameOperationBuilder WithRename(this SearchBuilder builder, string directoryPath, string replacement)
    {
        return new RenameOperationBuilder(builder.ToSearch(), directoryPath, replacement);
    }

    public static RenameOperationBuilder WithRename(this SearchBuilder builder, string directoryPath, MatchEvaluator matchEvaluator)
    {
        return new RenameOperationBuilder(builder.ToSearch(), directoryPath, matchEvaluator);
    }

    public static ReplaceOperationBuilder WithReplace(this SearchBuilder builder, string directoryPath, string destinationPath)
    {
        return new ReplaceOperationBuilder(builder.ToSearch(), directoryPath, destinationPath);
    }

    public static ReplaceOperationBuilder WithReplace(this SearchBuilder builder, string directoryPath, MatchEvaluator matchEvaluator)
    {
        return new ReplaceOperationBuilder(builder.ToSearch(), directoryPath, matchEvaluator);
    }
}
