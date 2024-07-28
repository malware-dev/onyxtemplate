using System;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    public readonly struct DocumentFieldReference
    {
        public readonly StringSegment Name;
        public readonly int Up;

        public DocumentFieldReference(StringSegment name, int up, bool isMacroReference)
        {
            if (name.IsEmpty)
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            Name = name;
            Up = up;
            IsMacroReference = isMacroReference;
        }

        public bool IsMacroReference { get; }

        public TextPtr Source => new TextPtr(Name.Text, Name.Start);

        public bool IsEmpty() => Name.IsEmpty;

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