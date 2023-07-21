// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using Orang.FileSystem.Operations;

#pragma warning disable RCS1223 // Mark publicly visible type with DebuggerDisplay attribute.

namespace Orang.FileSystem;

public static class SearchBuilderExtensions
{
    public static DeleteOperation Delete(this SearchBuilder builder, string directoryPath)
    {
        return new DeleteOperation(builder.Build(), directoryPath);
    }

    public static CopyOperation Copy(this SearchBuilder builder, string directoryPath, string destinationPath)
    {
        return new CopyOperation(builder.Build(), directoryPath, destinationPath);
    }

    public static MoveOperation Move(this SearchBuilder builder, string directoryPath, string destinationPath)
    {
        return new MoveOperation(builder.Build(), directoryPath, destinationPath);
    }

    public static RenameOperation Rename(this SearchBuilder builder, string directoryPath, string replacement)
    {
        return new RenameOperation(builder.Build(), directoryPath, replacement);
    }

    public static RenameOperation Rename(this SearchBuilder builder, string directoryPath, MatchEvaluator matchEvaluator)
    {
        return new RenameOperation(builder.Build(), directoryPath, matchEvaluator);
    }

    public static ReplaceOperation Replace(this SearchBuilder builder, string directoryPath, string destinationPath)
    {
        return new ReplaceOperation(builder.Build(), directoryPath, destinationPath);
    }

    public static ReplaceOperation Replace(this SearchBuilder builder, string directoryPath, MatchEvaluator matchEvaluator)
    {
        return new ReplaceOperation(builder.Build(), directoryPath, matchEvaluator);
    }
}
