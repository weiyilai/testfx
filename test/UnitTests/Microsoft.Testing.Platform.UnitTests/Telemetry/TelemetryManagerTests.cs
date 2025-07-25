﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.Helpers;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Services;
using Microsoft.Testing.Platform.Telemetry;

using Moq;

namespace Microsoft.Testing.Platform.UnitTests;

[TestClass]
public sealed class TelemetryManagerTests
{
    // When set to 1 or true it should suppress the message.
    [DataRow(EnvironmentVariableConstants.TESTINGPLATFORM_NOBANNER, "1")]
    [DataRow(EnvironmentVariableConstants.TESTINGPLATFORM_NOBANNER, "true")]
    [DataRow(EnvironmentVariableConstants.DOTNET_NOLOGO, "1")]
    [DataRow(EnvironmentVariableConstants.DOTNET_NOLOGO, "true")]

    // When set to 0 it should write the message.
    [DataRow(EnvironmentVariableConstants.TESTINGPLATFORM_NOBANNER, "0")]
    [DataRow(EnvironmentVariableConstants.DOTNET_NOLOGO, "0")]
    [TestMethod]
    public async Task TelemetryManager_UsingNoLogoShouldSuppressTelemetryMessage(string variable, string value)
    {
        // Arrange
        TestApplicationOptions options = new();
        Assert.IsTrue(options.EnableTelemetry);

        Mock<IFileSystem> fileSystemMock = new();
        Mock<IEnvironment> environmentMock = new();
        Mock<IOutputDevice> outputDevice = new();
        Mock<ICommandLineOptions> commandLineOptions = new();

        Mock<ITestApplicationModuleInfo> testApplicationModuleInfoMock = new();
        testApplicationModuleInfoMock.Setup(a => a.GetCurrentTestApplicationFullPath()).Returns("directory/myExe.exe");

        ServiceProvider serviceProvider = new();
        serviceProvider.AddService(commandLineOptions.Object);
        serviceProvider.AddService(fileSystemMock.Object);
        serviceProvider.AddService(environmentMock.Object);
        serviceProvider.AddService(outputDevice.Object);
        serviceProvider.AddService(testApplicationModuleInfoMock.Object);

        Mock<ILoggerFactory> loggerFactoryMock = new();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        TelemetryManager telemetryManager = new();
        telemetryManager.AddTelemetryCollectorProvider(_ => new NopTelemetryService(false));

        // Act
        environmentMock.Setup(e => e.GetEnvironmentVariable(variable)).Returns(value);
        await telemetryManager.BuildAsync(serviceProvider, loggerFactoryMock.Object, options);

        // Assert
        if (value != "0")
        {
            // Message is suppressed.
            outputDevice.Verify(c => c.DisplayAsync(It.IsAny<IOutputDeviceDataProducer>(), It.IsAny<IOutputDeviceData>()), Times.Never);
        }
        else
        {
            // Message is not suppressed.
            outputDevice.Verify(c => c.DisplayAsync(It.IsAny<IOutputDeviceDataProducer>(), It.IsAny<IOutputDeviceData>()), Times.Once);
        }
    }

    // When set to 1 or true it should suppress the message.
    [DataRow(EnvironmentVariableConstants.TESTINGPLATFORM_TELEMETRY_OPTOUT, "1")]
    [DataRow(EnvironmentVariableConstants.TESTINGPLATFORM_TELEMETRY_OPTOUT, "true")]
    [DataRow(EnvironmentVariableConstants.DOTNET_CLI_TELEMETRY_OPTOUT, "1")]
    [DataRow(EnvironmentVariableConstants.DOTNET_CLI_TELEMETRY_OPTOUT, "true")]

    // When set to 0 it should write the message.
    [DataRow(EnvironmentVariableConstants.TESTINGPLATFORM_TELEMETRY_OPTOUT, "0")]
    [DataRow(EnvironmentVariableConstants.DOTNET_CLI_TELEMETRY_OPTOUT, "0")]
    [TestMethod]
    public async Task TelemetryManager_UsingTelemetryOptOutShouldDisableTelemetry(string variable, string value)
    {
        // Arrange
        TestApplicationOptions options = new();
        Assert.IsTrue(options.EnableTelemetry);

        Mock<IFileSystem> fileSystemMock = new();
        Mock<IEnvironment> environmentMock = new();
        Mock<IOutputDevice> outputDevice = new();
        Mock<ICommandLineOptions> commandLineOptions = new();

        Mock<ITestApplicationModuleInfo> testApplicationModuleInfoMock = new();
        testApplicationModuleInfoMock.Setup(a => a.GetCurrentTestApplicationFullPath()).Returns("directory/myExe.exe");

        ServiceProvider serviceProvider = new();
        serviceProvider.AddService(commandLineOptions.Object);
        serviceProvider.AddService(fileSystemMock.Object);
        serviceProvider.AddService(environmentMock.Object);
        serviceProvider.AddService(outputDevice.Object);
        serviceProvider.AddService(testApplicationModuleInfoMock.Object);

        Mock<ILoggerFactory> loggerFactoryMock = new();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        TelemetryManager telemetryManager = new();
        telemetryManager.AddTelemetryCollectorProvider(_ => new NopTelemetryService(false));

        // Act
        environmentMock.Setup(e => e.GetEnvironmentVariable(variable)).Returns(value);
        await telemetryManager.BuildAsync(serviceProvider, loggerFactoryMock.Object, options);

        // Assert
        ITelemetryInformation telemetryInformation = serviceProvider.GetRequiredService<ITelemetryInformation>();

        if (value != "0")
        {
            // Telemetry is suppressed.
            Assert.IsFalse(telemetryInformation.IsEnabled);
            outputDevice.Verify(c => c.DisplayAsync(It.IsAny<IOutputDeviceDataProducer>(), It.IsAny<IOutputDeviceData>()), Times.Never);
        }
        else
        {
            // Telemetry is not suppressed.
            Assert.IsTrue(telemetryInformation.IsEnabled);
            outputDevice.Verify(c => c.DisplayAsync(It.IsAny<IOutputDeviceDataProducer>(), It.IsAny<IOutputDeviceData>()), Times.Once);
        }
    }

    [TestMethod]
    public async Task TelemetryManager_SentinelIsWrittenPerUserAndAvoidsShowingNoticeOnSubsequentRuns()
    {
        // Arrange
        TestApplicationOptions options = new();
        Assert.IsTrue(options.EnableTelemetry);

        Mock<IFileSystem> fileSystemMock = new();
        Mock<IEnvironment> environmentMock = new();
        Mock<ICommandLineOptions> commandLineOptions = new();
        environmentMock.Setup(s => s.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create))
            .Returns("sentinelDir");
        Mock<IOutputDevice> outputDevice = new();

        Mock<ITestApplicationModuleInfo> testApplicationModuleInfoMock = new();
        testApplicationModuleInfoMock.Setup(a => a.GetCurrentTestApplicationFullPath()).Returns("directory/myExe.exe");
        testApplicationModuleInfoMock.Setup(a => a.TryGetCurrentTestApplicationFullPath()).Returns("directory/myExe.exe");

        ServiceProvider serviceProvider = new();
        serviceProvider.AddService(commandLineOptions.Object);
        serviceProvider.AddService(fileSystemMock.Object);
        serviceProvider.AddService(environmentMock.Object);
        serviceProvider.AddService(outputDevice.Object);
        serviceProvider.AddService(testApplicationModuleInfoMock.Object);

        Mock<ILoggerFactory> loggerFactoryMock = new();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        TelemetryManager telemetryManager = new();
        telemetryManager.AddTelemetryCollectorProvider(_ => new NopTelemetryService(false));

        // Act
        await telemetryManager.BuildAsync(serviceProvider, loggerFactoryMock.Object, options);

        // Assert
        ITelemetryInformation telemetryInformation = serviceProvider.GetRequiredService<ITelemetryInformation>();

        Assert.IsTrue(telemetryInformation.IsEnabled, "telemetry is enabled");

        // Combination of where LOCALAPPDATA or HOME is, the name of the exe and our file extension.
        string path = Path.Combine("sentinelDir", "Microsoft", "TestingPlatform", "myExe.testingPlatformFirstTimeUseSentinel");
        fileSystemMock.Verify(f => f.ExistFile(path), Times.Once);

        // Message was written to screen.
        outputDevice.Verify(c => c.DisplayAsync(It.IsAny<IOutputDeviceDataProducer>(), It.IsAny<IOutputDeviceData>()), Times.Once);

        // And sentinel was written to filesystem.
        fileSystemMock.Verify(f => f.NewFileStream(path, It.IsAny<FileMode>(), It.IsAny<FileAccess>()), Times.Once);

        // Act - on next run the file already exists.
        outputDevice.Invocations.Clear();
        fileSystemMock.Invocations.Clear();

        fileSystemMock.Setup(f => f.ExistFile(path)).Returns(true);
        await telemetryManager.BuildAsync(serviceProvider, loggerFactoryMock.Object, options);
        fileSystemMock.Verify(f => f.ExistFile(path), Times.Once);

        // Message is not written to screen.
        outputDevice.Verify(c => c.DisplayAsync(It.IsAny<IOutputDeviceDataProducer>(), It.IsAny<IOutputDeviceData>()), Times.Never);

        // And sentinel is not written to filesystem, because it is already there.
        fileSystemMock.Verify(f => f.NewFileStream(path, It.IsAny<FileMode>(), It.IsAny<FileAccess>()), Times.Never);
    }

    [TestMethod]
    public async Task TelemetryManager_SentinelIsWrittenOnlyWhenUserWouldSeeTheMessage()
    {
        // Arrange
        TestApplicationOptions options = new();
        Assert.IsTrue(options.EnableTelemetry);

        Mock<IFileSystem> fileSystemMock = new();
        Mock<IEnvironment> environmentMock = new();
        Mock<ICommandLineOptions> commandLineOptions = new();
        environmentMock.Setup(s => s.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create))
            .Returns("sentinelDir");
        Mock<IOutputDevice> outputDevice = new();

        Mock<ITestApplicationModuleInfo> testApplicationModuleInfoMock = new();
        testApplicationModuleInfoMock.Setup(a => a.GetCurrentTestApplicationFullPath()).Returns("directory/myExe.exe");
        testApplicationModuleInfoMock.Setup(a => a.TryGetCurrentTestApplicationFullPath()).Returns("directory/myExe.exe");

        ServiceProvider serviceProvider = new();
        serviceProvider.AddService(commandLineOptions.Object);
        serviceProvider.AddService(fileSystemMock.Object);
        serviceProvider.AddService(environmentMock.Object);
        serviceProvider.AddService(outputDevice.Object);
        serviceProvider.AddService(testApplicationModuleInfoMock.Object);

        Mock<ILoggerFactory> loggerFactoryMock = new();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        TelemetryManager telemetryManager = new();
        telemetryManager.AddTelemetryCollectorProvider(_ => new NopTelemetryService(false));

        // Act
        // Disable showing the telemetry message.
        environmentMock.Setup(s => s.GetEnvironmentVariable(EnvironmentVariableConstants.TESTINGPLATFORM_NOBANNER)).Returns("1");
        await telemetryManager.BuildAsync(serviceProvider, loggerFactoryMock.Object, options);

        // Assert
        ITelemetryInformation telemetryInformation = serviceProvider.GetRequiredService<ITelemetryInformation>();

        Assert.IsTrue(telemetryInformation.IsEnabled, "telemetry is enabled");

        // Combination of where LOCALAPPDATA or HOME is, the name of the exe and our file extension.
        string path = Path.Combine("sentinelDir", "Microsoft", "TestingPlatform", "myExe.testingPlatformFirstTimeUseSentinel");

        // We should not check for the sentinel, because we disabled the logo.
        fileSystemMock.Verify(f => f.ExistFile(path), Times.Never);

        // Message was not written to screen.
        outputDevice.Verify(c => c.DisplayAsync(It.IsAny<IOutputDeviceDataProducer>(), It.IsAny<IOutputDeviceData>()), Times.Never);

        // And sentinel was not written to filesystem.
        fileSystemMock.Verify(f => f.NewFileStream(path, It.IsAny<FileMode>(), It.IsAny<FileAccess>()), Times.Never);

        // Act - on next run the file should not exist, and we should show it to the user, unless they specify NO_LOGO again.
        outputDevice.Invocations.Clear();
        fileSystemMock.Invocations.Clear();

        // Enable showing the telemetry message.
        environmentMock.Setup(s => s.GetEnvironmentVariable(EnvironmentVariableConstants.TESTINGPLATFORM_NOBANNER)).Returns("0");

        fileSystemMock.Setup(f => f.ExistFile(path)).Returns(false);
        await telemetryManager.BuildAsync(serviceProvider, loggerFactoryMock.Object, options);
        fileSystemMock.Verify(f => f.ExistFile(path), Times.Once);

        // Message is written to screen.
        outputDevice.Verify(c => c.DisplayAsync(It.IsAny<IOutputDeviceDataProducer>(), It.IsAny<IOutputDeviceData>()), Times.Once);

        // And sentinel is written to filesystem, because in the first run the user would not see the message, so we should not write sentinel.
        fileSystemMock.Verify(f => f.NewFileStream(path, It.IsAny<FileMode>(), It.IsAny<FileAccess>()), Times.Once);
    }

    [TestMethod]
    public async Task TelemetryManager_UsingNoBannerCommandLine_ShouldSuppressTelemetryMessage()
    {
        TestApplicationOptions options = new();
        Assert.IsTrue(options.EnableTelemetry);

        Mock<IFileSystem> fileSystemMock = new();
        Mock<IEnvironment> environmentMock = new();
        Mock<IOutputDevice> outputDevice = new();
        Mock<ICommandLineOptions> commandLineOptions = new();
        commandLineOptions.Setup(c => c.IsOptionSet(PlatformCommandLineProvider.NoBannerOptionKey)).Returns(true);

        Mock<ITestApplicationModuleInfo> testApplicationModuleInfoMock = new();
        testApplicationModuleInfoMock.Setup(a => a.GetCurrentTestApplicationFullPath()).Returns("directory/myExe.exe");

        ServiceProvider serviceProvider = new();
        serviceProvider.AddService(commandLineOptions.Object);
        serviceProvider.AddService(fileSystemMock.Object);
        serviceProvider.AddService(environmentMock.Object);
        serviceProvider.AddService(outputDevice.Object);
        serviceProvider.AddService(testApplicationModuleInfoMock.Object);

        Mock<ILoggerFactory> loggerFactoryMock = new();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        TelemetryManager telemetryManager = new();
        await telemetryManager.BuildAsync(serviceProvider, loggerFactoryMock.Object, options);

        outputDevice.Verify(c => c.DisplayAsync(It.IsAny<IOutputDeviceDataProducer>(), It.IsAny<IOutputDeviceData>()), Times.Never);
    }
}
