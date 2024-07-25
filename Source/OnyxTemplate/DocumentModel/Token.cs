// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

namespace Mal.OnyxTemplate.DocumentModel
{
    readonly struct Token
    {
        public readonly TokenKind Kind;
        public readonly StringSegment Image;

        public Token(TokenKind kind, StringSegment image)
        {
            Kind = kind;
            Image = image;
        }

        public int Select(StringSegment t1, StringSegment t2) => Image.EqualsIgnoreCase(t1) ? 0 : Image.EqualsIgnoreCase(t2) ? 1 : -1;
        public int Select(StringSegment t1, StringSegment t2, StringSegment t3) => Image.EqualsIgnoreCase(t1) ? 0 : Image.EqualsIgnoreCase(t2) ? 1 : Image.EqualsIgnoreCase(t3) ? 2 : -1;
        public int Select(StringSegment t1, StringSegment t2, StringSegment t3, StringSegment t4) => Image.EqualsIgnoreCase(t1) ? 0 : Image.EqualsIgnoreCase(t2) ? 1 : Image.EqualsIgnoreCase(t3) ? 2 : Image.EqualsIgnoreCase(t4) ? 3 : -1;
        public int Select(StringSegment t1, StringSegment t2, StringSegment t3, StringSegment t4, StringSegment t5) => Image.EqualsIgnoreCase(t1) ? 0 : Image.EqualsIgnoreCase(t2) ? 1 : Image.EqualsIgnoreCase(t3) ? 2 : Image.EqualsIgnoreCase(t4) ? 3 : Image.EqualsIgnoreCase(t5) ? 4 : -1;
        public int Select(StringSegment t1, StringSegment t2, StringSegment t3, StringSegment t4, StringSegment t5, StringSegment t6) => Image.EqualsIgnoreCase(t1) ? 0 : Image.EqualsIgnoreCase(t2) ? 1 : Image.EqualsIgnoreCase(t3) ? 2 : Image.EqualsIgnoreCase(t4) ? 3 : Image.EqualsIgnoreCase(t5) ? 4 : Image.EqualsIgnoreCase(t6) ? 5 : -1;
        
        public int Select(params StringSegment[] tokens)
        {
            for (var i = 0; i < tokens.Length; i++)
            {
                if (Image.EqualsIgnoreCase(tokens[i]))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}