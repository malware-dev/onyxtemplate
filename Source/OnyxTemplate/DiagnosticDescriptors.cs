// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using Microsoft.CodeAnalysis;

namespace Mal.OnyxTemplate
{
    /// <summary>
    /// A static class containing all diagnostic descriptors used by the OnyxTemplate project.
    /// </summary>
    public static class DiagnosticDescriptors
    {
        /// <summary>
        /// An error that occurs when a template is invalid.
        /// </summary>
        public static readonly DiagnosticDescriptor TemplateError = new DiagnosticDescriptor(
            id: "OT0001",
            title: "Template error",
            messageFormat: "Template error: {0}",
            category: "OnyxTemplate",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}