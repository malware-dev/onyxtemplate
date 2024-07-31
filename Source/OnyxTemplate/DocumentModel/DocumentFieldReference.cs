using System;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    public enum MacroKind
    {
        None,
        First,
        Last,
        Middle,
        Odd,
        Even
    }
    
    public readonly struct DocumentFieldReference: IEquatable<DocumentFieldReference>
    {
        public readonly StringSegment Name;
        public readonly int Up;
        readonly int _hashCode;

        public DocumentFieldReference(StringSegment name, int up, MacroKind macroKind)
        {
            if (name.IsEmpty)
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            Name = name;
            Up = up;
            MacroKind = macroKind;
            _hashCode = CalcHashCode(name, up, (int)macroKind);
        }

        public MacroKind MacroKind { get; }

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

        public bool Equals(DocumentFieldReference other) => Name.EqualsIgnoreCase(other.Name) && Up == other.Up && MacroKind == other.MacroKind;

        public override bool Equals(object obj) => obj is DocumentFieldReference other && Equals(other);

        static int CalcHashCode(StringSegment name, int up, int kind)
        {
            unchecked
            {
                var hashCode = name.GetHashCodeIgnoreCase();
                hashCode = (hashCode * 397) ^ up;
                hashCode = (hashCode * 397) ^ kind;
                return hashCode;
            }
        }

        public override int GetHashCode() => _hashCode;
    }
}