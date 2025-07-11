﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Testing.Platform.Helpers;

internal sealed class SystemProcess : IProcess, IDisposable
{
    private readonly Process _process;

#pragma warning disable CA1416 // Validate platform compatibility
    public SystemProcess(Process process)
    {
        _process = process;
        _process.Exited += OnProcessExited;
    }

    public event EventHandler? Exited;

    public bool HasExited => _process.HasExited;

    public int Id => _process.Id;

    public string Name => _process.ProcessName;

    public int ExitCode => _process.ExitCode;

    public IMainModule? MainModule
        => _process.MainModule is null
            ? null
            : new SystemMainModule(_process.MainModule);

    private void OnProcessExited(object? sender, EventArgs e)
        => Exited?.Invoke(sender, e);

    public void WaitForExit()
        => _process.WaitForExit();

    public Task WaitForExitAsync()
        => _process.WaitForExitAsync();

    public void Kill()
        => _process.Kill(true);

    public void Dispose() => _process.Dispose();
#pragma warning restore CA1416
}
