﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Acceptance.IntegrationTests;
using Microsoft.Testing.Platform.Acceptance.IntegrationTests.Helpers;

namespace MSTest.Acceptance.IntegrationTests;

[TestClass]
public sealed class ThreadingTests : AcceptanceTestBase<ThreadingTests.TestAssetFixture>
{
    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task TestMethodThreading_WhenMainIsNotSTA_NoRunsettingsProvided_ThreadIsNotSTA(string tfm)
    {
        var testHost = TestHost.LocateFrom(AssetFixture.ProjectPath, TestAssetFixture.ProjectName, tfm);
        TestHostResult testHostResult = await testHost.ExecuteAsync();

        testHostResult.AssertExitCodeIs(0);
        testHostResult.AssertOutputContains("Passed!");
    }

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task TestMethodThreading_WhenMainIsNotSTA_RunsettingsAsksForSTA_OnWindows_ThreadIsSTA(string tfm)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var testHost = TestHost.LocateFrom(AssetFixture.ProjectPath, TestAssetFixture.ProjectName, tfm);
        string runSettingsFilePath = Path.Combine(testHost.DirectoryName, "sta.runsettings");
        TestHostResult testHostResult = await testHost.ExecuteAsync($"--settings {runSettingsFilePath}", environmentVariables: new()
        {
            ["MSTEST_THREAD_STATE_IS_STA"] = "1",
        });

        testHostResult.AssertExitCodeIs(0);
        testHostResult.AssertOutputContains("Passed!");
    }

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task TestMethodThreading_WhenMainIsNotSTA_RunsettingsAsksForSTA_OnNonWindows_ThreadIsNotSTAAndWarningIsEmitted(string tfm)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var testHost = TestHost.LocateFrom(AssetFixture.ProjectPath, TestAssetFixture.ProjectName, tfm);
        string runSettingsFilePath = Path.Combine(testHost.DirectoryName, "sta.runsettings");
        TestHostResult testHostResult = await testHost.ExecuteAsync($"--settings {runSettingsFilePath}", environmentVariables: new()
        {
            ["MSTEST_THREAD_STATE_IS_STA"] = "0",
        });

        testHostResult.AssertExitCodeIs(0);
        testHostResult.AssertOutputContains("Passed!");
        testHostResult.AssertOutputContains("Runsettings entry '<ExecutionApartmentState>STA</ExecutionApartmentState>' is not supported on non-Windows OSes");
    }

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task TestMethodThreading_WhenMainIsNotSTA_RunsettingsAsksForMTA_ThreadIsNotSTA(string tfm)
    {
        var testHost = TestHost.LocateFrom(AssetFixture.ProjectPath, TestAssetFixture.ProjectName, tfm);
        string runSettingsFilePath = Path.Combine(testHost.DirectoryName, "mta.runsettings");
        TestHostResult testHostResult = await testHost.ExecuteAsync($"--settings {runSettingsFilePath}", environmentVariables: new()
        {
            ["MSTEST_THREAD_STATE_IS_STA"] = "0",
        });

        testHostResult.AssertExitCodeIs(0);
        testHostResult.AssertOutputContains("Passed!");
        testHostResult.AssertOutputDoesNotContain("Runsettings entry '<ExecutionApartmentState>STA</ExecutionApartmentState>' is not supported on non-Windows OSes");
    }

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task TestMethodThreading_MainIsSTAThread_OnWindows_NoRunsettingsProvided_ThreadIsSTA(string tfm)
    {
        // Test cannot work on non-Windows OSes as the main method is marked with [STAThread]
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var testHost = TestHost.LocateFrom(AssetFixture.STAThreadProjectPath, TestAssetFixture.STAThreadProjectName, tfm);
        TestHostResult testHostResult = await testHost.ExecuteAsync(environmentVariables: new()
        {
            ["MSTEST_THREAD_STATE_IS_STA"] = "1",
        });

        testHostResult.AssertExitCodeIs(0);
        testHostResult.AssertOutputContains("Passed!");
    }

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task TestMethodThreading_MainIsSTAThread_OnWindows_RunsettingsAsksForSTA_ThreadIsSTA(string tfm)
    {
        // Test cannot work on non-Windows OSes as the main method is marked with [STAThread]
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var testHost = TestHost.LocateFrom(AssetFixture.STAThreadProjectPath, TestAssetFixture.STAThreadProjectName, tfm);
        string runSettingsFilePath = Path.Combine(testHost.DirectoryName, "sta.runsettings");
        TestHostResult testHostResult = await testHost.ExecuteAsync($"--settings {runSettingsFilePath}", environmentVariables: new()
        {
            ["MSTEST_THREAD_STATE_IS_STA"] = "1",
        });

        testHostResult.AssertExitCodeIs(0);
        testHostResult.AssertOutputContains("Passed!");
    }

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task TestMethodThreading_MainIsSTAThread_OnWindows_RunsettingsAsksForMTA_ThreadIsMTA(string tfm)
    {
        // Test cannot work on non-Windows OSes as the main method is marked with [STAThread]
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var testHost = TestHost.LocateFrom(AssetFixture.STAThreadProjectPath, TestAssetFixture.STAThreadProjectName, tfm);
        string runSettingsFilePath = Path.Combine(testHost.DirectoryName, "mta.runsettings");
        TestHostResult testHostResult = await testHost.ExecuteAsync($"--settings {runSettingsFilePath}", environmentVariables: new()
        {
            ["MSTEST_THREAD_STATE_IS_STA"] = "0",
        });

        testHostResult.AssertExitCodeIs(0);
        testHostResult.AssertOutputContains("Passed!");
    }

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task LifecycleAttributesVoidThreading_WhenMainIsNotSTA_RunsettingsAsksForSTA_OnWindows_ThreadIsSTA(string tfm)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var testHost = TestHost.LocateFrom(AssetFixture.LifecycleAttributesVoidProjectPath, TestAssetFixture.LifecycleAttributesVoidProjectName, tfm);
        string runSettingsFilePath = Path.Combine(testHost.DirectoryName, "sta.runsettings");
        TestHostResult testHostResult = await testHost.ExecuteAsync($"--settings {runSettingsFilePath}", environmentVariables: new()
        {
            ["MSTEST_THREAD_STATE_IS_STA"] = "1",
        });

        testHostResult.AssertExitCodeIs(0);
        testHostResult.AssertOutputContains("Passed!");
    }

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task LifecycleAttributesTaskThreading_WhenMainIsNotSTA_RunsettingsAsksForSTA_OnWindows_ThreadIsSTA(string tfm)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var testHost = TestHost.LocateFrom(AssetFixture.LifecycleAttributesTaskProjectPath, TestAssetFixture.LifecycleAttributesTaskProjectName, tfm);
        string runSettingsFilePath = Path.Combine(testHost.DirectoryName, "sta.runsettings");
        TestHostResult testHostResult = await testHost.ExecuteAsync($"--settings {runSettingsFilePath}", environmentVariables: new()
        {
            ["MSTEST_THREAD_STATE_IS_STA"] = "1",
        });

        testHostResult.AssertExitCodeIs(0);
        testHostResult.AssertOutputContains("Passed!");
    }

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task LifecycleAttributesTaskThreading_WhenMainIsNotSTA_RunsettingsAsksForSTA_OnWindows_ThreadIsSTA_With_ParallelAttribute(string tfm)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var testHost = TestHost.LocateFrom(AssetFixture.LifecycleWithParallelAttributesTaskProjectNamePath, TestAssetFixture.LifecycleAttributesTaskProjectName, tfm);
        string runSettingsFilePath = Path.Combine(testHost.DirectoryName, "sta.runsettings");
        TestHostResult testHostResult = await testHost.ExecuteAsync($"--settings {runSettingsFilePath}", environmentVariables: new()
        {
            ["MSTEST_THREAD_STATE_IS_STA"] = "1",
        });

        testHostResult.AssertExitCodeIs(0);
        testHostResult.AssertOutputContains("Passed!");
    }

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.NetForDynamicData), typeof(TargetFrameworks))]
    public async Task LifecycleAttributesValueTaskThreading_WhenMainIsNotSTA_RunsettingsAsksForSTA_OnWindows_ThreadIsSTA(string tfm)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var testHost = TestHost.LocateFrom(AssetFixture.LifecycleAttributesValueTaskProjectPath, TestAssetFixture.LifecycleAttributesValueTaskProjectName, tfm);
        string runSettingsFilePath = Path.Combine(testHost.DirectoryName, "sta.runsettings");
        TestHostResult testHostResult = await testHost.ExecuteAsync($"--settings {runSettingsFilePath}", environmentVariables: new()
        {
            ["MSTEST_THREAD_STATE_IS_STA"] = "1",
        });

        testHostResult.AssertExitCodeIs(0);
        testHostResult.AssertOutputContains("Passed!");
    }

    public sealed class TestAssetFixture() : TestAssetFixtureBase(AcceptanceFixture.NuGetGlobalPackagesFolder)
    {
        public const string ProjectName = "TestThreading";
        public const string STAThreadProjectName = "STATestThreading";
        public const string LifecycleAttributesVoidProjectName = "LifecycleAttributesVoid";
        public const string LifecycleAttributesTaskProjectName = "LifecycleAttributesTask";
        public const string LifecycleWithParallelAttributesTaskProjectName = "LifecycleWithParallelAttributesTask";
        public const string LifecycleAttributesValueTaskProjectName = "LifecycleAttributesValueTask";

        public string ProjectPath => GetAssetPath(ProjectName);

        public string STAThreadProjectPath => GetAssetPath(STAThreadProjectName);

        public string LifecycleAttributesVoidProjectPath => GetAssetPath(LifecycleAttributesVoidProjectName);

        public string LifecycleAttributesTaskProjectPath => GetAssetPath(LifecycleAttributesTaskProjectName);

        public string LifecycleWithParallelAttributesTaskProjectNamePath => GetAssetPath(LifecycleWithParallelAttributesTaskProjectName);

        public string LifecycleAttributesValueTaskProjectPath => GetAssetPath(LifecycleAttributesValueTaskProjectName);

        public override IEnumerable<(string ID, string Name, string Code)> GetAssetsToGenerate()
        {
            yield return (ProjectName, ProjectName,
                SourceCode
                .PatchTargetFrameworks(TargetFrameworks.All)
                .PatchCodeWithReplace("$ProjectName$", ProjectName)
                .PatchCodeWithReplace("$GenerateEntryPoint$", "true")
                .PatchCodeWithReplace("$MSTestVersion$", MSTestVersion));

            yield return (STAThreadProjectName, STAThreadProjectName,
                (SourceCode + ProgramFileSourceCode)
                .PatchTargetFrameworks(TargetFrameworks.All)
                .PatchCodeWithReplace("$ProjectName$", STAThreadProjectName)
                .PatchCodeWithReplace("$GenerateEntryPoint$", "false")
                .PatchCodeWithReplace("$MSTestVersion$", MSTestVersion));

            yield return (LifecycleAttributesVoidProjectName, LifecycleAttributesVoidProjectName,
                LifecycleAttributesVoidSource
                .PatchTargetFrameworks(TargetFrameworks.All)
                .PatchCodeWithReplace("$MSTestVersion$", MSTestVersion));

            yield return (LifecycleAttributesTaskProjectName, LifecycleAttributesTaskProjectName,
                LifecycleAttributesTaskSource
                .PatchTargetFrameworks(TargetFrameworks.All)
                .PatchCodeWithReplace("$ParallelAttribute$", string.Empty)
                .PatchCodeWithReplace("$MSTestVersion$", MSTestVersion));

            yield return (LifecycleWithParallelAttributesTaskProjectName, LifecycleAttributesTaskProjectName,
                LifecycleAttributesTaskSource
                .PatchTargetFrameworks(TargetFrameworks.All)
                .PatchCodeWithReplace("$ParallelAttribute$", "[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]")
                .PatchCodeWithReplace("$MSTestVersion$", MSTestVersion));

            yield return (LifecycleAttributesValueTaskProjectName, LifecycleAttributesValueTaskProjectName,
                LifecycleAttributesValueTaskSource
                .PatchTargetFrameworks(TargetFrameworks.Net)
                .PatchCodeWithReplace("$MSTestVersion$", MSTestVersion));
        }

        private const string SourceCode = """
#file sta.runsettings
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
    <RunConfiguration>
        <ExecutionThreadApartmentState>STA</ExecutionThreadApartmentState>
    </RunConfiguration>
</RunSettings>

#file mta.runsettings
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
    <RunConfiguration>
        <ExecutionThreadApartmentState>MTA</ExecutionThreadApartmentState>
    </RunConfiguration>
</RunSettings>

#file $ProjectName$.csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <EnableMSTestRunner>true</EnableMSTestRunner>
    <TargetFrameworks>$TargetFrameworks$</TargetFrameworks>
    <LangVersion>latest</LangVersion>
    <GenerateTestingPlatformEntryPoint>$GenerateEntryPoint$</GenerateTestingPlatformEntryPoint>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MSTest.TestAdapter" Version="$MSTestVersion$" />
    <PackageReference Include="MSTest.TestFramework" Version="$MSTestVersion$" />
  </ItemGroup>

  <ItemGroup>
    <None Update="sta.runsettings">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="mta.runsettings">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>

#file UnitTest1.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        AssertCorrectThreadApartmentState();
    }

    [TestMethod]
    public async Task TestMethod2()
    {
        AssertCorrectThreadApartmentState();
        // Ensure that we continue on a thread pool thread after this await.
        await Task.Yield();
        Assert.IsTrue(Thread.CurrentThread.IsThreadPoolThread);
    }

    [TestMethod]
    public Task TestMethod3()
    {
        if (Environment.GetEnvironmentVariable("MSTEST_THREAD_STATE_IS_STA") == "1")
        {
            // TestMethod2 finished on a thread pool thread.
            // However, here in this method we should still start on STA thread.
            Assert.IsFalse(Thread.CurrentThread.IsThreadPoolThread);
        }

        AssertCorrectThreadApartmentState();
        return Task.CompletedTask;
    }

#if NET
    [TestMethod]
    public async ValueTask TestMethod4()
    {
        AssertCorrectThreadApartmentState();
        await ValueTask.CompletedTask;
    }

    [TestMethod]
    public ValueTask TestMethod5()
    {
        AssertCorrectThreadApartmentState();
        return ValueTask.CompletedTask;
    }
#endif

    private static void AssertCorrectThreadApartmentState()
    {
        var apartmentState = Thread.CurrentThread.GetApartmentState();
        if (Environment.GetEnvironmentVariable("MSTEST_THREAD_STATE_IS_STA") == "1")
        {
            Assert.AreEqual(ApartmentState.STA, apartmentState);
        }
        else
        {
            Assert.AreNotEqual(ApartmentState.STA, apartmentState);
        }
    }
}
""";

        private const string ProgramFileSourceCode = """
#file Program.cs
using System;
using Microsoft.Testing.Platform.Builder;

public static class Program
{
    // Async main doesn't respect [STAThread] attribute so do a version with `GetAwaiter().GetResult()`
    // See https://github.com/dotnet/roslyn/issues/22112
    [STAThread]
    public static int Main(string[] args)
    {
        ITestApplicationBuilder builder = TestApplication.CreateBuilderAsync(args).GetAwaiter().GetResult();
        Microsoft.VisualStudio.TestTools.UnitTesting.TestingPlatformBuilderHook.AddExtensions(builder, args);
        using ITestApplication app = builder.BuildAsync().GetAwaiter().GetResult();
        return app.RunAsync().GetAwaiter().GetResult();
    }
}
""";

        private const string LifecycleAttributesVoidSource = """
#file sta.runsettings
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
    <RunConfiguration>
        <ExecutionThreadApartmentState>STA</ExecutionThreadApartmentState>
    </RunConfiguration>
</RunSettings>

#file mta.runsettings
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
    <RunConfiguration>
        <ExecutionThreadApartmentState>MTA</ExecutionThreadApartmentState>
    </RunConfiguration>
</RunSettings>

#file LifecycleAttributesVoid.csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <EnableMSTestRunner>true</EnableMSTestRunner>
    <TargetFrameworks>$TargetFrameworks$</TargetFrameworks>
    <LangVersion>latest</LangVersion>
    <GenerateTestingPlatformEntryPoint>true</GenerateTestingPlatformEntryPoint>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MSTest.TestAdapter" Version="$MSTestVersion$" />
    <PackageReference Include="MSTest.TestFramework" Version="$MSTestVersion$" />
  </ItemGroup>

  <ItemGroup>
    <None Update="sta.runsettings">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="mta.runsettings">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>

#file LifecycleAttributesVoid.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class LifecycleAttributesVoidTests
{
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        AssertCorrectThreadApartmentState();
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        AssertCorrectThreadApartmentState();
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        AssertCorrectThreadApartmentState();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        AssertCorrectThreadApartmentState();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        AssertCorrectThreadApartmentState();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        AssertCorrectThreadApartmentState();
    }

    [TestMethod]
    public void TestMethod()
    {
    }

    private static void AssertCorrectThreadApartmentState()
    {
        var apartmentState = Thread.CurrentThread.GetApartmentState();
        if (Environment.GetEnvironmentVariable("MSTEST_THREAD_STATE_IS_STA") == "1")
        {
            Assert.AreEqual(ApartmentState.STA, apartmentState);
        }
        else
        {
            Assert.AreNotEqual(ApartmentState.STA, apartmentState);
        }
    }
}
""";

        private const string LifecycleAttributesTaskSource = """
#file sta.runsettings
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
    <RunConfiguration>
        <ExecutionThreadApartmentState>STA</ExecutionThreadApartmentState>
    </RunConfiguration>
</RunSettings>

#file mta.runsettings
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
    <RunConfiguration>
        <ExecutionThreadApartmentState>MTA</ExecutionThreadApartmentState>
    </RunConfiguration>
</RunSettings>

#file LifecycleAttributesTask.csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <EnableMSTestRunner>true</EnableMSTestRunner>
    <TargetFrameworks>$TargetFrameworks$</TargetFrameworks>
    <LangVersion>latest</LangVersion>
    <GenerateTestingPlatformEntryPoint>true</GenerateTestingPlatformEntryPoint>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MSTest.TestAdapter" Version="$MSTestVersion$" />
    <PackageReference Include="MSTest.TestFramework" Version="$MSTestVersion$" />
  </ItemGroup>

  <ItemGroup>
    <None Update="sta.runsettings">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="mta.runsettings">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>

#file LifecycleAttributesTask.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

$ParallelAttribute$
[TestClass]
public class LifecycleAttributesTaskTests
{
    [AssemblyInitialize]
    public static Task AssemblyInitialize(TestContext context)
    {
        AssertCorrectThreadApartmentState();
        return Task.CompletedTask;
    }

    [AssemblyCleanup]
    public static Task AssemblyCleanup()
    {
        AssertCorrectThreadApartmentState();
        return Task.CompletedTask;
    }

    [ClassInitialize]
    public static Task ClassInitialize(TestContext context)
    {
        AssertCorrectThreadApartmentState();
        return Task.CompletedTask;
    }

    [ClassCleanup]
    public static Task ClassCleanup()
    {
        AssertCorrectThreadApartmentState();
        return Task.CompletedTask;
    }

    [TestInitialize]
    public Task TestInitialize()
    {
        AssertCorrectThreadApartmentState();
        return Task.CompletedTask;
    }

    [TestCleanup]
    public Task TestCleanup()
    {
        AssertCorrectThreadApartmentState();
        return Task.CompletedTask;
    }

    [TestMethod]
    public void TestMethod()
    {
    }

    private static void AssertCorrectThreadApartmentState()
    {
        var apartmentState = Thread.CurrentThread.GetApartmentState();
        if (Environment.GetEnvironmentVariable("MSTEST_THREAD_STATE_IS_STA") == "1")
        {
            Assert.AreEqual(ApartmentState.STA, apartmentState);
        }
        else
        {
            Assert.AreNotEqual(ApartmentState.STA, apartmentState);
        }
    }
}
""";

        private const string LifecycleAttributesValueTaskSource = """
#file sta.runsettings
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
    <RunConfiguration>
        <ExecutionThreadApartmentState>STA</ExecutionThreadApartmentState>
    </RunConfiguration>
</RunSettings>

#file mta.runsettings
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
    <RunConfiguration>
        <ExecutionThreadApartmentState>MTA</ExecutionThreadApartmentState>
    </RunConfiguration>
</RunSettings>

#file LifecycleAttributesValueTask.csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <EnableMSTestRunner>true</EnableMSTestRunner>
    <TargetFrameworks>$TargetFrameworks$</TargetFrameworks>
    <LangVersion>latest</LangVersion>
    <GenerateTestingPlatformEntryPoint>true</GenerateTestingPlatformEntryPoint>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MSTest.TestAdapter" Version="$MSTestVersion$" />
    <PackageReference Include="MSTest.TestFramework" Version="$MSTestVersion$" />
  </ItemGroup>

  <ItemGroup>
    <None Update="sta.runsettings">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="mta.runsettings">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>

#file LifecycleAttributesValueTask.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class LifecycleAttributesValueTaskTests
{
    [AssemblyInitialize]
    public static ValueTask AssemblyInitialize(TestContext context)
    {
        AssertCorrectThreadApartmentState();
        return ValueTask.CompletedTask;
    }

    [AssemblyCleanup]
    public static ValueTask AssemblyCleanup()
    {
        AssertCorrectThreadApartmentState();
        return ValueTask.CompletedTask;
    }

    [ClassInitialize]
    public static ValueTask ClassInitialize(TestContext context)
    {
        AssertCorrectThreadApartmentState();
        return ValueTask.CompletedTask;
    }

    [ClassCleanup]
    public static ValueTask ClassCleanup()
    {
        AssertCorrectThreadApartmentState();
        return ValueTask.CompletedTask;
    }

    [TestInitialize]
    public ValueTask TestInitialize()
    {
        AssertCorrectThreadApartmentState();
        return ValueTask.CompletedTask;
    }

    [TestCleanup]
    public ValueTask TestCleanup()
    {
        AssertCorrectThreadApartmentState();
        return ValueTask.CompletedTask;
    }

    [TestMethod]
    public void TestMethod()
    {
    }

    private static void AssertCorrectThreadApartmentState()
    {
        var apartmentState = Thread.CurrentThread.GetApartmentState();
        if (Environment.GetEnvironmentVariable("MSTEST_THREAD_STATE_IS_STA") == "1")
        {
            Assert.AreEqual(ApartmentState.STA, apartmentState);
        }
        else
        {
            Assert.AreNotEqual(ApartmentState.STA, apartmentState);
        }
    }
}
""";
    }
}
