// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;
using System.Collections.Generic;
using System.Text;

namespace Mal.OnyxTemplate.DocumentModel
{
    public readonly struct Identifier : IEquatable<Identifier>, IComparable<Identifier>, IComparable
    {
        public static bool operator ==(Identifier left, Identifier right) => left.Equals(right);
        public static bool operator !=(Identifier left, Identifier right) => !left.Equals(right);
        public static bool operator <(Identifier left, Identifier right) => left.CompareTo(right) < 0;
        public static bool operator >(Identifier left, Identifier right) => left.CompareTo(right) > 0;
        public static bool operator <=(Identifier left, Identifier right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Identifier left, Identifier right) => left.CompareTo(right) >= 0;

        public static readonly IdentifierIgnoreCaseComparer IgnoreCaseComparer = new IdentifierIgnoreCaseComparer();

        public static readonly IdentifierComparer Comparer = new IdentifierComparer();

        public static readonly Identifier Undefined = new Identifier();

        public static Identifier MakeSafe(string input, bool camelCase = false) => MakeSafe(string.IsNullOrEmpty(input) ? default : new StringSegment(input), camelCase);

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

        public Identifier(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));
            _value = value;
        }

        public Identifier(StringSegment value)
        {
            if (value.IsEmpty)
                throw new ArgumentException("Value cannot be empty.", nameof(value));
            _value = value.ToString();
        }

        public bool IsDefined() => _value != null;

        public Identifier AsPascalCase()
        {
            var value = _value;
            if (value.Length == 0)
                return this;
            if (char.IsUpper(value[0]))
                return this;
            return new Identifier(char.ToUpper(value[0]) + value.Substring(1));
        }

        public Identifier AsCamelCase()
        {
            var value = _value;
            if (value.Length == 0)
                return this;
            if (char.IsLower(value[0]))
                return this;
            return new Identifier(char.ToLower(value[0]) + value.Substring(1));
        }

        public Identifier WithPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                return this;
            return new Identifier(prefix + _value);
        }

        public bool Equals(Identifier other) => _value == other._value;

        public bool EqualsIgnoreCase(Identifier other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) => obj is Identifier other && Equals(other);

        public override int GetHashCode() => _value != null ? _value.GetHashCode() : 0;

        public int GetHashCodeIgnoreCase() => _value != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(_value) : 0;

        public override string ToString() => _value ?? string.Empty;
        
        public StringSegment ToStringSegment()
        {
            if (_value == null)
                return default;
            return new StringSegment(_value);
        }

        public int CompareTo(Identifier other) => string.Compare(_value, other._value, StringComparison.Ordinal);

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            return obj is Identifier other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Identifier)}");
        }

        public class IdentifierIgnoreCaseComparer : IEqualityComparer<Identifier>
        {
            public bool Equals(Identifier x, Identifier y) => x.EqualsIgnoreCase(y);

            public int GetHashCode(Identifier obj) => obj.GetHashCodeIgnoreCase();
        }

        public class IdentifierComparer : IEqualityComparer<Identifier>
        {
            public bool Equals(Identifier x, Identifier y) => x.Equals(y);

            public int GetHashCode(Identifier obj) => obj.GetHashCode();
        }
    }
}