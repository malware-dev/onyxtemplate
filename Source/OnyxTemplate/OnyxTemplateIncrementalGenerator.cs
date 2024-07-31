// OnyxTemplate
// 
// Copyright 2023 Morten A. Lyrstad

using System;
using System.IO;
using System.Text;
using Mal.OnyxTemplate.DocumentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mal.OnyxTemplate
{
    /// <summary>
    ///     Incremental template generator for the Onyx templates.
    /// </summary>
    [Generator]
    public class OnyxTemplateIncrementalGenerator : IIncrementalGenerator
    {
        /// <summary>
        ///     Called to initialize the generator and register generation steps via callbacks
        ///     on the <paramref name="context" />
        /// </summary>
        /// <param name="context">
        ///     The <see cref="T:Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext" /> to register
        ///     callbacks on
        /// </param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(AddFrameworkCode);
            
            var onyxFilesProvider = context.AdditionalTextsProvider
                .Where(text => string.Equals(Path.GetExtension(text.Path), ".onyx", StringComparison.InvariantCultureIgnoreCase))
                .Combine(context.AnalyzerConfigOptionsProvider.Select((options, _) =>
                    {
                        options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
                        return rootNamespace ?? "OnyxTemplate";
                    })
                    .Combine(context.CompilationProvider.Select((compilation, _) => compilation.Options.NullableContextOptions != NullableContextOptions.Disable)));

            context.RegisterSourceOutput(onyxFilesProvider, GenerateOnyxSource);
        }

        void AddFrameworkCode(IncrementalGeneratorPostInitializationContext context)
        {
            var writer = new StringWriter();
            var fw = new FrameworkWriter(writer);
            fw.Write();
            context.AddSource("Mal.OnyxTemplate.TemplateFramework.cs", SourceText.From(fw.ToString(), Encoding.UTF8));
        }

        static void GenerateOnyxSource(SourceProductionContext context, (AdditionalText sourceText, (string rootNamespace, bool supportsNullable) Right) info)
        {
            var (sourceText, (rootNamespace, supportsNullable)) = info;
            var text = sourceText.GetText()?.ToString() ?? string.Empty;
            try
            {
                var document = Document.Parse(text);
                var writer = new StringWriter();
                var dw = new DocumentWriter(writer, supportsNullable);
                var className = Identifier.MakeSafe(Path.GetFileNameWithoutExtension(sourceText.Path));
                dw.Write(document, rootNamespace, className, true);

                var source = writer.ToString();
                var hintName = Path.GetFileNameWithoutExtension(sourceText.Path) + ".onyx.cs";

                context.AddSource(hintName, source);
            }
            catch (DomException e)
            {
                var ptr = e.Ptr;
                var length = e.Length;
                var textSpan = TextSpan.FromBounds(ptr.Index, ptr.Index + length);
                var lc = ptr.GetLineInfo();
                var linePositionSpan = new LinePositionSpan(new LinePosition(lc.Line-1, lc.Char-1), new LinePosition(lc.Line-1, lc.Char-1 + length));
                var location = Location.Create(sourceText.Path, textSpan, linePositionSpan);
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.TemplateError, location, e.Message));
            }
        }
    }
}