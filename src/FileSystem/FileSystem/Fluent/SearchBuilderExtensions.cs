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

    public static DeleteOperationBuilder Delete(this SearchBuilder builder)
    {
        return new DeleteOperationBuilder(builder.ToSearch());
    }

    public static CopyOperationBuilder Copy(this SearchBuilder builder)
    {
        return new CopyOperationBuilder(builder.ToSearch());
    }

    public static MoveOperationBuilder Move(this SearchBuilder builder)
    {
        return new MoveOperationBuilder(builder.ToSearch());
    }

    public static RenameOperationBuilder Rename(this SearchBuilder builder, string replacement)
    {
        return new RenameOperationBuilder(builder.ToSearch(), replacement);
    }

    public static RenameOperationBuilder Rename(this SearchBuilder builder, MatchEvaluator matchEvaluator)
    {
        return new RenameOperationBuilder(builder.ToSearch(), matchEvaluator);
    }

    public static ReplaceOperationBuilder Replace(this SearchBuilder builder, string replacement)
    {
        return new ReplaceOperationBuilder(builder.ToSearch(), replacement);
    }

    public static ReplaceOperationBuilder Replace(this SearchBuilder builder, MatchEvaluator matchEvaluator)
    {
        return new ReplaceOperationBuilder(builder.ToSearch(), matchEvaluator);
    }
}
