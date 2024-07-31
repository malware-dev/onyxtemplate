// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System.Collections.Generic;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     The base class of all document blocks.
    /// </summary>
    public abstract class DocumentBlock
    {
        /// <summary>
        ///     Returns all descendants of this block.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<DocumentBlock> Descendants();

        /// <summary>
        ///     Determines if this block needs a macro state to be rendered (will also evaluate any descendants).
        /// </summary>
        /// <returns></returns>
        public abstract bool NeedsMacroState();
    }
}