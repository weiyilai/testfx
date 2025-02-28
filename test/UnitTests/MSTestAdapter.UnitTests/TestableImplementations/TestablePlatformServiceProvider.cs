﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices;
using Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.Interface;
using Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.Interface.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using Moq;

using UTF = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.TestPlatform.MSTestAdapter.UnitTests.TestableImplementations;

internal class TestablePlatformServiceProvider : IPlatformServiceProvider
{
    public TestablePlatformServiceProvider()
    {
        MockTestSourceValidator = new Mock<ITestSource>();
        MockFileOperations = new Mock<IFileOperations>();
        MockTraceLogger = new Mock<IAdapterTraceLogger>();
        MockTestSourceHost = new Mock<ITestSourceHost>();
        MockTestDeployment = new Mock<ITestDeployment>();
        MockSettingsProvider = new Mock<ISettingsProvider>();
        MockTestDataSource = new Mock<ITestDataSource>();
        MockTraceListener = new Mock<ITraceListener>();
        MockTraceListenerManager = new Mock<ITraceListenerManager>();
        MockThreadOperations = new Mock<IThreadOperations>();
    }

    #region Mock Implementations

    public Mock<ITestSource> MockTestSourceValidator
    {
        get;
        set;
    }

    public Mock<IFileOperations> MockFileOperations
    {
        get;
        set;
    }

    public Mock<IAdapterTraceLogger> MockTraceLogger
    {
        get;
        set;
    }

    public Mock<ITestSourceHost> MockTestSourceHost
    {
        get;
        set;
    }

    public Mock<ITestDeployment> MockTestDeployment
    {
        get;
        set;
    }

    public Mock<ISettingsProvider> MockSettingsProvider
    {
        get;
        set;
    }

    public Mock<ITestDataSource> MockTestDataSource
    {
        get;
        set;
    }

    public Mock<ITraceListener> MockTraceListener
    {
        get;
        set;
    }

    public Mock<ITraceListenerManager> MockTraceListenerManager
    {
        get;
        set;
    }

    public Mock<IThreadOperations> MockThreadOperations
    {
        get;
        set;
    }

    public Mock<IReflectionOperations2> MockReflectionOperations
    {
        get;
        set;
    }

    #endregion

    public ITestSource TestSource => MockTestSourceValidator.Object;

    public IFileOperations FileOperations => MockFileOperations.Object;

    public IAdapterTraceLogger AdapterTraceLogger { get => MockTraceLogger.Object; set => throw new NotSupportedException(); }

    public ITestDeployment TestDeployment => MockTestDeployment.Object;

    public ISettingsProvider SettingsProvider => MockSettingsProvider.Object;

    public IThreadOperations ThreadOperations => MockThreadOperations.Object;

    public IReflectionOperations2 ReflectionOperations
    {
        get => MockReflectionOperations != null
            ? MockReflectionOperations.Object
            : field ??= new ReflectionOperations2();
        private set;
    }

    public ITestDataSource TestDataSource => MockTestDataSource.Object;

    public TestRunCancellationToken TestRunCancellationToken { get; set; }

    public bool IsGracefulStopRequested { get; set; }

    public ITestContext GetTestContext(ITestMethod testMethod, StringWriter writer, IDictionary<string, object> properties, IMessageLogger messageLogger, UTF.UnitTestOutcome outcome)
    {
        var testContextImpl = new TestContextImplementation(testMethod, writer, properties, messageLogger);
        testContextImpl.SetOutcome(outcome);
        return testContextImpl;
    }

    public ITestSourceHost CreateTestSourceHost(string source, TestPlatform.ObjectModel.Adapter.IRunSettings runSettings, TestPlatform.ObjectModel.Adapter.IFrameworkHandle frameworkHandle) => MockTestSourceHost.Object;

    public ITraceListener GetTraceListener(TextWriter textWriter) => MockTraceListener.Object;

    [SuppressMessage("Naming", "CA1725:Parameter names should match base declaration", Justification = "Part of the public API")]
    public ITraceListenerManager GetTraceListenerManager(TextWriter standardOutputWriter, TextWriter standardErrorWriter) => MockTraceListenerManager.Object;

    public void SetupMockReflectionOperations() => MockReflectionOperations = new Mock<IReflectionOperations2>();
}
