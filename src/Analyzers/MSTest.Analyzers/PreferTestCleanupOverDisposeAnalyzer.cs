﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

using Analyzer.Utilities.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using MSTest.Analyzers.Helpers;

namespace MSTest.Analyzers;

/// <summary>
/// MSTEST0022: <inheritdoc cref="Resources.PreferTestCleanupOverDisposeTitle"/>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public sealed class PreferTestCleanupOverDisposeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableResourceString Title = new(nameof(Resources.PreferTestCleanupOverDisposeTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableResourceString MessageFormat = new(nameof(Resources.PreferTestCleanupOverDisposeMessageFormat), Resources.ResourceManager, typeof(Resources));

    internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
        DiagnosticIds.PreferTestCleanupOverDisposeRuleId,
        Title,
        MessageFormat,
        null,
        Category.Design,
        DiagnosticSeverity.Info,
        isEnabledByDefault: false,
        disableInAllMode: true);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(Rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(context =>
        {
            if (context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemIDisposable, out INamedTypeSymbol? idisposableSymbol) &&
                context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftVisualStudioTestToolsUnitTestingTestClassAttribute, out INamedTypeSymbol? testClassAttributeSymbol))
            {
                INamedTypeSymbol? iasyncDisposableSymbol = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemIAsyncDisposable);
                INamedTypeSymbol? valueTaskSymbol = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksValueTask);
                context.RegisterSymbolAction(context => AnalyzeSymbol(context, testClassAttributeSymbol, idisposableSymbol, iasyncDisposableSymbol, valueTaskSymbol), SymbolKind.Method);
            }
        });
    }

    private static void AnalyzeSymbol(
        SymbolAnalysisContext context,
        INamedTypeSymbol testClassAttributeSymbol,
        INamedTypeSymbol idisposableSymbol,
        INamedTypeSymbol? iasyncDisposableSymbol,
        INamedTypeSymbol? valueTaskSymbol)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        if (methodSymbol.ContainingType.GetAttributes().Any(x => x.AttributeClass.Inherits(testClassAttributeSymbol)) &&
            (methodSymbol.IsAsyncDisposeImplementation(iasyncDisposableSymbol, valueTaskSymbol)
            || methodSymbol.IsDisposeImplementation(idisposableSymbol)))
        {
            context.ReportDiagnostic(methodSymbol.CreateDiagnostic(Rule));
        }
    }
}
