// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    /// Determines the kind of a <see cref="Token"/>.
    /// </summary>
    enum TokenKind
    {
        /// <summary>
        /// Not defined.
        /// </summary>
        None,
        
        /// <summary>
        /// <c>$template</c>
        /// </summary>
        Template,
        
        /// <summary>
        /// <c>$if</c>
        /// </summary>
        If,
        
        /// <summary>
        /// <c>$elseif</c>
        /// </summary>
        ElseIf,
        
        /// <summary>
        /// <c>$else</c>
        /// </summary>
        Else,
        
        /// <summary>
        /// <c>$end</c>
        /// </summary>
        End,
        
        /// <summary>
        /// <c>$foreach</c>
        /// </summary>
        ForEach,
        
        /// <summary>
        /// <c>$next</c>
        /// </summary>
        Next,
        
        /// <summary>
        /// A word, which is a sequence of characters that are not whitespace or punctuation, and does not start with a digit.
        /// </summary>
        Word,
        
        /// <summary>
        /// The <c>.</c> character.
        /// </summary>
        Dot,
        
        /// <summary>
        /// The <c>:</c> character.
        /// </summary>
        Colon,
        
        /// <summary>
        /// <c>first</c>
        /// </summary>
        First,
        
        /// <summary>
        /// <c>last</c>
        /// </summary>
        Last,
        
        /// <summary>
        /// <c>middle</c>
        /// </summary>
        Middle,
        
        /// <summary>
        /// <c>odd</c>
        /// </summary>
        Odd,
        
        /// <summary>
        /// <c>even</c>
        /// </summary>
        Even,
        
        /// <summary>
        /// A string literal: <c>"..."</c> or <c>'...'</c>. Cannot contain newlines or the same quote character that delimits the string.
        /// </summary>
        /// <remarks>
        /// There's currently no support for escape sequences in string literals.
        /// </remarks>
        String
    }
}