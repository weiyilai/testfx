﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

using Analyzer.Utilities.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using MSTest.Analyzers.Helpers;

namespace MSTest.Analyzers;

/// <summary>
/// MSTEST0028: <inheritdoc cref="Resources.UseAsyncSuffixTestFixtureMethodSuppressorJustification"/>.
/// </summary>
#pragma warning disable RS1004 // Recommend adding language support to diagnostic analyzer - This suppressor is not valid for VB
[DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning restore RS1004 // Recommend adding language support to diagnostic analyzer
public sealed class NonNullableReferenceNotInitializedSuppressor : DiagnosticSuppressor
{
    // CS8618: Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
    // https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-messages/nullable-warnings?f1url=%3FappId%3Droslyn%26k%3Dk(CS8618)#nonnullable-reference-not-initialized
    private const string SuppressedDiagnosticId = "CS8618";

    internal static readonly SuppressionDescriptor Rule =
        new(DiagnosticIds.NonNullableReferenceNotInitializedSuppressorRuleId, SuppressedDiagnosticId, Resources.UseAsyncSuffixTestFixtureMethodSuppressorJustification);

    /// <inheritdoc />
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Rule);

    /// <inheritdoc />
    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftVisualStudioTestToolsUnitTestingTestContext, out INamedTypeSymbol? testContextSymbol)
            || !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftVisualStudioTestToolsUnitTestingTestClassAttribute, out INamedTypeSymbol? testClassAttributeSymbol))
        {
            return;
        }

        foreach (Diagnostic diagnostic in context.ReportedDiagnostics)
        {
            // The main diagnostic location isn't always pointing to the TestContext property.
            // It can point to the constructor.
            // The additional locations will have the right thing.
            // See https://github.com/dotnet/roslyn/issues/79188#issuecomment-3017087900.
            // It was an intentional design to include the additional locations specifically for DiagnosticSuppressor scenarios.
            // So it is safe to use AdditionalLocations here. We are not relying on an implementation detail here.
            // However, we still fallback to diagnostic.Location just in case Roslyn regresses the AdditionalLocations behavior.
            // Such regression happened in the past in Roslyn.
            // See https://github.com/dotnet/roslyn/issues/66037
            Location location = diagnostic.AdditionalLocations.Count >= 1 ? diagnostic.AdditionalLocations[0] : diagnostic.Location;
            if (location.SourceTree is not { } tree)
            {
                continue;
            }

            SyntaxNode root = tree.GetRoot(context.CancellationToken);
            SyntaxNode node = root.FindNode(location.SourceSpan, getInnermostNodeForTie: true);

            SemanticModel semanticModel = context.GetSemanticModel(tree);
            ISymbol? declaredSymbol = semanticModel.GetDeclaredSymbol(node, context.CancellationToken);
            if (declaredSymbol is IPropertySymbol property
                && string.Equals(property.Name, "TestContext", StringComparison.Ordinal)
                && SymbolEqualityComparer.Default.Equals(testContextSymbol, property.GetMethod?.ReturnType)
                && property.ContainingType.GetAttributes().Any(attr => attr.AttributeClass.Inherits(testClassAttributeSymbol)))
            {
                context.ReportSuppression(Suppression.Create(Rule, diagnostic));
            }
        }
    }
}
