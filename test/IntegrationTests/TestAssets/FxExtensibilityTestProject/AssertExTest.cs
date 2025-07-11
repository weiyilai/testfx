﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MSTest.Extensibility.Samples;

namespace FxExtensibilityTestProject;

[TestClass]
public class AssertExTest
{
    [TestMethod]
    public void BasicAssertExtensionTest() => Assert.Instance.IsOfType<ArgumentException>(new ArgumentOutOfRangeException());

    [TestMethod]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public void BasicFailingAssertExtensionTest() => Assert.Instance.IsOfType<FormatException>(new ArgumentNullException());

    [TestMethod]
    public void ChainedAssertExtensionTest() => Assert.Instance.Is().Divisor(120, 5);

    [TestMethod]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public void ChainedFailingAssertExtensionTest() => Assert.Instance.Is().Positive(-10);
}
