namespace Mal.OnyxTemplate.DocumentModel
{
    readonly struct TokenPtr
    {
        readonly IntermediateMacro _macro;
        public readonly int Index;

        public TokenPtr(IntermediateMacro macro, int index = 0)
        {
            _macro = macro;
            Index = index;
        }
        
        public Token Current
        {
            get
            {
                if (Index < 0 || Index >= _macro.Tokens.Length)
                    return default;
                return _macro[Index];
            }
        }
        
        public TextPtr TextPtr => new TextPtr(Current.Image.Text, Current.Image.Start);

        public bool IsAtEnd => Index >= _macro.Tokens.Length;
        
        public static TokenPtr operator ++(TokenPtr ptr) => new TokenPtr(ptr._macro, ptr.Index + 1);
        public static TokenPtr operator --(TokenPtr ptr) => new TokenPtr(ptr._macro, ptr.Index - 1);
        public static TokenPtr operator +(TokenPtr ptr, int offset) => new TokenPtr(ptr._macro, ptr.Index + offset);
        public static TokenPtr operator -(TokenPtr ptr, int offset) => new TokenPtr(ptr._macro, ptr.Index - offset);

        public bool IsKind(TokenKind kind) => !IsAtEnd && Current.Kind == kind;
    }
}