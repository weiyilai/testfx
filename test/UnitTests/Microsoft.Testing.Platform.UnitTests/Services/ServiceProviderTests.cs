﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Capabilities;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.Extensions.TestHostControllers;
using Microsoft.Testing.Platform.Services;
using Microsoft.Testing.Platform.TestHost;

namespace Microsoft.Testing.Platform.UnitTests;

[TestClass]
public sealed class ServiceProviderTests
{
    private readonly ServiceProvider _serviceProvider = new();

    [TestMethod]
    public void GetService_InternalExtension_ShouldNotReturn()
    {
        _serviceProvider.AddService(new TestHostProcessLifetimeHandler());
        Assert.IsNull(_serviceProvider.GetService<ITestHostProcessLifetimeHandler>());

        _serviceProvider.AddService(new TestHostEnvironmentVariableProvider());
        Assert.IsNull(_serviceProvider.GetService<ITestHostEnvironmentVariableProvider>());

        _serviceProvider.AddService(new TestSessionLifetimeHandler());
        Assert.IsNull(_serviceProvider.GetService<ITestSessionLifetimeHandler>());

        _serviceProvider.AddService(new DataConsumer());
        Assert.IsNull(_serviceProvider.GetService<IDataConsumer>());

        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());
#pragma warning disable CS0618 // Type or member is obsolete
        Assert.IsNull(_serviceProvider.GetService<ITestApplicationLifecycleCallbacks>());
#pragma warning restore CS0618 // Type or member is obsolete
        Assert.IsNull(_serviceProvider.GetService<ITestHostApplicationLifetime>());
    }

    [TestMethod]
    public void GetServiceInternal_InternalExtension_ShouldReturn()
    {
        _serviceProvider.AddService(new TestHostProcessLifetimeHandler());
        Assert.IsNotNull(_serviceProvider.GetServiceInternal<ITestHostProcessLifetimeHandler>());

        _serviceProvider.AddService(new TestHostEnvironmentVariableProvider());
        Assert.IsNotNull(_serviceProvider.GetServiceInternal<ITestHostEnvironmentVariableProvider>());

        _serviceProvider.AddService(new TestSessionLifetimeHandler());
        Assert.IsNotNull(_serviceProvider.GetServiceInternal<ITestSessionLifetimeHandler>());

        _serviceProvider.AddService(new DataConsumer());
        Assert.IsNotNull(_serviceProvider.GetServiceInternal<IDataConsumer>());

        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());
#pragma warning disable CS0618 // Type or member is obsolete
        Assert.IsNotNull(_serviceProvider.GetServiceInternal<ITestApplicationLifecycleCallbacks>());
#pragma warning restore CS0618 // Type or member is obsolete
        Assert.IsNotNull(_serviceProvider.GetServiceInternal<ITestHostApplicationLifetime>());
    }

    [TestMethod]
    public void Clone_WithoutFilter_Succeeded()
    {
        _serviceProvider.AddService(new TestHostProcessLifetimeHandler());
        _serviceProvider.AddService(new TestHostEnvironmentVariableProvider());
        _serviceProvider.AddService(new TestSessionLifetimeHandler());
        _serviceProvider.AddService(new DataConsumer());
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());

        var clonedServiceProvider = (ServiceProvider)_serviceProvider.Clone();

        Assert.AreEqual(_serviceProvider.Services.Count, clonedServiceProvider.Services.Count);
        for (int i = 0; i < _serviceProvider.Services.Count; i++)
        {
            Assert.AreEqual(_serviceProvider.Services.ToArray()[i], clonedServiceProvider.Services.ToArray()[i]);
        }
    }

    [TestMethod]
    public void Clone_WithFilter_Succeeded()
    {
        _serviceProvider.AddService(new TestHostProcessLifetimeHandler());
        _serviceProvider.AddService(new TestHostEnvironmentVariableProvider());
        _serviceProvider.AddService(new TestSessionLifetimeHandler());
        _serviceProvider.AddService(new DataConsumer());
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());

        var clonedServiceProvider = (ServiceProvider)_serviceProvider.Clone(o => o is TestHostProcessLifetimeHandler);

        Assert.HasCount(1, clonedServiceProvider.Services);
        Assert.AreEqual(_serviceProvider.Services.ToArray()[0].GetType(), typeof(TestHostProcessLifetimeHandler));
    }

    [TestMethod]
    public void AddService_TestFramework_ShouldFail()
        => Assert.ThrowsExactly<ArgumentException>(() => _serviceProvider.AddService(new TestFramework()));

    [TestMethod]
    public void TryAddService_TestFramework_ShouldFail()
        => Assert.ThrowsExactly<ArgumentException>(() => _serviceProvider.TryAddService(new TestFramework()));

    [TestMethod]
    public void AddService_TestFramework_ShouldNotFail()
    {
        _serviceProvider.AllowTestAdapterFrameworkRegistration = true;
        _serviceProvider.AddService(new TestFramework());
    }

    [TestMethod]
    public void TryAddService_TestFramework_ShouldNotFail()
    {
        _serviceProvider.AllowTestAdapterFrameworkRegistration = true;
        _serviceProvider.TryAddService(new TestFramework());
    }

    [TestMethod]
    public void AddService_SameInstance_ShouldFail()
    {
        TestHostProcessLifetimeHandler instance = new();
        _serviceProvider.AddService(instance);
        _ = Assert.ThrowsExactly<InvalidOperationException>(() => _serviceProvider.AddService(instance));
    }

    [TestMethod]
    public void AddService_SameInstance_ShouldNotFail()
    {
        TestHostProcessLifetimeHandler instance = new();
        _serviceProvider.AddService(instance);
        _serviceProvider.AddService(instance, throwIfSameInstanceExit: false);
    }

    [TestMethod]
    public void AddServices_SameInstance_ShouldFail()
    {
        TestHostProcessLifetimeHandler instance = new();
        _serviceProvider.AddServices([instance]);
        _ = Assert.ThrowsExactly<InvalidOperationException>(() => _serviceProvider.AddServices([instance]));
    }

    [TestMethod]
    public void AddServices_SameInstance_ShouldNotFail()
    {
        TestHostProcessLifetimeHandler instance = new();
        _serviceProvider.AddServices([instance]);
        _serviceProvider.AddServices([instance], throwIfSameInstanceExit: false);
    }

    [TestMethod]
    public void TryAddService_SameInstance_ShouldReturnFalse()
    {
        TestHostProcessLifetimeHandler instance = new();
        Assert.IsTrue(_serviceProvider.TryAddService(instance));
        Assert.IsFalse(_serviceProvider.TryAddService(instance));
    }

    [TestMethod]
    public void GetServicesInternal_ExtensionMethod_InternalExtension_ShouldReturn()
    {
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());

#pragma warning disable CS0618 // Type or member is obsolete
        Assert.AreEqual(2, _serviceProvider.GetServicesInternal<ITestApplicationLifecycleCallbacks>().Count());
#pragma warning restore CS0618 // Type or member is obsolete
        Assert.AreEqual(2, _serviceProvider.GetServicesInternal<ITestHostApplicationLifetime>().Count());
    }

    [TestMethod]
    public void GetServicesInternal_InternalExtension_ShouldNotReturn()
    {
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());

#pragma warning disable CS0618 // Type or member is obsolete
        Assert.AreEqual(0, _serviceProvider.GetServicesInternal(typeof(ITestApplicationLifecycleCallbacks), stopAtFirst: false, skipInternalOnlyExtensions: true).Count());
#pragma warning restore CS0618 // Type or member is obsolete
        Assert.AreEqual(0, _serviceProvider.GetServicesInternal(typeof(ITestHostApplicationLifetime), stopAtFirst: false, skipInternalOnlyExtensions: true).Count());
    }

    [TestMethod]
    public void GetServicesInternal_InternalExtension_ShouldReturn()
    {
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());

#pragma warning disable CS0618 // Type or member is obsolete
        Assert.AreEqual(2, _serviceProvider.GetServicesInternal(typeof(ITestApplicationLifecycleCallbacks), stopAtFirst: false, skipInternalOnlyExtensions: false).Count());
#pragma warning restore CS0618 // Type or member is obsolete
        Assert.AreEqual(2, _serviceProvider.GetServicesInternal(typeof(ITestHostApplicationLifetime), stopAtFirst: false, skipInternalOnlyExtensions: false).Count());
    }

    [TestMethod]
    public void GetServicesInternal_InternalExtension_FirstOnly_ShouldReturnOne()
    {
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());

#pragma warning disable CS0618 // Type or member is obsolete
        Assert.AreEqual(1, _serviceProvider.GetServicesInternal(typeof(ITestApplicationLifecycleCallbacks), stopAtFirst: true, skipInternalOnlyExtensions: false).Count());
#pragma warning restore CS0618 // Type or member is obsolete
        Assert.AreEqual(1, _serviceProvider.GetServicesInternal(typeof(ITestHostApplicationLifetime), stopAtFirst: true, skipInternalOnlyExtensions: false).Count());
    }

    [TestMethod]
    public void GetServiceInternal_InternalExtension_ShouldReturnOne()
    {
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());

#pragma warning disable CS0618 // Type or member is obsolete
        Assert.IsNotNull(_serviceProvider.GetServiceInternal(typeof(ITestApplicationLifecycleCallbacks), skipInternalOnlyExtensions: false));
#pragma warning restore CS0618 // Type or member is obsolete
        Assert.IsNotNull(_serviceProvider.GetServiceInternal(typeof(ITestHostApplicationLifetime), skipInternalOnlyExtensions: false));
    }

    [TestMethod]
    public void GetServiceInternal_InternalExtension_SkipInternalOnlyExtensions_ShouldReturnNull()
    {
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());
        _serviceProvider.AddService(new TestApplicationLifecycleCallbacks());

#pragma warning disable CS0618 // Type or member is obsolete
        Assert.IsNull(_serviceProvider.GetServiceInternal(typeof(ITestApplicationLifecycleCallbacks), skipInternalOnlyExtensions: true));
#pragma warning restore CS0618 // Type or member is obsolete
        Assert.IsNull(_serviceProvider.GetServiceInternal(typeof(ITestHostApplicationLifetime), skipInternalOnlyExtensions: true));
    }

    private sealed class TestFramework : ITestFramework
    {
        public string Uid => throw new NotImplementedException();

        public string Version => throw new NotImplementedException();

        public string DisplayName => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public ICapability[] Capabilities => throw new NotImplementedException();

        public Task<CloseTestSessionResult> CloseTestSessionAsync(CloseTestSessionContext context) => throw new NotImplementedException();

        public Task<CreateTestSessionResult> CreateTestSessionAsync(CreateTestSessionContext context) => throw new NotImplementedException();

        public Task ExecuteRequestAsync(ExecuteRequestContext context) => throw new NotImplementedException();

        public Task<bool> IsEnabledAsync() => throw new NotImplementedException();
    }

    private sealed class TestHostProcessLifetimeHandler : ITestHostProcessLifetimeHandler
    {
        public string Uid => throw new NotImplementedException();

        public string Version => throw new NotImplementedException();

        public string DisplayName => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public Task BeforeTestHostProcessStartAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<bool> IsEnabledAsync() => throw new NotImplementedException();

        public Task OnTestHostProcessExitedAsync(ITestHostProcessInformation testHostProcessInformation, CancellationToken cancellation) => throw new NotImplementedException();

        public Task OnTestHostProcessStartedAsync(ITestHostProcessInformation testHostProcessInformation, CancellationToken cancellation) => throw new NotImplementedException();
    }

    private sealed class TestHostEnvironmentVariableProvider : ITestHostEnvironmentVariableProvider
    {
        public string Uid => throw new NotImplementedException();

        public string Version => throw new NotImplementedException();

        public string DisplayName => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public Task<ValidationResult> ValidateTestHostEnvironmentVariablesAsync(IReadOnlyEnvironmentVariables environmentVariables) => throw new NotImplementedException();

        public Task<bool> IsEnabledAsync() => throw new NotImplementedException();

        public Task UpdateAsync(IEnvironmentVariables environmentVariables) => throw new NotImplementedException();
    }

    private sealed class TestSessionLifetimeHandler : ITestSessionLifetimeHandler
    {
        public string Uid => throw new NotImplementedException();

        public string Version => throw new NotImplementedException();

        public string DisplayName => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public Task<bool> IsEnabledAsync() => throw new NotImplementedException();

        public Task OnTestSessionFinishingAsync(SessionUid sessionUid, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task OnTestSessionStartingAsync(SessionUid sessionUid, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    private sealed class DataConsumer : IDataConsumer
    {
        public Type[] DataTypesConsumed => throw new NotImplementedException();

        public string Uid => throw new NotImplementedException();

        public string Version => throw new NotImplementedException();

        public string DisplayName => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<bool> IsEnabledAsync() => throw new NotImplementedException();
    }

    private sealed class TestApplicationLifecycleCallbacks : ITestHostApplicationLifetime
    {
        public string Uid => throw new NotImplementedException();

        public string Version => throw new NotImplementedException();

        public string DisplayName => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public Task AfterRunAsync(int exitCode, CancellationToken cancellation) => throw new NotImplementedException();

        public Task BeforeRunAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<bool> IsEnabledAsync() => throw new NotImplementedException();
    }
}
