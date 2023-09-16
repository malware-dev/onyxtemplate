// OnyxTemplate
// 
// Copyright 2023 Morten A. Lyrstad

namespace Mal.OnyxTemplate
{
    /// <summary>
    /// Defines the type of a <see cref="Macro"/>.
    /// </summary>
    enum MacroType
    {
        /// <summary>
        /// The root macro. Only contains child macros.
        /// </summary>
        Root,

        /// <summary>
        /// The header macro
        /// </summary>
        Header,
        
        /// <summary>
        /// Plaintext.
        /// </summary>
        Text,

        /// <summary>
        /// A single item.
        /// </summary>
        Ref,
        
        /// <summary>
        /// An item loop.
        /// </summary>
        ForEach,
        
        /// <summary>
        /// The "next" part of <see cref="ForEach"/>.
        /// </summary>
        Next
    }
}