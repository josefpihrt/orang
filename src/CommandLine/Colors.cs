// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Orang;

internal static class Colors
{
    public static ConsoleColors BasePath { get; } = new(ConsoleColor.DarkGray);
    public static ConsoleColors ContextLine { get; } = new(ConsoleColor.DarkGray);
    public static ConsoleColors EmptyMatch { get; } = new(ConsoleColor.Green);
    public static ConsoleColors EmptyReplacement { get; } = new(ConsoleColor.Cyan);
    public static ConsoleColors EmptySplit { get; } = new(ConsoleColor.Green);
    public static ConsoleColors InvalidReplacement { get; } = new(ConsoleColor.Black, ConsoleColor.Yellow);
    public static ConsoleColors LineNumber { get; } = new(ConsoleColor.Cyan);
    public static ConsoleColors Match { get; } = new(ConsoleColor.Black, ConsoleColor.Green);
    public static ConsoleColors Match_Path { get; } = new(ConsoleColor.Green);
    public static ConsoleColors MatchBoundary { get; } = new(ConsoleColor.Green);
    public static ConsoleColors Matched_Path { get; } = new(ConsoleColor.Cyan);
    public static ConsoleColors Message_OK { get; } = new(ConsoleColor.Green);
    public static ConsoleColors Message_Change { get; } = new(ConsoleColor.Cyan);
    public static ConsoleColors Message_DryRun { get; } = new(ConsoleColor.DarkGray);
    public static ConsoleColors Message_Warning { get; } = new(ConsoleColor.Yellow);
    public static ConsoleColors Path_Progress { get; } = new(ConsoleColor.DarkGray);
    public static ConsoleColors Replacement { get; } = new(ConsoleColor.Black, ConsoleColor.Cyan);
    public static ConsoleColors ReplacementBoundary { get; } = new(ConsoleColor.Cyan);
    public static ConsoleColors Split { get; } = new(ConsoleColor.Black, ConsoleColor.Green);
    public static ConsoleColors SplitBoundary { get; } = new(ConsoleColor.Green);
    public static ConsoleColors Symbol { get; } = new(ConsoleColor.Red);
    public static ConsoleColors Sync_Add { get; } = new(ConsoleColor.Black, ConsoleColor.Green);
    public static ConsoleColors Sync_Error { get; } = new(ConsoleColor.Black, ConsoleColor.Yellow);
    public static ConsoleColors Sync_Delete { get; } = new(ConsoleColor.Black, ConsoleColor.Magenta);
    public static ConsoleColors Sync_Update { get; } = new(ConsoleColor.Black, ConsoleColor.Cyan);
    public static ConsoleColors Sync_Rename { get; } = new(ConsoleColor.Black, ConsoleColor.Cyan);
    public static ConsoleColors Syntax { get; } = new(ConsoleColor.Green);
}
