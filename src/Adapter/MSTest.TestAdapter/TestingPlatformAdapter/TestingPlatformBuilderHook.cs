﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !WINDOWS_UWP
using Microsoft.Testing.Platform.Builder;

namespace Microsoft.VisualStudio.TestTools.UnitTesting;

public static class TestingPlatformBuilderHook
{
#pragma warning disable IDE0060 // Remove unused parameter
    public static void AddExtensions(ITestApplicationBuilder testApplicationBuilder, string[] arguments) => testApplicationBuilder.AddMSTest(() => [Assembly.GetEntryAssembly()!]);
}
#endif
