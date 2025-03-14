﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable CS8618 // Properties below are set by MSBuild.

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Testing.Platform.MSBuild;

/// <summary>
/// A task that copies the Microsoft Testing Platform configuration file to the output directory.
/// </summary>
// Took inspiration from https://github.com/dotnet/sdk/blob/main/src/Tasks/Microsoft.NET.Build.Tasks/GenerateRuntimeConfigurationFiles.cs
public sealed class ConfigurationFileTask : Build.Utilities.Task
{
    private const string ConfigurationFileNameSuffix = "testconfig.json";
    private readonly IFileSystem _fileSystem;

    internal ConfigurationFileTask(IFileSystem? fileSystem)
    {
        Guard.NotNull(fileSystem);
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationFileTask"/> class.
    /// </summary>
    public ConfigurationFileTask()
        : this(new FileSystem())
    {
    }

    /// <summary>
    /// Gets or sets the Microsoft Testing Platform configuration file source.
    /// </summary>
    [Required]
    public ITaskItem TestingPlatformConfigurationFileSource { get; set; }

    /// <summary>
    /// Gets or sets the MSBuild project directory.
    /// </summary>
    [Required]
    public ITaskItem MSBuildProjectDirectory { get; set; }

    /// <summary>
    /// Gets or sets the assembly name.
    /// </summary>
    [Required]
    public ITaskItem AssemblyName { get; set; }

    /// <summary>
    /// Gets or sets the output path.
    /// </summary>
    [Required]
    public ITaskItem OutputPath { get; set; }

    /// <summary>
    /// Gets or sets the final Microsoft Testing Platform configuration file.
    /// </summary>
    [Output]
    public ITaskItem FinalTestingPlatformConfigurationFile { get; set; }

    /// <inheritdoc/>
    public override bool Execute()
    {
        Log.LogMessage(MessageImportance.Normal, $"Microsoft Testing Platform configuration file: '{TestingPlatformConfigurationFileSource.ItemSpec}'");
        if (!_fileSystem.Exist(TestingPlatformConfigurationFileSource.ItemSpec))
        {
            Log.LogMessage(MessageImportance.Normal, "Microsoft Testing Platform configuration file not found");
            return true;
        }

        Log.LogMessage(MessageImportance.Normal, $"MSBuildProjectDirectory: '{MSBuildProjectDirectory.ItemSpec}'");
        Log.LogMessage(MessageImportance.Normal, $"AssemblyName: '{AssemblyName.ItemSpec}'");
        Log.LogMessage(MessageImportance.Normal, $"OutputPath: '{OutputPath.ItemSpec}'");

        string finalPath = Path.Combine(MSBuildProjectDirectory.ItemSpec, OutputPath.ItemSpec);
        Log.LogMessage(MessageImportance.Normal, $"Final path: '{finalPath}'");

        string finalFileName = Path.Combine(finalPath, $"{AssemblyName.ItemSpec}.{ConfigurationFileNameSuffix}");
        Log.LogMessage(MessageImportance.Normal, $"Final configuration file path : '{finalFileName}'");

        Log.LogMessage(MessageImportance.Normal, $"Configuration file found: '{TestingPlatformConfigurationFileSource.ItemSpec}'");
        _fileSystem.CopyFile(TestingPlatformConfigurationFileSource.ItemSpec, finalFileName);
        FinalTestingPlatformConfigurationFile = new TaskItem(finalFileName);
        Log.LogMessage(MessageImportance.Normal, "Microsoft Testing Platform configuration file written");

        return true;
    }
}
