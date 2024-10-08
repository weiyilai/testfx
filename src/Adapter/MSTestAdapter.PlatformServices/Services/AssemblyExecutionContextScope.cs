﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices;

internal readonly struct AssemblyExecutionContextScope : IExecutionContextScope
{
    public AssemblyExecutionContextScope(bool isCleanup) => IsCleanup = isCleanup;

    public bool IsCleanup { get; }
}
