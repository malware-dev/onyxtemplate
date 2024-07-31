using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    /// Represents the header of a <see cref="Document"/>, and contains information about how
    /// a template should be generated from said document.
    /// </summary>
    public readonly struct DocumentHeader
    {
        readonly bool _isDefined;
        
        /// <summary>
        /// Whether the template should be auto-indented.
        /// </summary>
        public readonly bool Indent;
        
        /// <summary>
        /// Whether the template should be public, rather than the default internal.
        /// </summary>
        public readonly bool PublicVisibility;
        
        /// <summary>
        /// A description of the template.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Creates a new instance of <see cref="DocumentHeader"/>.
        /// </summary>
        /// <param name="indent"></param>
        /// <param name="publicVisibility"></param>
        /// <param name="description"></param>
        public DocumentHeader(bool indent, bool publicVisibility, string description)
        {
            Description = description;
            _isDefined = true;
            Indent = indent;
            PublicVisibility = publicVisibility;
        }

        /// <summary>
        /// Determines if this header is empty.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty() => !_isDefined;

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{{ $template");
            if (PublicVisibility)
                sb.Append(" public");
            if (Indent)
                sb.Append(" indent");
            sb.Append(" }}");
            return sb.ToString();
        }
    }
}