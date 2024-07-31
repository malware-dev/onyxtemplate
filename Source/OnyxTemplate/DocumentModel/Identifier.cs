// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;
using System.Collections.Generic;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     Represents an identifier.
    /// </summary>
    public readonly struct Identifier : IEquatable<Identifier>, IComparable<Identifier>, IComparable
    {
        public static bool operator ==(Identifier left, Identifier right) => left.Equals(right);
        public static bool operator !=(Identifier left, Identifier right) => !left.Equals(right);
        public static bool operator <(Identifier left, Identifier right) => left.CompareTo(right) < 0;
        public static bool operator >(Identifier left, Identifier right) => left.CompareTo(right) > 0;
        public static bool operator <=(Identifier left, Identifier right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Identifier left, Identifier right) => left.CompareTo(right) >= 0;

        /// <summary>
        ///     A comparer that compares identifiers case-insensitively.
        /// </summary>
        public static readonly IdentifierIgnoreCaseComparer IgnoreCaseComparer = new IdentifierIgnoreCaseComparer();

        /// <summary>
        ///     A comparer that compares identifiers case-sensitively.
        /// </summary>
        public static readonly IdentifierComparer Comparer = new IdentifierComparer();

        /// <summary>
        ///     An undefined identifier.
        /// </summary>
        public static readonly Identifier Undefined = new Identifier();

        /// <summary>
        ///     Creates a new identifier from a string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="camelCase"></param>
        /// <returns></returns>
        public static Identifier MakeSafe(string input, bool camelCase = false) => MakeSafe(string.IsNullOrEmpty(input) ? default : new StringSegment(input), camelCase);

        /// <summary>
        ///     Creates a new identifier from a string segment.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="camelCase"></param>
        /// <returns></returns>
        public static Identifier MakeSafe(StringSegment input, bool camelCase = false)
        {
            if (input.IsEmpty)
                return new Identifier("_");

            var buffer = new StringBuilder();
            var firstChar = input[0];

            if (camelCase)
                buffer.Append(char.IsDigit(firstChar) ? '_' : char.ToLower(firstChar));
            else
                buffer.Append(char.IsDigit(firstChar) ? '_' : char.ToUpper(firstChar));

            for (var i = 1; i < input.Length; i++)
            {
                var c = input[i];

                if (char.IsLetterOrDigit(c))
                    buffer.Append(c);
                else
                {
                    switch (c)
                    {
                        case 'æ':
                            buffer.Append("ae");
                            break;
                        case 'ø':
                            buffer.Append("oe");
                            break;
                        case 'å':
                            buffer.Append("aa");
                            break;
                        case 'ü':
                            buffer.Append("ue");
                            break;
                        case 'ö':
                            buffer.Append("oe");
                            break;
                        case 'ä':
                            buffer.Append("ae");
                            break;
                        case 'ß':
                            buffer.Append("ss");
                            break;
                        default:
                            buffer.Append('_');
                            break;
                    }
                }
            }

            return new Identifier(buffer.ToString());
        }

        readonly string _value;

        /// <summary>
        ///     Creates a new instance of <see cref="Identifier" />.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException"></exception>
        public Identifier(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));
            _value = value;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Identifier" />.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException"></exception>
        public Identifier(StringSegment value)
        {
            if (value.IsEmpty)
                throw new ArgumentException("Value cannot be empty.", nameof(value));
            _value = value.ToString();
        }

        /// <summary>
        ///     Whether this identifier is defined.
        /// </summary>
        /// <returns></returns>
        public bool IsDefined() => _value != null;

        /// <summary>
        ///     Converts this identifier to PascalCase.
        /// </summary>
        /// <returns></returns>
        public Identifier AsPascalCase()
        {
            var value = _value;
            if (value.Length == 0)
                return this;
            if (char.IsUpper(value[0]))
                return this;
            return new Identifier(char.ToUpper(value[0]) + value.Substring(1));
        }

        /// <summary>
        ///     Converts this identifier to camelCase.
        /// </summary>
        /// <returns></returns>
        public Identifier AsCamelCase()
        {
            var value = _value;
            if (value.Length == 0)
                return this;
            if (char.IsLower(value[0]))
                return this;
            return new Identifier(char.ToLower(value[0]) + value.Substring(1));
        }

        /// <summary>
        ///     Adds a prefix to this identifier.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public Identifier WithPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                return this;
            return new Identifier(prefix + _value);
        }

        /// <inheritdoc />
        public bool Equals(Identifier other) => _value == other._value;

        /// <summary>
        ///     Determines if this identifier is equal to another identifier, ignoring case.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EqualsIgnoreCase(Identifier other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Identifier other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => _value != null ? _value.GetHashCode() : 0;

        /// <summary>
        ///     Gets the hash code of this identifier, ignoring case.
        /// </summary>
        /// <returns></returns>
        public int GetHashCodeIgnoreCase() => _value != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(_value) : 0;

        /// <inheritdoc />
        public override string ToString() => _value ?? string.Empty;

        /// <summary>
        ///     Converts this identifier to a string segment.
        /// </summary>
        /// <returns></returns>
        public StringSegment ToStringSegment()
        {
            if (_value == null)
                return default;
            return new StringSegment(_value);
        }

        /// <inheritdoc />
        public int CompareTo(Identifier other) => string.Compare(_value, other._value, StringComparison.Ordinal);

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            return obj is Identifier other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Identifier)}");
        }

        /// <summary>
        ///     A comparer that compares identifiers case-insensitively.
        /// </summary>
        public class IdentifierIgnoreCaseComparer : IEqualityComparer<Identifier>
        {
            /// <inheritdoc />
            public bool Equals(Identifier x, Identifier y) => x.EqualsIgnoreCase(y);

            /// <inheritdoc />
            public int GetHashCode(Identifier obj) => obj.GetHashCodeIgnoreCase();
        }

        /// <summary>
        ///     A comparer that compares identifiers case-sensitively.
        /// </summary>
        public class IdentifierComparer : IEqualityComparer<Identifier>
        {
            /// <inheritdoc />
            public bool Equals(Identifier x, Identifier y) => x.Equals(y);

            /// <inheritdoc />
            public int GetHashCode(Identifier obj) => obj.GetHashCode();
        }
    }
}