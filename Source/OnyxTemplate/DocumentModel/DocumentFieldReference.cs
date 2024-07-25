using System;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    public readonly struct DocumentFieldReference
    {
        public readonly string Name;
        public readonly int Up;

        public DocumentFieldReference(string name, int up)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Up = up;
        }
        
        public bool IsEmpty() => Name == null;
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{{ ");
            if (Up > 0)
                sb.Append(new string('.', Up));
            sb.Append(Name);
            sb.Append(" }}");
            return sb.ToString();
        }
    }
}