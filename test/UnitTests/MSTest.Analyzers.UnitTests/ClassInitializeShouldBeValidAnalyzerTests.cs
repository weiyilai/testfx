﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using VerifyCS = MSTest.Analyzers.Test.CSharpCodeFixVerifier<
    MSTest.Analyzers.ClassInitializeShouldBeValidAnalyzer,
    MSTest.Analyzers.ClassInitializeShouldBeValidFixer>;

namespace MSTest.Analyzers.Test;

[TestClass]
public sealed class ClassInitializeShouldBeValidAnalyzerTests
{
    [TestMethod]
    public async Task WhenClassInitializeIsPublic_NoDiagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsGenericWithInheritanceModeSet_NoDiagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass<T>
            {
                [ClassInitialize(inheritanceBehavior: InheritanceBehavior.BeforeEachDerivedClass)]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsGenericWithInheritanceModeSetToNone_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass<T>
            {
                [ClassInitialize(InheritanceBehavior.None)]
                public static void {|#0:ClassInitialize|}(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code,
            VerifyCS.Diagnostic().WithLocation(0).WithArguments("ClassInitialize"),
            code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsGenericWithoutSettingInheritanceMode_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass<T>
            {
                [ClassInitialize]
                public static void {|#0:ClassInitialize|}(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code,
            VerifyCS.Diagnostic().WithLocation(0).WithArguments("ClassInitialize"),
            code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsPublic_InsideInternalClassWithDiscoverInternals_NoDiagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [assembly: DiscoverInternals]

            [TestClass]
            internal class MyTestClass
            {
                [ClassInitialize]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsInternal_InsidePublicClassWithDiscoverInternals_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [assembly: DiscoverInternals]

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                internal static void {|#0:ClassInitialize|}(TestContext testContext)
                {
                }
            }
            """;

        string fixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [assembly: DiscoverInternals]

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code,
            VerifyCS.Diagnostic(ClassInitializeShouldBeValidAnalyzer.Rule)
                .WithLocation(0)
                .WithArguments("ClassInitialize"),
            fixedCode);
    }

    [DataRow("protected")]
    [DataRow("internal")]
    [DataRow("internal protected")]
    [DataRow("private")]
    [TestMethod]
    public async Task WhenClassInitializeIsNotPublic_Diagnostic(string accessibility)
    {
        string code = $$"""
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                {{accessibility}} static void {|#0:ClassInitialize|}(TestContext testContext)
                {
                }
            }
            """;

        string fixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code,
            VerifyCS.Diagnostic().WithLocation(0).WithArguments("ClassInitialize"),
            fixedCode);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsNotOrdinary_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                ~{|#0:MyTestClass|}()
                {
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code,
            VerifyCS.Diagnostic().WithLocation(0).WithArguments("Finalize"),
            code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsGeneric_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static void {|#0:ClassInitialize|}<T>(TestContext testContext)
                {
                }
            }
            """;

        string fixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code,
            VerifyCS.Diagnostic().WithLocation(0).WithArguments("ClassInitialize"),
            fixedCode);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsNotStatic_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public void {|#0:ClassInitialize|}(TestContext testContext)
                {
                }
            }
            """;

        string fixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code,
            VerifyCS.Diagnostic().WithLocation(0).WithArguments("ClassInitialize"),
            fixedCode);
    }

    [TestMethod]
    public async Task WhenClassInitializeDoesNotHaveParameters_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static void {|#0:ClassInitialize|}()
                {
                }
            }
            """;

        string fixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code,
            VerifyCS.Diagnostic().WithLocation(0).WithArguments("ClassInitialize"),
            fixedCode);
    }

#if NET
    [TestMethod]
    public async Task WhenClassInitializeReturnTypeIsNotValid_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using System.Threading.Tasks;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static int {|#0:ClassInitialize0|}(TestContext testContext)
                {
                    return 0;
                }

                [ClassInitialize]
                public static string {|#1:ClassInitialize1|}(TestContext testContext)
                {
                    return "0";
                }

                [ClassInitialize]
                public static Task<int> {|#2:ClassInitialize2|}(TestContext testContext)
                {
                    return Task.FromResult(0);
                }

                [ClassInitialize]
                public static ValueTask<int> {|#3:ClassInitialize3|}(TestContext testContext)
                {
                    return ValueTask.FromResult(0);
                }
            }
            """;

        string fixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using System.Threading.Tasks;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static void ClassInitialize0(TestContext testContext)
                {
                }

                [ClassInitialize]
                public static void ClassInitialize1(TestContext testContext)
                {
                }

                [ClassInitialize]
                public static Task {|CS0161:ClassInitialize2|}(TestContext testContext)
                {
                }

                [ClassInitialize]
                public static ValueTask {|CS0161:ClassInitialize3|}(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code,
            [
                VerifyCS.Diagnostic().WithLocation(0).WithArguments("ClassInitialize0"),
                VerifyCS.Diagnostic().WithLocation(1).WithArguments("ClassInitialize1"),
                VerifyCS.Diagnostic().WithLocation(2).WithArguments("ClassInitialize2"),
                VerifyCS.Diagnostic().WithLocation(3).WithArguments("ClassInitialize3")
            ],
            fixedCode);
    }

    [TestMethod]
    public async Task WhenClassInitializeReturnTypeIsValid_NoDiagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using System.Threading.Tasks;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static void ClassInitialize0(TestContext testContext)
                {
                }

                [ClassInitialize]
                public static Task ClassInitialize1(TestContext testContext)
                {
                    return Task.CompletedTask;
                }

                [ClassInitialize]
                public static ValueTask ClassInitialize2(TestContext testContext)
                {
                    return ValueTask.CompletedTask;
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }
#endif

    [TestMethod]
    public async Task WhenClassInitializeIsAsyncVoid_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using System.Threading.Tasks;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static async void {|#0:ClassInitialize|}(TestContext testContext)
                {
                    await Task.Delay(0);
                }
            }
            """;

        string fixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using System.Threading.Tasks;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static async Task ClassInitialize(TestContext testContext)
                {
                    await Task.Delay(0);
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code,
            VerifyCS.Diagnostic().WithLocation(0).WithArguments("ClassInitialize"),
            fixedCode);
    }

    [TestMethod]
    public async Task WhenMultipleViolations_TheyAllGetFixed()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using System.Threading.Tasks;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public async void {|#0:ClassInitialize|}<T>(int i)
                {
                    await Task.Delay(0);
                }
            }
            """;

        string fixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using System.Threading.Tasks;

            [TestClass]
            public class MyTestClass
            {
                [ClassInitialize]
                public static async Task ClassInitialize(TestContext testContext)
                {
                    await Task.Delay(0);
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            code,
            VerifyCS.Diagnostic().WithLocation(0).WithArguments("ClassInitialize"),
            fixedCode);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsNotOnClass_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            public struct MyTestClass
            {
                [ClassInitialize]
                public static void [|ClassInitialize|](TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsOnSealedClassNotMarkedWithTestClass_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            public sealed class MyTestClass
            {
                [ClassInitialize]
                public static void [|ClassInitialize|](TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsOnNonSealedClassNotMarkedWithTestClass_NoDiagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            public class MyTestClass
            {
                [ClassInitialize]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsOnAbstractClassNotMarkedWithTestClass_AndWithInheritanceBehavior_NoDiagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            public abstract class MyTestClass
            {
                [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsOnAbstractClassMarkedWithTestClass_AndWithInheritanceBehavior_NoDiagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public abstract class MyTestClass
            {
                [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsAbstractClassNotMarkedWithTestClass_AndWithoutInheritanceBehavior_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            public abstract class MyTestClass
            {
                [ClassInitialize]
                public static void [|ClassInitialize|](TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsOnAbstractClassMarkedWithTestClass_AndWithoutInheritanceBehavior_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public abstract class MyTestClass
            {
                [ClassInitialize]
                public static void [|ClassInitialize|](TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsOnAbstractClassMarkedWithTestClass_AndWithInheritanceBehaviorNone_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public abstract class MyTestClass
            {
                [ClassInitialize(InheritanceBehavior.None)]
                public static void [|ClassInitialize|](TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsOnSealedClassMarkedWithTestClass_AndWithInheritanceBehavior_Diagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public sealed class MyTestClass
            {
                [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
                public static void [|ClassInitialize|](TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsOnSealedClassMarkedWithTestClass_AndWithInheritanceBehaviorNone_NoDiagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public sealed class MyTestClass
            {
                [ClassInitialize(InheritanceBehavior.None)]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task WhenClassInitializeIsOnSealedClassMarkedWithTestClass_AndDefaultInheritanceBehavior_NoDiagnostic()
    {
        string code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [TestClass]
            public sealed class MyTestClass
            {
                [ClassInitialize]
                public static void ClassInitialize(TestContext testContext)
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }
}
