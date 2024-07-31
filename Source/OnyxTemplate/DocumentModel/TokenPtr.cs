namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    /// A pointer to a token in an intermediate macro.
    /// </summary>
    readonly struct TokenPtr
    {
        readonly IntermediateMacro _macro;
        
        /// <summary>
        /// The index of the token.
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Creates a new instance of <see cref="TokenPtr"/>.
        /// </summary>
        /// <param name="macro"></param>
        /// <param name="index"></param>
        public TokenPtr(IntermediateMacro macro, int index = 0)
        {
            _macro = macro;
            Index = index;
        }
        
        /// <summary>
        /// The current token.
        /// </summary>
        public Token Current
        {
            get
            {
                if (Index < 0 || Index >= _macro.Tokens.Length)
                    return default;
                return _macro[Index];
            }
        }
        
        /// <summary>
        /// A pointer to the text of the current token.
        /// </summary>
        public TextPtr TextPtr => new TextPtr(Current.Image.Text, Current.Image.Start);

        /// <summary>
        /// Whether the pointer is at the end of the macro (or beyond).
        /// </summary>
        public bool IsAtEnd => Index >= _macro.Tokens.Length;
        
        /// <summary>
        /// Moves the pointer to the next token.
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static TokenPtr operator ++(TokenPtr ptr) => new TokenPtr(ptr._macro, ptr.Index + 1);
        
        /// <summary>
        /// Moves the pointer to the previous token.
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static TokenPtr operator --(TokenPtr ptr) => new TokenPtr(ptr._macro, ptr.Index - 1);
        
        /// <summary>
        /// Moves the pointer by the specified offset.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static TokenPtr operator +(TokenPtr ptr, int offset) => new TokenPtr(ptr._macro, ptr.Index + offset);
        
        /// <summary>
        /// Moves the pointer back by the specified offset.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static TokenPtr operator -(TokenPtr ptr, int offset) => new TokenPtr(ptr._macro, ptr.Index - offset);

        /// <summary>
        /// Determines whether the current token is of the specified kind.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public bool IsKind(TokenKind kind) => !IsAtEnd && Current.Kind == kind;
    }
}