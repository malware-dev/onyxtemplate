// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     A token used when parsing a macro.
    /// </summary>
    readonly struct Token
    {
        /// <summary>
        ///     What kind of token this is.
        /// </summary>
        public readonly TokenKind Kind;

        /// <summary>
        ///     The image of the token.
        /// </summary>
        public readonly StringSegment Image;

        /// <summary>
        ///     Creates a new instance of <see cref="Token" />.
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="image"></param>
        public Token(TokenKind kind, StringSegment image)
        {
            Kind = kind;
            Image = image;
        }

        /// <summary>
        ///     Selects the index of the token that matches the image. Returns -1 if no match is found.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public int Select(StringSegment t1, StringSegment t2) => Image.EqualsIgnoreCase(t1) ? 0 : Image.EqualsIgnoreCase(t2) ? 1 : -1;

        /// <summary>
        ///     Selects the index of the token that matches the image. Returns -1 if no match is found.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <returns></returns>
        public int Select(StringSegment t1, StringSegment t2, StringSegment t3) => Image.EqualsIgnoreCase(t1) ? 0 : Image.EqualsIgnoreCase(t2) ? 1 : Image.EqualsIgnoreCase(t3) ? 2 : -1;

        /// <summary>
        ///     Selects the index of the token that matches the image. Returns -1 if no match is found.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <returns></returns>
        public int Select(StringSegment t1, StringSegment t2, StringSegment t3, StringSegment t4) => Image.EqualsIgnoreCase(t1) ? 0 : Image.EqualsIgnoreCase(t2) ? 1 : Image.EqualsIgnoreCase(t3) ? 2 : Image.EqualsIgnoreCase(t4) ? 3 : -1;

        /// <summary>
        ///     Selects the index of the token that matches the image. Returns -1 if no match is found.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <param name="t5"></param>
        /// <returns></returns>
        public int Select(StringSegment t1, StringSegment t2, StringSegment t3, StringSegment t4, StringSegment t5) =>
            Image.EqualsIgnoreCase(t1) ? 0 : Image.EqualsIgnoreCase(t2) ? 1 : Image.EqualsIgnoreCase(t3) ? 2 : Image.EqualsIgnoreCase(t4) ? 3 : Image.EqualsIgnoreCase(t5) ? 4 : -1;

        /// <summary>
        ///     Selects the index of the token that matches the image. Returns -1 if no match is found.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        /// <param name="t5"></param>
        /// <param name="t6"></param>
        /// <returns></returns>
        public int Select(StringSegment t1, StringSegment t2, StringSegment t3, StringSegment t4, StringSegment t5, StringSegment t6) => Image.EqualsIgnoreCase(t1) ? 0 :
            Image.EqualsIgnoreCase(t2) ? 1 :
            Image.EqualsIgnoreCase(t3) ? 2 :
            Image.EqualsIgnoreCase(t4) ? 3 :
            Image.EqualsIgnoreCase(t5) ? 4 :
            Image.EqualsIgnoreCase(t6) ? 5 : -1;

        /// <summary>
        ///     Selects the index of the token that matches the image. Returns -1 if no match is found.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public int Select(params StringSegment[] tokens)
        {
            for (var i = 0; i < tokens.Length; i++)
            {
                if (Image.EqualsIgnoreCase(tokens[i]))
                    return i;
            }

            return -1;
        }
    }
}