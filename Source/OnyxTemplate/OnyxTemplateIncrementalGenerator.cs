// OnyxTemplate
// 
// Copyright 2023 Morten A. Lyrstad
//
// There's a problem with incremental generators where it doesn't detect changes to the .onyx file.
//
// using System;
// using System.IO;
// using Microsoft.CodeAnalysis;
//
// namespace Mal.OnyxTemplate
// {
//     /// <summary>
//     /// Incremental template generator for the Onyx templates.
//     /// </summary>
//     [Generator]
//     public class OnyxTemplateIncrementalGenerator : IIncrementalGenerator
//     {
//         /// <summary>
//         /// Called to initialize the generator and register generation steps via callbacks
//         /// on the <paramref name="context" />
//         /// </summary>
//         /// <param name="context">The <see cref="T:Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext" /> to register callbacks on</param>
//         public void Initialize(IncrementalGeneratorInitializationContext context)
//         {
//             var onyxFilesProvider = context.AdditionalTextsProvider.Where(text => string.Equals(Path.GetExtension(text.Path), ".onyx", StringComparison.InvariantCultureIgnoreCase))
//                 .Combine(context.AnalyzerConfigOptionsProvider.Select((options, _) =>
//                 {
//                     options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
//                     return rootNamespace ?? "OnyxTemplate";
//                 }));
//
//             context.RegisterSourceOutput(onyxFilesProvider, GenerateOnyxSource);
//         }
//
//         static void GenerateOnyxSource(SourceProductionContext context, (AdditionalText sourceText, string rootNamespace) pair)
//         {
//             var (sourceText, rootNamespace) = pair;
//             var templateContext = new TemplateContext(sourceText, context.CancellationToken, context.ReportDiagnostic, context.AddSource, rootNamespace);
//             OnyxProducer.GenerateOnyxSource(templateContext, sourceText);
//         }
//     }
// }