using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    public class SimpleMacroBlock : DocumentBlock
    {
        public SimpleMacroBlock(DocumentFieldReference field, bool indent)
        {
            Field = field;
            Indent = indent;
        }

        public DocumentFieldReference Field { get; }

        public bool Indent { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{{ ");
            sb.Append(Field);
            if (Indent)
                sb.Append(" indent");
            sb.Append(" }}");
            return sb.ToString();
        }
    }
}