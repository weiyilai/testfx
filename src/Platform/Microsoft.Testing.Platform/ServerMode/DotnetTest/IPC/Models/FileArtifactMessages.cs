﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Testing.Platform.IPC.Models;

internal sealed record FileArtifactMessage(string? FullPath, string? DisplayName, string? Description, string? TestUid, string? TestDisplayName, string? SessionUid);

internal sealed record FileArtifactMessages(string? ExecutionId, string? InstanceId, FileArtifactMessage[] FileArtifacts) : IRequest;
