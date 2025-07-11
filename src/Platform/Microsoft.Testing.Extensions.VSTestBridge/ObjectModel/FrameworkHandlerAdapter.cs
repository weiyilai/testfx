﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Extensions.VSTestBridge.Helpers;
using Microsoft.Testing.Platform.Capabilities.TestFramework;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.Messages;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Services;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using TestSessionContext = Microsoft.Testing.Platform.TestHost.TestSessionContext;

namespace Microsoft.Testing.Extensions.VSTestBridge.ObjectModel;

/// <summary>
/// Bridge implementation of <see cref="IFrameworkHandle"/> that forwards calls to VSTest and Microsoft Testing Platforms.
/// </summary>
internal sealed class FrameworkHandlerAdapter : IFrameworkHandle
{
    /// <remarks>
    /// Not null when used in the context of VSTest.
    /// </remarks>
    private readonly IFrameworkHandle? _frameworkHandle;
    private readonly ILogger<FrameworkHandlerAdapter> _logger;
    private readonly IMessageBus _messageBus;
    private readonly VSTestBridgedTestFrameworkBase _adapterExtensionBase;
    private readonly TestSessionContext _session;
    private readonly CancellationToken _cancellationToken;
    private readonly bool _isTrxEnabled;
    private readonly MessageLoggerAdapter _comboMessageLogger;
    private readonly string _testAssemblyPath;
    private readonly INamedFeatureCapability? _namedFeatureCapability;
    private readonly ICommandLineOptions _commandLineOptions;
    private readonly IClientInfo _clientInfo;

    public FrameworkHandlerAdapter(
        VSTestBridgedTestFrameworkBase adapterExtensionBase,
        TestSessionContext session,
        string[] testAssemblyPaths,
        ITestApplicationModuleInfo testApplicationModuleInfo,
        INamedFeatureCapability? namedFeatureCapability,
        ICommandLineOptions commandLineOptions,
        IClientInfo clientInfo,
        IMessageBus messageBus,
        IOutputDevice outputDevice,
        ILoggerFactory loggerFactory,
        bool isTrxEnabled,
        CancellationToken cancellationToken,
        IFrameworkHandle? frameworkHandle = null)
    {
        if (testAssemblyPaths.Length == 0)
        {
            throw new ArgumentException($"{nameof(testAssemblyPaths)} should contain at least one test assembly.");
        }
        else if (testAssemblyPaths.Length > 1)
        {
            _testAssemblyPath = testApplicationModuleInfo.GetCurrentTestApplicationFullPath();

            if (!testAssemblyPaths.Contains(_testAssemblyPath))
            {
                throw new ArgumentException("None of the test assemblies are the test application.");
            }
        }
        else
        {
            _testAssemblyPath = testAssemblyPaths[0];
        }

        _namedFeatureCapability = namedFeatureCapability;
        _commandLineOptions = commandLineOptions;
        _clientInfo = clientInfo;
        _frameworkHandle = frameworkHandle;
        _logger = loggerFactory.CreateLogger<FrameworkHandlerAdapter>();
        _messageBus = messageBus;
        _adapterExtensionBase = adapterExtensionBase;
        _session = session;
        _cancellationToken = cancellationToken;
        _isTrxEnabled = isTrxEnabled;
        _comboMessageLogger = new MessageLoggerAdapter(loggerFactory, outputDevice, adapterExtensionBase, frameworkHandle);
    }

    /// <inheritdoc/>
    public bool EnableShutdownAfterTestRun
    {
        get => _frameworkHandle?.EnableShutdownAfterTestRun ?? false;
        set
        {
            _logger.LogTrace($"{nameof(FrameworkHandlerAdapter)}.EnableShutdownAfterTestRun: set to {value}");
            _frameworkHandle?.EnableShutdownAfterTestRun = value;
        }
    }

    /// <inheritdoc/>
    public int LaunchProcessWithDebuggerAttached(string filePath, string? workingDirectory, string? arguments,
        IDictionary<string, string?>? environmentVariables)
    {
        _logger.LogTrace($"{nameof(FrameworkHandlerAdapter)}.LaunchProcessWithDebuggerAttached");
        return _frameworkHandle?.LaunchProcessWithDebuggerAttached(filePath, workingDirectory, arguments, environmentVariables)
            ?? -1;
    }

    /// <inheritdoc/>
    public void RecordAttachments(IList<AttachmentSet> attachmentSets)
    {
        _logger.LogTrace($"{nameof(FrameworkHandlerAdapter)}.RecordAttachments");
        _frameworkHandle?.RecordAttachments(attachmentSets);
        PublishTestSessionAttachmentsAsync(attachmentSets).Await();
    }

    /// <inheritdoc/>
    public void RecordEnd(TestCase testCase, TestOutcome outcome)
    {
        _logger.LogTrace($"{nameof(FrameworkHandlerAdapter)}.RecordEnd");

        _cancellationToken.ThrowIfCancellationRequested();

        testCase.FixUpTestCase(_testAssemblyPath);

        // Forward call to VSTest
        _frameworkHandle?.RecordEnd(testCase, outcome);
    }

    /// <inheritdoc/>
    public void RecordResult(TestResult testResult)
    {
        _logger.LogTrace($"{nameof(FrameworkHandlerAdapter)}.RecordResult");

        _cancellationToken.ThrowIfCancellationRequested();

        testResult.TestCase.FixUpTestCase(_testAssemblyPath);

        // Forward call to VSTest
        _frameworkHandle?.RecordResult(testResult);

        // Publish node state change to Microsoft Testing Platform
        var testNode = testResult.ToTestNode(_isTrxEnabled, _adapterExtensionBase.UseFullyQualifiedNameAsTestNodeUid, _namedFeatureCapability, _commandLineOptions, _clientInfo);

        var testNodeChange = new TestNodeUpdateMessage(_session.SessionUid, testNode);
        _messageBus.PublishAsync(_adapterExtensionBase, testNodeChange).Await();
    }

    /// <inheritdoc/>
    public void RecordStart(TestCase testCase)
    {
        _logger.LogTrace($"{nameof(FrameworkHandlerAdapter)}.RecordStart");

        _cancellationToken.ThrowIfCancellationRequested();

        testCase.FixUpTestCase(_testAssemblyPath);

        // Forward call to VSTest
        _frameworkHandle?.RecordStart(testCase);

        // Publish node state change to Microsoft Testing Platform
        var testNode = testCase.ToTestNode(_isTrxEnabled, _adapterExtensionBase.UseFullyQualifiedNameAsTestNodeUid, _namedFeatureCapability, _commandLineOptions, _clientInfo);
        testNode.Properties.Add(InProgressTestNodeStateProperty.CachedInstance);
        var testNodeChange = new TestNodeUpdateMessage(_session.SessionUid, testNode);

        _messageBus.PublishAsync(_adapterExtensionBase, testNodeChange).Await();
    }

    /// <inheritdoc/>
    public void SendMessage(TestMessageLevel testMessageLevel, string message)
        => _comboMessageLogger.SendMessage(testMessageLevel, message);

    private async Task PublishTestSessionAttachmentsAsync(IEnumerable<AttachmentSet> attachments)
    {
        foreach (AttachmentSet attachmentSet in attachments)
        {
            foreach (UriDataAttachment attachment in attachmentSet.Attachments)
            {
                if (!attachment.Uri.IsFile)
                {
                    throw new FormatException($"Test adapter {_adapterExtensionBase.DisplayName} only supports file attachments.");
                }

                var fileArtifact = new SessionFileArtifact(_session.SessionUid, new(attachment.Uri.LocalPath), attachmentSet.DisplayName, attachment.Description);
                await _messageBus.PublishAsync(_adapterExtensionBase, fileArtifact).ConfigureAwait(false);
            }
        }
    }
}
