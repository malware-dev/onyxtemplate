namespace Mal.OnyxTemplate
{
    /// <summary>
    ///     Contains information about a line and character in a text document.
    /// </summary>
    public readonly struct LineInfo
    {
        /// <summary>
        ///     What line the character is on.
        /// </summary>
        public readonly int Line;

        /// <summary>
        ///     What character in the line. Does not evaluate tabs.
        /// </summary>
        public readonly int Char;

        /// <summary>
        ///     Creates a new instance of <see cref="LineInfo" />.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="char"></param>
        public LineInfo(int line, int @char)
        {
            Line = line;
            Char = @char;
        }
    }
}