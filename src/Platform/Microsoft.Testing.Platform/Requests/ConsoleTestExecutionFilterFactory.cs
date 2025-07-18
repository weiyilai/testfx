﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Helpers;
using Microsoft.Testing.Platform.Resources;

namespace Microsoft.Testing.Platform.Requests;

internal sealed class ConsoleTestExecutionFilterFactory(ICommandLineOptions commandLineService) : ITestExecutionFilterFactory
{
    private readonly ICommandLineOptions _commandLineService = commandLineService;

    public string Uid => nameof(ConsoleTestExecutionFilterFactory);

    public string Version => AppVersion.DefaultSemVer;

    public string DisplayName => PlatformResources.ConsoleTestExecutionFilterFactoryDisplayName;

    public string Description => PlatformResources.ConsoleTestExecutionFilterFactoryDescription;

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public Task<(bool Success, ITestExecutionFilter? TestExecutionFilter)> TryCreateAsync()
    {
        bool hasTreenodeFilter = _commandLineService.TryGetOptionArgumentList(TreeNodeFilterCommandLineOptionsProvider.TreenodeFilter, out string[]? treenodeFilter);
        bool hasTestNodeUidFilter = _commandLineService.TryGetOptionArgumentList(PlatformCommandLineProvider.FilterUidOptionKey, out string[]? uidFilter);
        ITestExecutionFilter filter = (hasTreenodeFilter, hasTestNodeUidFilter) switch
        {
            (true, true) => throw new NotSupportedException(PlatformResources.OnlyOneFilterSupported),
            (true, false) => new TreeNodeFilter(treenodeFilter![0]),
            (false, true) => new TestNodeUidListFilter([.. uidFilter!.Select(x => new TestNodeUid(x))]),
            (false, false) => new NopFilter(),
        };

        return Task.FromResult<(bool, ITestExecutionFilter?)>((true, filter));
    }
}
