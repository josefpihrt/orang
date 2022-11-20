// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Orang.FileSystem;
//TODO: delete FileSystemOperation
//public static class FileSystemOperation
//{
//public static OperationResult Replace(
//    string directoryPath,
//    FileSystemFilter filter,
//    IEnumerable<NameFilter>? directoryFilters = null,
//    ReplaceOptions? replaceOptions = null,
//    SearchTarget searchTarget = SearchTarget.Files,
//    bool recurseSubdirectories = true,
//    bool dryRun = false,
//    CancellationToken cancellationToken = default)
//{
//    if (directoryPath == null)
//        throw new ArgumentNullException(nameof(directoryPath));

//    var searchOptions = new FileSystemSearchOptions(
//        searchTarget: searchTarget,
//        recurseSubdirectories: recurseSubdirectories);

//    var search = new FileSystemSearch(
//        filter,
//        directoryFilters: directoryFilters?.ToImmutableArray() ?? ImmutableArray<NameFilter>.Empty,
//        searchProgress: null,
//        options: searchOptions);

//    if (filter.Content == null)
//        throw new InvalidOperationException("Content filter is not defined.");

//    if (filter.Content.IsNegative)
//        throw new InvalidOperationException("Content filter cannot be negative.");

//    var operation = new ReplaceCommand()
//    {
//        Search = search,
//        ReplaceOptions = replaceOptions ?? ReplaceOptions.Empty,
//        Progress = null,
//        DryRun = dryRun,
//        CancellationToken = cancellationToken,
//        MaxMatchingFiles = 0,
//        MaxMatchesInFile = 0,
//        MaxTotalMatches = 0,
//    };

//    operation.Execute(directoryPath);

//    return new OperationResult(operation.Telemetry);
//}

//public static OperationResult Delete(
//    string directoryPath,
//    FileSystemFilter filter,
//    IEnumerable<NameFilter>? directoryFilters = null,
//    DeleteOptions? deleteOptions = null,
//    SearchTarget searchTarget = SearchTarget.Files,
//    IProgress<OperationProgress>? progress = null,
//    bool recurseSubdirectories = true,
//    bool dryRun = false,
//    CancellationToken cancellationToken = default)
//{
//    if (directoryPath == null)
//        throw new ArgumentNullException(nameof(directoryPath));

//    var searchOptions = new FileSystemSearchOptions(
//        searchTarget: searchTarget,
//        recurseSubdirectories: recurseSubdirectories);

//    var search = new FileSystemSearch(
//        filter,
//        directoryFilters: directoryFilters?.ToImmutableArray() ?? ImmutableArray<NameFilter>.Empty,
//        searchProgress: null,
//        options: searchOptions)
//    {
//        CanRecurseMatch = false
//    };

//    var operation = new DeleteCommand()
//    {
//        Search = search,
//        DeleteOptions = deleteOptions ?? DeleteOptions.Default,
//        Progress = progress,
//        DryRun = dryRun,
//        CancellationToken = cancellationToken,
//        MaxMatchingFiles = 0,
//    };

//    operation.Execute(directoryPath);

//    return new OperationResult(operation.Telemetry);
//}

//public static OperationResult Copy(
//    string directoryPath,
//    string destinationPath,
//    FileSystemFilter filter,
//    IEnumerable<NameFilter>? directoryFilters = null,
//    CopyOptions? copyOptions = null,
//    SearchTarget searchTarget = SearchTarget.Files,
//    IDialogProvider<ConflictInfo>? dialogProvider = null,
//    IProgress<OperationProgress>? progress = null,
//    bool recurseSubdirectories = true,
//    bool dryRun = false,
//    CancellationToken cancellationToken = default)
//{
//    if (directoryPath == null)
//        throw new ArgumentNullException(nameof(directoryPath));

//    if (searchTarget == SearchTarget.All)
//        throw new ArgumentException($"Search target cannot be '{nameof(SearchTarget.All)}'.", nameof(searchTarget));

//    var searchOptions = new FileSystemSearchOptions(
//        searchTarget: searchTarget,
//        recurseSubdirectories: recurseSubdirectories);

//    ImmutableArray<NameFilter> directoryFilters2 = directoryFilters?.ToImmutableArray() ?? ImmutableArray<NameFilter>.Empty;

//    directoryFilters2 = directoryFilters2.Add(NameFilter.CreateFromDirectoryPath(destinationPath, isNegative: true));

//    var search = new FileSystemSearch(
//        filter,
//        directoryFilters: directoryFilters2,
//        searchProgress: null,
//        options: searchOptions);

//    copyOptions ??= CopyOptions.Default;

//    OperationHelpers.VerifyCopyMoveArguments(directoryPath, destinationPath, copyOptions, dialogProvider);

//    var operation = new CopyCommand()
//    {
//        Search = search,
//        DestinationPath = destinationPath,
//        CopyOptions = copyOptions,
//        Progress = progress,
//        DryRun = dryRun,
//        CancellationToken = cancellationToken,
//        DialogProvider = dialogProvider,
//        MaxMatchingFiles = 0,
//        MaxMatchesInFile = 0,
//        MaxTotalMatches = 0,
//        ConflictResolution = copyOptions.ConflictResolution,
//    };

//    operation.Execute(directoryPath);

//    return new OperationResult(operation.Telemetry);
//}

//public static OperationResult Move(
//    string directoryPath,
//    string destinationPath,
//    FileSystemFilter filter,
//    IEnumerable<NameFilter>? directoryFilters = null,
//    CopyOptions? copyOptions = null,
//    SearchTarget searchTarget = SearchTarget.Files,
//    IDialogProvider<ConflictInfo>? dialogProvider = null,
//    IProgress<OperationProgress>? progress = null,
//    bool recurseSubdirectories = true,
//    bool dryRun = false,
//    CancellationToken cancellationToken = default)
//{
//    if (directoryPath == null)
//        throw new ArgumentNullException(nameof(directoryPath));

//    if (searchTarget == SearchTarget.All)
//        throw new ArgumentException($"Search target cannot be '{nameof(SearchTarget.All)}'.", nameof(searchTarget));

//    var searchOptions = new FileSystemSearchOptions(
//        searchTarget: searchTarget,
//        recurseSubdirectories: recurseSubdirectories);

//    ImmutableArray<NameFilter> directoryFilters2 = directoryFilters?.ToImmutableArray() ?? ImmutableArray<NameFilter>.Empty;

//    directoryFilters2 = directoryFilters2.Add(NameFilter.CreateFromDirectoryPath(destinationPath, isNegative: true));

//    var search = new FileSystemSearch(
//        filter,
//        directoryFilters: directoryFilters2,
//        searchProgress: null,
//        options: searchOptions);

//    copyOptions ??= CopyOptions.Default;

//    OperationHelpers.VerifyCopyMoveArguments(directoryPath, destinationPath, copyOptions, dialogProvider);

//    var operation = new MoveCommand()
//    {
//        Search = search,
//        DestinationPath = destinationPath,
//        CopyOptions = copyOptions,
//        Progress = progress,
//        DryRun = dryRun,
//        CancellationToken = cancellationToken,
//        DialogProvider = dialogProvider,
//        MaxMatchingFiles = 0,
//        MaxMatchesInFile = 0,
//        MaxTotalMatches = 0,
//        ConflictResolution = copyOptions.ConflictResolution,
//    };

//    operation.Execute(directoryPath);

//    return new OperationResult(operation.Telemetry);
//}
//}
