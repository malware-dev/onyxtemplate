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
            //
            // var onyxFilesProvider = context.AdditionalTextsProvider.Where(text => string.Equals(Path.GetExtension(text.Path), ".onyx", StringComparison.InvariantCultureIgnoreCase))
            //     .Combine(context.AnalyzerConfigOptionsProvider.Select((options, _) =>
            //     {
            //         options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
            //         return rootNamespace ?? "OnyxTemplate";
            //     }));
            //
            // context.RegisterSourceOutput(onyxFilesProvider, GenerateOnyxSource);
        }

        void AddFrameworkCode(IncrementalGeneratorPostInitializationContext context)
        {
            var file = new StringBuilder();
            file.AppendLine("using System;")
                .AppendLine("using System.Text;")
                .AppendLine("namespace Mal.OnyxTemplate")
                .AppendLine("{")
                // .AppendLine("    /// <summary>")
                // .AppendLine("    ///     Base class for generated Onyx templates.")
                // .AppendLine("    /// </summary>")
                .AppendLine("    public abstract class TextTemplate")
                .AppendLine("    {")
                // .AppendLine("        /// <summary>")
                // .AppendLine("        ///     A writer used to write the template.")
                // .AppendLine("        /// </summary>")
                .AppendLine("        protected class Writer")
                .AppendLine("        {")
                .AppendLine("            readonly StringBuilder _buffer = new StringBuilder(1024);")
                .AppendLine("            static int FindEndOfLine(string input, int start)")
                .AppendLine("            {")
                .AppendLine("                var index = input.IndexOf('\\n', start);")
                .AppendLine("                return index < 0? input.Length : index + 1;")
                .AppendLine("            }")
                .AppendLine("            public int Col")
                .AppendLine("            {")
                .AppendLine("               get")
                .AppendLine("               {")
                .AppendLine("                   var index = _buffer.Length - 1;")
                .AppendLine("                   while (index >= 0 && _buffer[index] != '\\n')")
                .AppendLine("                       index--;")
                .AppendLine("                   return _buffer.Length - index;")
                .AppendLine("               }")
                .AppendLine("            }")
                // .AppendLine("            /// <summary>")
                // .AppendLine("            ///     Gets or sets the current indentation.")
                // .AppendLine("            /// </summary>")
                .AppendLine("            public string Indentation { get; set; } = \"\";")
                .AppendLine("            void Indent() => _buffer.Append(Indentation);")
                // .AppendLine("            /// <summary>")
                // .AppendLine("            ///     Appends a string to the buffer.")
                // .AppendLine("            /// </summary>")
                .AppendLine("            public void Append(string input)")
                .AppendLine("            {")
                .AppendLine("                if (string.IsNullOrEmpty(input)) return;")
                .AppendLine("                if (_buffer.Length > 0 && _buffer[_buffer.Length - 1] == '\\n')")
                .AppendLine("                    Indent();")
                .AppendLine("                var start = 0;")
                .AppendLine("                var end = FindEndOfLine(input, start);")
                .AppendLine("                _buffer.Append(input, start, end);")
                .AppendLine("                while (end < input.Length)")
                .AppendLine("                {")
                .AppendLine("                    start = end;")
                .AppendLine("                    end = FindEndOfLine(input, start);")
                .AppendLine("                    Indent();")
                .AppendLine("                    _buffer.Append(input, start, end - start);")
                .AppendLine("                }")
                .AppendLine("            }")
                // .AppendLine("            /// <summary>")
                // .AppendLine("            ///     Appends a string to the buffer followed by a newline.")
                // .AppendLine("            /// </summary>")
                .AppendLine("            public void AppendLine(string input = null)")
                .AppendLine("            {")
                .AppendLine("                Append(input);")
                .AppendLine("                _buffer.AppendLine();")
                .AppendLine("            }")
                .AppendLine("            /// <summary>")
                .AppendLine("            ///     Returns the buffer as a string.")
                .AppendLine("            /// </summary>")
                .AppendLine("            public override string ToString() => _buffer.ToString();")
                .AppendLine("        }")
                // .AppendLine("        /// <summary>")
                // .AppendLine("        ///     A state object used to track how an item is positioned in a list.")
                // .AppendLine("        /// </summary>")
                .AppendLine("        protected class State")
                .AppendLine("        {")
                .AppendLine("            int _index;")
                // .AppendLine("            /// <summary>")
                // .AppendLine("            ///     Initializes a new instance of the <see cref=\"State\" /> class.")
                // .AppendLine("            /// </summary>")
                .AppendLine("            public State(int count, State parent = null)")
                .AppendLine("            {")
                .AppendLine("                Parent = parent;")
                .AppendLine("                Count = count;")
                .AppendLine("            }")
                // .AppendLine("            /// <summary>")
                // .AppendLine("            ///     Returns the parent state, or <c>null</c> if this is the root state.")
                // .AppendLine("            /// </summary>")
                .AppendLine("            public State Parent { get; }")
                // .AppendLine("            /// <summary>")
                // .AppendLine("            ///     Returns the total number of items in the list.")
                // .AppendLine("            /// </summary>")
                .AppendLine("            public int Count { get; }")
                // .AppendLine("            /// <summary>")
                // .AppendLine("            ///     Gets or sets the index of the current item in the list.")
                // .AppendLine("            /// </summary>")
                .AppendLine("            public int Index")
                .AppendLine("            {")
                .AppendLine("                get { return _index; }")
                .AppendLine("                set")
                .AppendLine("                {")
                .AppendLine("                    _index = value;")
                .AppendLine("                    First = value == 0;")
                .AppendLine("                    Last = value == Count - 1;")
                .AppendLine("                    Even = value % 2 == 0;")
                .AppendLine("                }")
                .AppendLine("            }")
                // .AppendLine("            /// <summary>")
                // .AppendLine("            ///     Determines whether the item is the first in the list.")
                // .AppendLine("            /// </summary>")
                .AppendLine("            public bool First { get; private set; }")
                // .AppendLine("            /// <summary>")
                // .AppendLine("            ///     Determines whether the item is the last in the list.")
                // .AppendLine("            /// </summary>")
                .AppendLine("            public bool Last { get; private set; }")
                // .AppendLine("            /// <summary>")
                // .AppendLine("            ///     Determines whether the item is in an even position in the list.")
                // .AppendLine("            /// </summary>")
                .AppendLine("            public bool Even { get; private set;}")
                // .AppendLine("            /// <summary>")
                // .AppendLine("            ///     Determines whether the item is in the middle of the list.")
                // .AppendLine("            /// </summary>")
                .AppendLine("            public bool Middle => !First && !Last;")
                // .AppendLine("            /// <summary>")
                // .AppendLine("            ///     Determines whether the item is in an odd position in the list.")
                // .AppendLine("            /// </summary>")
                .AppendLine("            public bool Odd => !Even;")
                .AppendLine("        }")
                // .AppendLine("        /// <summary>")
                // .AppendLine("        ///     A delegate used to write a list item.")
                // .AppendLine("        /// </summary>")
                .AppendLine("        protected delegate void ListWriterFn<T>(Writer writer, T item, in State state);")
                // .AppendLine("        /// <summary>")
                // .AppendLine("        ///     Writes a list of items using the specified writer function.")
                // .AppendLine("        /// </summary>")
                .AppendLine("        protected static void WriteListItem<T>(IEnumerable<T> items, Writer writer, ListWriterFn<T> writeFn)")
                .AppendLine("        {")
                .AppendLine("            using (var enumerator = items.GetEnumerator())")
                .AppendLine("            {")
                .AppendLine("                if (!enumerator.MoveNext()) return;")
                .AppendLine("                var n = 1;")
                .AppendLine("                do")
                .AppendLine("                {")
                .AppendLine("                    var current = enumerator.Current;")
                .AppendLine("                    var state = new State(n == 0, !enumerator.MoveNext(), n % 2 == 0);")
                .AppendLine("                    n++;")
                .AppendLine("                    writeFn(writer, current, state);")
                .AppendLine("                    if (state.Last)")
                .AppendLine("                        break;")
                .AppendLine("                }")
                .AppendLine("                while (true);")
                .AppendLine("            }")
                .AppendLine("        }")
                .AppendLine("    }")
                .AppendLine("}");
            
            context.AddSource("Mal.OnyxTemplate.TemplateFramework.cs", SourceText.From(file.ToString(), Encoding.UTF8));
        }

        static void GenerateOnyxSource(SourceProductionContext context, (AdditionalText sourceText, (string rootNamespace, bool supportsNullable) Right) info)
        {
            var (sourceText, (rootNamespace, supportsNullable)) = info;
            // var templateContext = new TemplateContext(sourceText, context.CancellationToken, context.ReportDiagnostic, context.AddSource, rootNamespace, supportsNullable);
            // OnyxProducer.GenerateOnyxSource(templateContext, sourceText);
            var text = sourceText.GetText()?.ToString() ?? string.Empty;
            try
            {
                var document = Document.Parse(text);
                var writer = new StringWriter();
                var dw = new DocumentWriter(writer, supportsNullable);
                var className = Document.CSharpify(Path.GetFileNameWithoutExtension(sourceText.Path));
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
                var linePositionSpan = new LinePositionSpan(new LinePosition(lc.Line, lc.Char), new LinePosition(lc.Line, lc.Char + length));
                var location = Location.Create(sourceText.Path, textSpan, linePositionSpan);
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.TemplateError, location, e.Message));
            }
        }
    }
}