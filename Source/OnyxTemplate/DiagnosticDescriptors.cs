// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using Microsoft.CodeAnalysis;

namespace Mal.OnyxTemplate
{
    public class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor TemplateError = new DiagnosticDescriptor(
            id: "OT0001",
            title: "Template error",
            messageFormat: "Template error: {0}",
            category: "OnyxTemplate",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}