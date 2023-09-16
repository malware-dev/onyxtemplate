// OnyxTemplate
// 
// Copyright 2023 Morten A. Lyrstad

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mal.OnyxTemplate
{
    /// <summary>
    /// Context for the source generator.
    /// </summary>
    class TemplateContext
    {
        readonly AdditionalText _source;
        readonly CancellationToken _cancellationToken;
        readonly Action<Diagnostic> _reportDiagnostic;
        readonly Action<string, string> _addSource;
        readonly Stack<Macro> _scopes = new Stack<Macro>();

        /// <summary>
        /// Create a new <see cref="TemplateContext"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="reportDiagnostic"></param>
        /// <param name="addSource"></param>
        /// <param name="rootNamespace"></param>
        public TemplateContext(AdditionalText source, CancellationToken cancellationToken, Action<Diagnostic> reportDiagnostic, Action<string, string> addSource, string rootNamespace)
        {
            RootNamespace = rootNamespace;
            _source = source;
            _cancellationToken = cancellationToken;
            _reportDiagnostic = reportDiagnostic;
            _addSource = addSource;
            CurrentScope = Root;
        }

        /// <summary>
        /// Gets the default namespace of the project the .onyx file resides in.
        /// </summary>
        public string RootNamespace { get; }

        /// <summary>
        /// The currently scoped macro. <see cref="Add"/> will add macros to this scope.
        /// </summary>
        public Macro CurrentScope { get; private set; }

        /// <summary>
        /// The root macro.
        /// </summary>
        public Macro Root { get; } = new Macro(MacroType.Root, new TextPtr(), new TextPtr(), null, null, null);

        /// <summary>
        /// Whether this is a subscope (i.e. not the root node). 
        /// </summary>
        public bool IsSubScope => _scopes.Count > 0;

        bool _canConfigure = true;
        
        /// <summary>
        /// Whether auto-indentation is enabled for this template.
        /// </summary>
        public bool Indentation { get; private set; }
        
        /// <summary>
        /// Whether the generated runtime template should be public.
        /// </summary>
        public bool PublicClass { get; private set; }

        /// <summary>
        /// Pop a scope macro off of the stack.
        /// </summary>
        public void PopScope()
        {
            CurrentScope = _scopes.Pop();
        }

        /// <summary>
        /// Report an error in the template.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public void Error(TextPtr from, TextPtr to, int code, string message)
        {
            var descriptor = new DiagnosticDescriptor($"OT{code:D4}", "OnyxTemplate Config Error", message, "OnyxTemplate", DiagnosticSeverity.Error, true);

            var location = Location.None;
            var sourceText = _source.GetText(_cancellationToken);
            if (sourceText != null)
            {
                var span = new TextSpan(from.Index, to.Index - from.Index);
                location = Location.Create(_source.Path, span, sourceText.Lines.GetLinePositionSpan(span));
            }

            var diagnostic = Diagnostic.Create(descriptor, location);
            _reportDiagnostic(diagnostic);
        }

        /// <summary>
        /// Add a macro to the current scope.
        /// </summary>
        /// <param name="macro"></param>
        public void Add(Macro macro)
        {
            if (macro.Type == MacroType.Header)
            {
                if (!_canConfigure)
                {
                    Error(macro.Start, macro.End, 1, "The header must occur at the very top of the template file.");
                    return;
                }

                Indentation = macro.Tags.Contains("indented");
                PublicClass = macro.Tags.Contains("public");
                _canConfigure = false;
                return;
            }

            _canConfigure = false;

            macro.Parent = CurrentScope;
            CurrentScope.Macros.Add(macro);
            if (macro.Type != MacroType.ForEach) return;
            _scopes.Push(CurrentScope);
            CurrentScope = macro;
        }

        /// <summary>
        /// Add a new source file to the target project.
        /// </summary>
        /// <param name="hintName"></param>
        /// <param name="source"></param>
        public void AddSource(string hintName, string source)
        {
            _addSource(hintName, source);
        }
    }
}