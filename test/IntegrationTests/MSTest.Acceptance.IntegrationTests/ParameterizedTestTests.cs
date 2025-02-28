﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Acceptance.IntegrationTests;
using Microsoft.Testing.Platform.Acceptance.IntegrationTests.Helpers;
using Microsoft.Testing.Platform.Helpers;

namespace MSTest.Acceptance.IntegrationTests;

[TestClass]
public class ParameterizedTestTests : AcceptanceTestBase<ParameterizedTestTests.TestAssetFixture>
{
    private const string DynamicDataAssetName = "DynamicData";
    private const string DataSourceAssetName = "DataSource";

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task SendingEmptyDataToDynamicDataTest_WithSettingConsiderEmptyDataSourceAsInconclusive_Passes(string currentTfm)
        => await RunTestsAsync(currentTfm, DynamicDataAssetName, true);

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task SendingEmptyDataToDataSourceTest_WithSettingConsiderEmptyDataSourceAsInconclusive_Passes(string currentTfm)
        => await RunTestsAsync(currentTfm, DataSourceAssetName, true);

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task SendingEmptyDataToDynamicDataTest_WithSettingConsiderEmptyDataSourceAsInconclusiveToFalse_Fails(string currentTfm)
        => await RunTestsAsync(currentTfm, DynamicDataAssetName, false);

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task SendingEmptyDataToDataSourceTest_WithSettingConsiderEmptyDataSourceAsInconclusiveToFalse_Fails(string currentTfm)
    => await RunTestsAsync(currentTfm, DataSourceAssetName, false);

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task SendingEmptyDataToDynamicDataTest_WithoutSettingConsiderEmptyDataSourceAsInconclusive_Fails(string currentTfm)
        => await RunTestsAsync(currentTfm, DynamicDataAssetName, null);

    [TestMethod]
    [DynamicData(nameof(TargetFrameworks.AllForDynamicData), typeof(TargetFrameworks))]
    public async Task SendingEmptyDataToDataSourceTest_WithoutSettingConsiderEmptyDataSourceAsInconclusive_Fails(string currentTfm)
        => await RunTestsAsync(currentTfm, DataSourceAssetName, null);

    private static async Task RunTestsAsync(string currentTfm, string assetName, bool? isEmptyDataInconclusive)
    {
        var testHost = TestHost.LocateFrom(AssetFixture.GetAssetPath(assetName), assetName, currentTfm);

        TestHostResult testHostResult = await testHost.ExecuteAsync(SetupRunSettingsAndGetArgs(isEmptyDataInconclusive));

        bool isSuccess = isEmptyDataInconclusive.HasValue && isEmptyDataInconclusive.Value;

        testHostResult.AssertExitCodeIs(isSuccess ? ExitCodes.Success : ExitCodes.AtLeastOneTestFailed);

        testHostResult.AssertOutputContains(isSuccess ? "skipped Test" : "failed Test");

        string? SetupRunSettingsAndGetArgs(bool? isEmptyDataInconclusive)
        {
            if (!isEmptyDataInconclusive.HasValue)
            {
                return null;
            }

            string runSettings = $"""
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
    <RunConfiguration>
    </RunConfiguration>
    <MSTest>
        <ConsiderEmptyDataSourceAsInconclusive>{isEmptyDataInconclusive}</ConsiderEmptyDataSourceAsInconclusive>
    </MSTest>
</RunSettings>
""";

            string runSettingsFilePath = Path.Combine(testHost.DirectoryName, $"{Guid.NewGuid():N}.runsettings");
            File.WriteAllText(runSettingsFilePath, runSettings);
            return $"--settings {runSettingsFilePath}";
        }
    }

    public sealed class TestAssetFixture() : TestAssetFixtureBase(AcceptanceFixture.NuGetGlobalPackagesFolder)
    {
        public override IEnumerable<(string ID, string Name, string Code)> GetAssetsToGenerate()
        {
            yield return (DynamicDataAssetName, DynamicDataAssetName,
                SourceCodeDynamicData
                .PatchTargetFrameworks(TargetFrameworks.All)
                .PatchCodeWithReplace("$MSTestVersion$", MSTestVersion));
            yield return (DataSourceAssetName, DataSourceAssetName,
                SourceCodeDataSource
                .PatchTargetFrameworks(TargetFrameworks.All)
                .PatchCodeWithReplace("$MSTestVersion$", MSTestVersion));
        }

        private const string SourceCodeDynamicData = """
#file DynamicData.csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <EnableMSTestRunner>true</EnableMSTestRunner>
    <TargetFrameworks>$TargetFrameworks$</TargetFrameworks>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MSTest.TestAdapter" Version="$MSTestVersion$" />
    <PackageReference Include="MSTest.TestFramework" Version="$MSTestVersion$" />
  </ItemGroup>

</Project>

#file UnitTest1.cs

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class TestClass
{
    [TestMethod]
    [DynamicData(nameof(AdditionalData))]
    [DynamicData(nameof(AdditionalData2))]
    public void Test(int i)
    {
    }

    public static IEnumerable<object[]> AdditionalData => Array.Empty<object[]>();

    public static IEnumerable<object[]> AdditionalData2
    {
        get
        {
            yield return new object[] { 2 };
        }
    }
}
""";

        private const string SourceCodeDataSource = """
#file DataSource.csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <EnableMSTestRunner>true</EnableMSTestRunner>
    <TargetFrameworks>$TargetFrameworks$</TargetFrameworks>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MSTest.TestAdapter" Version="$MSTestVersion$" />
    <PackageReference Include="MSTest.TestFramework" Version="$MSTestVersion$" />
  </ItemGroup>

</Project>

#file UnitTest1.cs

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class TestClass
{
    [TestMethod]
    [CustomTestDataSource]
    [CustomEmptyTestDataSource]
    public void Test(int a, int b, int c)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CustomTestDataSourceAttribute : Attribute, ITestDataSource
{
    public IEnumerable<object[]> GetData(MethodInfo methodInfo) => [[1, 2, 3], [4, 5, 6]];

    public string GetDisplayName(MethodInfo methodInfo, object[] data) => data != null ? string.Format(CultureInfo.CurrentCulture, "{0} ({1})", methodInfo.Name, string.Join(",", data)) : null;
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CustomEmptyTestDataSourceAttribute : Attribute, ITestDataSource
{
    public IEnumerable<object[]> GetData(MethodInfo methodInfo) => [];

    public string GetDisplayName(MethodInfo methodInfo, object[] data) => data != null ? string.Format(CultureInfo.CurrentCulture, "{0} ({1})", methodInfo.Name, string.Join(",", data)) : null;
}
""";
    }
}
