using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    public readonly struct DocumentHeader
    {
        readonly bool _isDefined;
        public readonly bool Indent;
        public readonly bool PublicVisibility;

        public DocumentHeader(bool indent, bool publicVisibility)
        {
            _isDefined = true;
            Indent = indent;
            PublicVisibility = publicVisibility;
        }

        public bool IsEmpty() => !_isDefined;

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