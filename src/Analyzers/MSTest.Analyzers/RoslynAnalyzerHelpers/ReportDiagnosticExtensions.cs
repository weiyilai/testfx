﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis;

internal static class ReportDiagnosticExtensions
{
    public static DiagnosticSeverity? ToDiagnosticSeverity(this ReportDiagnostic reportDiagnostic) => reportDiagnostic switch
    {
        ReportDiagnostic.Error => DiagnosticSeverity.Error,
        ReportDiagnostic.Warn => DiagnosticSeverity.Warning,
        ReportDiagnostic.Info => DiagnosticSeverity.Info,
        ReportDiagnostic.Hidden => DiagnosticSeverity.Hidden,
        ReportDiagnostic.Suppress => null,
        ReportDiagnostic.Default => null,
        _ => throw new NotImplementedException(),
    };

    public static bool IsLessSevereThan(this ReportDiagnostic current, ReportDiagnostic other) => current switch
    {
        ReportDiagnostic.Error => false,

        ReportDiagnostic.Warn =>
            other switch
            {
                ReportDiagnostic.Error => true,
                _ => false
            },

        ReportDiagnostic.Info =>
            other switch
            {
                ReportDiagnostic.Error => true,
                ReportDiagnostic.Warn => true,
                _ => false
            },

        ReportDiagnostic.Hidden =>
            other switch
            {
                ReportDiagnostic.Error => true,
                ReportDiagnostic.Warn => true,
                ReportDiagnostic.Info => true,
                _ => false
            },

        ReportDiagnostic.Suppress => true,

        _ => false
    };
}
