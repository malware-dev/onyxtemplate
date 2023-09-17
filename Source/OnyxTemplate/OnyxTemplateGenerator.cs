// // OnyxTemplate
// // 
// // Copyright 2023 Morten A. Lyrstad
//
// using System;
// using System.IO;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.CodeAnalysis;
//
// namespace Mal.OnyxTemplate
// {
//     /// <summary>
//     ///     Template generator for the Onyx templates.
//     /// </summary>
//     [Generator]
//     public class OnyxTemplateGenerator : ISourceGenerator
//     {
//         /// <summary>
//         ///     Called before generation occurs. A generator can use the <paramref name="context" />
//         ///     to register callbacks required to perform generation.
//         /// </summary>
//         /// <param name="context">
//         ///     The <see cref="T:Microsoft.CodeAnalysis.GeneratorInitializationContext" /> to register callbacks
//         ///     on
//         /// </param>
//         public void Initialize(GeneratorInitializationContext context)
//         { }
//
//         /// <summary>
//         ///     Called to perform source generation. A generator can use the <paramref name="context" />
//         ///     to add source files via the
//         ///     <see
//         ///         cref="M:Microsoft.CodeAnalysis.GeneratorExecutionContext.AddSource(System.String,Microsoft.CodeAnalysis.Text.SourceText)" />
//         ///     method.
//         /// </summary>
//         /// <param name="context">The <see cref="T:Microsoft.CodeAnalysis.GeneratorExecutionContext" /> to add source to</param>
//         /// <remarks>
//         ///     This call represents the main generation step. It is called after a
//         ///     <see cref="T:Microsoft.CodeAnalysis.Compilation" /> is
//         ///     created that contains the user written code.
//         ///     A generator can use the <see cref="P:Microsoft.CodeAnalysis.GeneratorExecutionContext.Compilation" /> property to
//         ///     discover information about the users compilation and make decisions on what source to
//         ///     provide.
//         /// </remarks>
//         public void Execute(GeneratorExecutionContext context)
//         {
//             context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
//             rootNamespace = rootNamespace ?? "OnyxTemplate";
//
//             var files = context.AdditionalFiles.Where(text => string.Equals(Path.GetExtension(text.Path), ".onyx", StringComparison.InvariantCultureIgnoreCase)).ToList();
//             Parallel.ForEach(files,
//                 sourceText =>
//                 {
//                     var templateContext = new TemplateContext(sourceText, context.CancellationToken, context.ReportDiagnostic, context.AddSource, rootNamespace);
//                     OnyxProducer.GenerateOnyxSource(templateContext, sourceText);
//                 });
//         }
//     }
// }