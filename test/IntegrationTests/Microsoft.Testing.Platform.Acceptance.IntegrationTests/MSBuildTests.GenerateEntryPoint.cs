﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SL = Microsoft.Build.Logging.StructuredLogger;

namespace Microsoft.Testing.Platform.Acceptance.IntegrationTests;

[TestClass]
public class MSBuildTests_EntryPoint : AcceptanceTestBase<NopAssetFixture>
{
    private const string AssetName = "MSBuildTests";

    [DynamicData(nameof(GetBuildMatrixTfmBuildVerbConfiguration), typeof(AcceptanceTestBase<NopAssetFixture>))]
    [TestMethod]
    public async Task When_GenerateTestingPlatformEntryPoint_IsFalse_NoEntryPointInjected(string tfm, BuildConfiguration compilationMode, Verb verb)
    {
        using TestAsset testAsset = await TestAsset.GenerateAssetAsync(
            nameof(GenerateCSharpEntryPointAndVerifyTheCacheUsage),
            CSharpSourceCode
            .PatchCodeWithReplace("$TargetFrameworks$", tfm)
            .PatchCodeWithReplace("$MicrosoftTestingPlatformVersion$", MicrosoftTestingPlatformVersion));
        DotnetMuxerResult compilationResult = await DotnetCli.RunAsync($"restore -r {RID} {testAsset.TargetAssetPath}{Path.DirectorySeparatorChar}MSBuildTests.csproj ", AcceptanceFixture.NuGetGlobalPackagesFolder.Path);
        string binlogFile = Path.Combine(testAsset.TargetAssetPath, Guid.NewGuid().ToString("N"), "msbuild.binlog");
        compilationResult = await DotnetCli.RunAsync($"{(verb == Verb.publish ? $"publish -f {tfm}" : "build")}  -c {compilationMode} -r {RID} -nodeReuse:false -p:GenerateTestingPlatformEntryPoint=False -bl:{binlogFile} {testAsset.TargetAssetPath} -v:n", AcceptanceFixture.NuGetGlobalPackagesFolder.Path, failIfReturnValueIsNotZero: false);
        SL.Build binLog = SL.Serialization.Read(binlogFile);
        SL.Target generateTestingPlatformEntryPoint = binLog.FindChildrenRecursive<SL.Target>().Single(t => t.Name == "_GenerateTestingPlatformEntryPoint");
        Assert.AreEqual("Target \"_GenerateTestingPlatformEntryPoint\" skipped, due to false condition; ( '$(GenerateTestingPlatformEntryPoint)' == 'True' ) was evaluated as ( 'False' == 'True' ).", ((SL.Message)generateTestingPlatformEntryPoint.Children[0]).Text);
        SL.Target includeGenerateTestingPlatformEntryPointIntoCompilation = binLog.FindChildrenRecursive<SL.Target>().Single(t => t.Name == "_IncludeGenerateTestingPlatformEntryPointIntoCompilation");
        Assert.IsEmpty(includeGenerateTestingPlatformEntryPointIntoCompilation.Children);
        Assert.AreNotEqual(0, compilationResult.ExitCode);
    }

    [DynamicData(nameof(GetBuildMatrixTfmBuildVerbConfiguration), typeof(AcceptanceTestBase<NopAssetFixture>))]
    [TestMethod]
    public async Task GenerateCSharpEntryPointAndVerifyTheCacheUsage(string tfm, BuildConfiguration compilationMode, Verb verb)
        => await GenerateAndVerifyLanguageSpecificEntryPointAsync(nameof(GenerateCSharpEntryPointAndVerifyTheCacheUsage), CSharpSourceCode, "cs", tfm, compilationMode, verb,
            @"Entrypoint source:
'//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Microsoft.Testing.Platform.MSBuild
// </auto-generated>
//------------------------------------------------------------------------------

namespace MSBuildTests
{
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal sealed class TestingPlatformEntryPoint
    {
        public static async global::System.Threading.Tasks.Task<int> Main(string[] args)
        {
            global::Microsoft.Testing.Platform.Builder.ITestApplicationBuilder builder = await global::Microsoft.Testing.Platform.Builder.TestApplication.CreateBuilderAsync(args);
            SelfRegisteredExtensions.AddSelfRegisteredExtensions(builder, args);
            using (global::Microsoft.Testing.Platform.Builder.ITestApplication app = await builder.BuildAsync())
            {
                return await app.RunAsync();
            }
        }
    }
}'", "Csc");

    [DynamicData(nameof(GetBuildMatrixTfmBuildVerbConfiguration), typeof(AcceptanceTestBase<NopAssetFixture>))]
    [TestMethod]
    public async Task GenerateVBEntryPointAndVerifyTheCacheUsage(string tfm, BuildConfiguration compilationMode, Verb verb)
        => await GenerateAndVerifyLanguageSpecificEntryPointAsync(nameof(GenerateVBEntryPointAndVerifyTheCacheUsage), VBSourceCode, "vb", tfm, compilationMode, verb,
            @"Entrypoint source:
''------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by Microsoft.Testing.Platform.MSBuild
' </auto-generated>
'------------------------------------------------------------------------------

Namespace MSBuildTests
    <System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>
    Module TestingPlatformEntryPoint

        Function Main(args As String()) As Integer
            Return MainAsync(args).GetAwaiter().GetResult()
        End Function

        Public Async Function MainAsync(args As String()) As Global.System.Threading.Tasks.Task(Of Integer)
            Dim builder = Await Global.Microsoft.Testing.Platform.Builder.TestApplication.CreateBuilderAsync(args)
            SelfRegisteredExtensions.AddSelfRegisteredExtensions(builder, args)
            Using testApplication = Await builder.BuildAsync()
                Return Await testApplication.RunAsync()
            End Using
        End Function

    End Module
End Namespace'", "Vbc");

    [DynamicData(nameof(GetBuildMatrixTfmBuildVerbConfiguration), typeof(AcceptanceTestBase<NopAssetFixture>))]
    [TestMethod]
    public async Task GenerateFSharpEntryPointAndVerifyTheCacheUsage(string tfm, BuildConfiguration compilationMode, Verb verb)
        => await GenerateAndVerifyLanguageSpecificEntryPointAsync(nameof(GenerateFSharpEntryPointAndVerifyTheCacheUsage), FSharpSourceCode, "fs", tfm, compilationMode, verb,
            @"Entrypoint source:
'//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Microsoft.Testing.Platform.MSBuild
// </auto-generated>
//------------------------------------------------------------------------------

namespace MSBuildTests

module TestingPlatformEntryPoint =

    [<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
    [<EntryPoint>]
    let main args =
        task {
            let! builder = Microsoft.Testing.Platform.Builder.TestApplication.CreateBuilderAsync args
            SelfRegisteredExtensions.AddSelfRegisteredExtensions(builder, args)
            use! app = builder.BuildAsync()
            return! app.RunAsync()
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously'", "Fsc");

    private async Task GenerateAndVerifyLanguageSpecificEntryPointAsync(string assetName, string sourceCode, string languageFileExtension, string tfm,
        BuildConfiguration compilationMode, Verb verb, string expectedEntryPoint, string cscProcessName)
    {
        string finalSourceCode = sourceCode
            .PatchCodeWithReplace("$TargetFrameworks$", tfm)
            .PatchCodeWithReplace("$MicrosoftTestingPlatformVersion$", MicrosoftTestingPlatformVersion);
        using TestAsset testAsset = await TestAsset.GenerateAssetAsync(assetName, finalSourceCode);
        await DotnetCli.RunAsync($"restore -r {RID} {testAsset.TargetAssetPath}{Path.DirectorySeparatorChar}MSBuildTests.{languageFileExtension}proj", AcceptanceFixture.NuGetGlobalPackagesFolder.Path);
        string binlogFile = Path.Combine(testAsset.TargetAssetPath, Guid.NewGuid().ToString("N"), "msbuild.binlog");
        DotnetMuxerResult buildResult = await DotnetCli.RunAsync($"{(verb == Verb.publish ? $"publish -f {tfm}" : "build")}  -c {compilationMode} -r {RID} -nodeReuse:false -bl:{binlogFile} {testAsset.TargetAssetPath} -v:n", AcceptanceFixture.NuGetGlobalPackagesFolder.Path);
        SL.Build binLog = SL.Serialization.Read(binlogFile);
        SL.Target generateTestingPlatformEntryPoint = binLog.FindChildrenRecursive<SL.Target>().Single(t => t.Name == "_GenerateTestingPlatformEntryPoint");
        SL.Task testingPlatformEntryPoint = generateTestingPlatformEntryPoint.FindChildrenRecursive<SL.Task>().Single(t => t.Name == "TestingPlatformEntryPointTask");
        SL.Message generatedSource = testingPlatformEntryPoint.FindChildrenRecursive<SL.Message>().Single(m => m.Text.Contains("Entrypoint source:"));
        Assert.AreEqual(expectedEntryPoint.ReplaceLineEndings(), generatedSource.Text.ReplaceLineEndings());

        var testHost = TestInfrastructure.TestHost.LocateFrom(testAsset.TargetAssetPath, AssetName, tfm, rid: RID, verb: verb, buildConfiguration: compilationMode);
        TestHostResult testHostResult = await testHost.ExecuteAsync();
        testHostResult.AssertExitCodeIs(ExitCodes.Success);
        Assert.IsTrue(testHostResult.StandardOutput.Contains("Passed!"));

        SL.Target coreCompile = binLog.FindChildrenRecursive<SL.Target>().Single(t => t.Name == "CoreCompile" && t.Children.Count > 0);
        SL.Task csc = coreCompile.FindChildrenRecursive<SL.Task>(t => t.Name == cscProcessName).Single();
        SL.Parameter sources = csc.FindChildrenRecursive<SL.Parameter>(t => t.Name == "Sources").Single();
        string? sourceFilePathInObj = sources.FindChildrenRecursive<SL.Item>(i => i.Text.EndsWith($"TestPlatformEntryPoint.{languageFileExtension}", StringComparison.OrdinalIgnoreCase)).SingleOrDefault()?.Text;
        Assert.IsNotNull(sourceFilePathInObj);

        File.Delete(binlogFile);
        await DotnetCli.RunAsync($"{(verb == Verb.publish ? $"publish -f {tfm}" : "build")}  -c {compilationMode} -r {RID} -nodeReuse:false -bl:{binlogFile} {testAsset.TargetAssetPath} -v:n", AcceptanceFixture.NuGetGlobalPackagesFolder.Path);
        binLog = SL.Serialization.Read(binlogFile);
        generateTestingPlatformEntryPoint = binLog.FindChildrenRecursive<SL.Target>(t => t.Name == "_GenerateTestingPlatformEntryPoint" && t.Children.Count > 0).Single();
        Assert.IsNotNull(generateTestingPlatformEntryPoint.FindChildrenRecursive<SL.Message>(m => m.Text.Contains("Skipping target \"_GenerateTestingPlatformEntryPoint\" because all output files are up-to-date with respect to the input files.", StringComparison.OrdinalIgnoreCase)).Single());

        testHost = TestInfrastructure.TestHost.LocateFrom(testAsset.TargetAssetPath, AssetName, tfm, rid: RID, verb: verb, buildConfiguration: compilationMode);
        testHostResult = await testHost.ExecuteAsync();
        Assert.AreEqual(ExitCodes.Success, testHostResult.ExitCode);
        Assert.IsTrue(testHostResult.StandardOutput.Contains("Passed!"));
    }

    private const string CSharpSourceCode = """
#file MSBuildTests.csproj
<Project Sdk="Microsoft.NET.Sdk">

    <ItemGroup>
      <TestingPlatformBuilderHook Include="A" >
        <DisplayName>DummyTestFramework</DisplayName>
        <TypeFullName>MyNamespaceRoot.Level1.Level2.DummyTestFrameworkRegistration</TypeFullName>
      </TestingPlatformBuilderHook>
    </ItemGroup>

    <ItemGroup>
      <TestingPlatformBuilderHook Include="B" >
        <DisplayName>DummyTestFramework2</DisplayName>
        <TypeFullName>MyNamespaceRoot.Level1.Level2.DummyTestFrameworkRegistration2</TypeFullName>
      </TestingPlatformBuilderHook>
    </ItemGroup>

    <PropertyGroup>
        <TargetFramework>$TargetFrameworks$</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <LangVersion>preview</LangVersion>
        <OutputType>Exe</OutputType>
        <NoWarn>$(NoWarn);NETSDK1201</NoWarn>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.Testing.Platform.MSBuild" Version="$MicrosoftTestingPlatformVersion$" />
    </ItemGroup>
</Project>

#file Program.cs
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities;
using Microsoft.Testing.Platform.Capabilities.TestFramework;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.Testing.Platform.Requests;

namespace MyNamespaceRoot.Level1.Level2;

public static class DummyTestFrameworkRegistration
{
    public static void AddExtensions(ITestApplicationBuilder testApplicationBuilder, string[] args)
    {
        testApplicationBuilder.RegisterTestFramework(_ => new Capabilities(), (_, __) => new DummyTestFramework());
    }
}

public static class DummyTestFrameworkRegistration2
{
    public static void AddExtensions(ITestApplicationBuilder testApplicationBuilder, string[] args)
    {

    }
}

internal sealed class DummyTestFramework : ITestFramework, IDataProducer
{
    public string Uid => nameof(DummyTestFramework);

    public string Version => string.Empty;

    public string DisplayName => string.Empty;

    public string Description => string.Empty;

    public Type[] DataTypesProduced => new[] { typeof(TestNodeUpdateMessage) };

    public Task<CloseTestSessionResult> CloseTestSessionAsync(CloseTestSessionContext context) => Task.FromResult(new CloseTestSessionResult() { IsSuccess = true });

    public Task<CreateTestSessionResult> CreateTestSessionAsync(CreateTestSessionContext context) => Task.FromResult(new CreateTestSessionResult() { IsSuccess = true });

    public async Task ExecuteRequestAsync(ExecuteRequestContext context)
    {
        await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(context.Request.Session.SessionUid,
            new TestNode() { Uid = "1", DisplayName = "DummyTest", Properties = new(PassedTestNodeStateProperty.CachedInstance) }));
        context.Complete();
    }

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);
}

internal sealed class Capabilities : ITestFrameworkCapabilities
{
    IReadOnlyCollection<ITestFrameworkCapability> ICapabilities<ITestFrameworkCapability>.Capabilities => Array.Empty<ITestFrameworkCapability>();
}
""";

    private const string VBSourceCode = """
#file MSBuildTests.vbproj
<Project Sdk="Microsoft.NET.Sdk">

    <ItemGroup>
      <TestingPlatformBuilderHook Include="A" >
        <DisplayName>DummyTestFramework</DisplayName>
        <TypeFullName>MyNamespaceRoot.Level1.Level2.DummyTestFrameworkRegistration</TypeFullName>
      </TestingPlatformBuilderHook>
    </ItemGroup>

    <ItemGroup>
      <TestingPlatformBuilderHook Include="B" >
        <DisplayName>DummyTestFramework2</DisplayName>
        <TypeFullName>MyNamespaceRoot.Level1.Level2.DummyTestFrameworkRegistration2</TypeFullName>
      </TestingPlatformBuilderHook>
    </ItemGroup>

    <PropertyGroup>
        <TargetFramework>$TargetFrameworks$</TargetFramework>
        <OutputType>Exe</OutputType>
        <NoWarn>$(NoWarn);NETSDK1201</NoWarn>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.Testing.Platform.MSBuild" Version="$MicrosoftTestingPlatformVersion$" />
    </ItemGroup>
</Project>

#file Program.vb
Imports Microsoft.Testing.Platform.Builder
Imports Microsoft.Testing.Platform.Capabilities
Imports Microsoft.Testing.Platform.Capabilities.TestFramework
Imports Microsoft.Testing.Platform.Extensions.Messages
Imports Microsoft.Testing.Platform.Extensions.TestFramework
Imports Microsoft.Testing.Platform.Requests
Imports Microsoft.Testing.Platform.Extensions
Imports Microsoft.Testing.Platform

Namespace MyNamespaceRoot.Level1.Level2
  Public Module DummyTestFrameworkRegistration
    Public Sub AddExtensions(builder As ITestApplicationBuilder, args As String())
      builder.RegisterTestFramework(Function() New Capabilities(), Function(cap, services) New DummyTestFramework())
    End Sub
  End Module

  Public Module DummyTestFrameworkRegistration2
    Public Sub AddExtensions(builder As ITestApplicationBuilder, args As String())
    End Sub
  End Module

  Class DummyTestFramework
    Implements ITestFramework
    Implements IDataProducer

    Public ReadOnly Property Uid As String Implements IExtension.Uid
      Get
        Return String.Empty
      End Get
    End Property

    Public ReadOnly Property Version As String Implements IExtension.Version
      Get
        Return String.Empty
      End Get
    End Property

    Public ReadOnly Property DisplayName As String Implements IExtension.DisplayName
      Get
        Return String.Empty
      End Get
    End Property

    Public ReadOnly Property Description As String Implements IExtension.Description
      Get
        Return String.Empty
      End Get
    End Property

    Public ReadOnly Property DataTypesProduced As Type() Implements Messages.IDataProducer.DataTypesProduced
      Get
        Dim types(1) As Type
        types(0) = GetType(Messages.TestNodeUpdateMessage)
        Return types
      End Get
    End Property

    Public Function CreateTestSessionAsync(context As TestFramework.CreateTestSessionContext) As Task(Of TestFramework.CreateTestSessionResult) Implements TestFramework.ITestFramework.CreateTestSessionAsync
      Dim ctx As New TestFramework.CreateTestSessionResult()
      ctx.IsSuccess = True
      Return Task.FromResult(ctx)
    End Function

    Public Async Function ExecuteRequestAsync(context As TestFramework.ExecuteRequestContext) As Task Implements TestFramework.ITestFramework.ExecuteRequestAsync
      Await context.MessageBus.PublishAsync(Me, New Messages.TestNodeUpdateMessage(context.Request.Session.SessionUid,
                                                                          New Messages.TestNode With {
                                                                          .Uid = "1",
                                                                          .DisplayName = "DummyTest",
                                                                          .Properties = New Messages.PropertyBag(Messages.PassedTestNodeStateProperty.CachedInstance)}))
      context.Complete()
    End Function

    Public Function CloseTestSessionAsync(context As TestFramework.CloseTestSessionContext) As Task(Of TestFramework.CloseTestSessionResult) Implements TestFramework.ITestFramework.CloseTestSessionAsync
      Dim ctx As New TestFramework.CloseTestSessionResult()
      ctx.IsSuccess = True
      Return Task.FromResult(ctx)
    End Function

    Public Function IsEnabledAsync() As Task(Of Boolean) Implements IExtension.IsEnabledAsync
      Return Task(Of Boolean).FromResult(True)
    End Function
  End Class

  Class Capabilities
    Implements ITestFrameworkCapabilities

    Private ReadOnly Property ICapabilities_Capabilities As IReadOnlyCollection(Of ITestFrameworkCapability) Implements ICapabilities(Of ITestFrameworkCapability).Capabilities
      Get
        Return Array.Empty(Of ITestFrameworkCapability)()
      End Get
    End Property
  End Class
End Namespace
""";

    private const string FSharpSourceCode = """
#file MSBuildTests.fsproj
<Project Sdk="Microsoft.NET.Sdk">

    <ItemGroup>
      <TestingPlatformBuilderHook Include="A" >
        <DisplayName>DummyTestFramework</DisplayName>
        <TypeFullName>MyNamespaceRoot.Level1.Level2.DummyTestFrameworkRegistration</TypeFullName>
      </TestingPlatformBuilderHook>
    </ItemGroup>

    <ItemGroup>
      <TestingPlatformBuilderHook Include="B" >
        <DisplayName>DummyTestFramework2</DisplayName>
        <TypeFullName>MyNamespaceRoot.Level1.Level2.DummyTestFrameworkRegistration2</TypeFullName>
      </TestingPlatformBuilderHook>
    </ItemGroup>

    <PropertyGroup>
        <TargetFramework>$TargetFrameworks$</TargetFramework>
        <LangVersion>preview</LangVersion>
        <OutputType>Exe</OutputType>
        <NoWarn>$(NoWarn);NETSDK1201</NoWarn>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.Testing.Platform.MSBuild" Version="$MicrosoftTestingPlatformVersion$" />
        <PackageReference Include="Microsoft.Testing.Platform" Version="$MicrosoftTestingPlatformVersion$" />
    </ItemGroup>

    <ItemGroup>
        <Compile Include="Program.fs" />
    </ItemGroup>
</Project>

#file Program.fs
namespace MyNamespaceRoot.Level1.Level2

open Microsoft.Testing.Platform.Builder
open Microsoft.Testing.Platform.Capabilities
open Microsoft.Testing.Platform.Capabilities.TestFramework
open Microsoft.Testing.Platform.Extensions.Messages
open Microsoft.Testing.Platform.Extensions.TestFramework
open Microsoft.Testing.Platform.Requests
open System.Threading.Tasks

type Capabilities () =
    interface ITestFrameworkCapabilities with
        member _.Capabilities = [||]

type DummyTestFramework() =
    let dataProducer = {
        new IDataProducer with
            member _.DataTypesProduced = [| typedefof<TestNodeUpdateMessage> |]
            member _.Uid = nameof(DummyTestFramework)
            member _.Version = ""
            member _.DisplayName = ""
            member _.Description = ""

            member _.IsEnabledAsync() = Task.FromResult true
        }

    interface ITestFramework with
        member _.Uid = nameof(DummyTestFramework)
        member _.Version = ""
        member _.DisplayName = ""
        member _.Description = ""

        member _.IsEnabledAsync() = Task.FromResult true
        member _.CreateTestSessionAsync _ = CreateTestSessionResult(IsSuccess = true) |> Task.FromResult
        member _.CloseTestSessionAsync _ = CloseTestSessionResult(IsSuccess = true) |> Task.FromResult
        member _.ExecuteRequestAsync context = task {
            do! context.MessageBus.PublishAsync(
                dataProducer,
                TestNodeUpdateMessage(
                    context.Request.Session.SessionUid,
                    TestNode(Uid = "1", DisplayName = "DummyTest", Properties = PropertyBag(PassedTestNodeStateProperty.CachedInstance))))
            context.Complete()
            }

module DummyTestFrameworkRegistration =
    let AddExtensions (testApplicationBuilder : ITestApplicationBuilder, args: string[]) =
        testApplicationBuilder.RegisterTestFramework((fun _ -> Capabilities()), (fun _ _ -> DummyTestFramework())) |> ignore

module DummyTestFrameworkRegistration2 =
    let AddExtensions (_, _) = ()

""";
}
