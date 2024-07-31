using System;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    /// A reference to a field or meta-macro in a document.
    /// </summary>
    public readonly struct DocumentFieldReference: IEquatable<DocumentFieldReference>
    {
        public readonly StringSegment Name;
        public readonly int Up;
        readonly int _hashCode;

        /// <summary>
        /// Creates a new instance of <see cref="DocumentFieldReference"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="up"></param>
        /// <param name="metaMacroKind"></param>
        /// <exception cref="ArgumentException"></exception>
        public DocumentFieldReference(StringSegment name, int up, MetaMacroKind metaMacroKind)
        {
            if (name.IsEmpty)
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            Name = name;
            Up = up;
            MetaMacroKind = metaMacroKind;
            _hashCode = CalcHashCode(name, up, (int)metaMacroKind);
        }

        /// <summary>
        /// Determines if this reference is a meta-macro, and if so, which kind.
        /// </summary>
        public MetaMacroKind MetaMacroKind { get; }

        /// <summary>
        /// Gets a text-pointer to the source of this reference.
        /// </summary>
        public TextPtr Source => new TextPtr(Name.Text, Name.Start);

        /// <summary>
        /// Whether this reference is empty.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty() => Name.IsEmpty;

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool Equals(DocumentFieldReference other) => Name.EqualsIgnoreCase(other.Name) && Up == other.Up && MetaMacroKind == other.MetaMacroKind;

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override int GetHashCode() => _hashCode;
    }
}