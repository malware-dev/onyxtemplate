// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System.Collections.Generic;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    /// The base class for all conditional macro sections.
    /// </summary>
    public abstract class ConditionalMacroSection
    {
        /// <summary>
        /// Returns all descendants of this section.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<DocumentBlock> Descendants();
        
        /// <summary>
        /// Determines if this section needs a macro state to be rendered (will also evaluate any descendants).
        /// </summary>
        /// <returns></returns>
        public abstract bool NeedsMacroState();
    }
}