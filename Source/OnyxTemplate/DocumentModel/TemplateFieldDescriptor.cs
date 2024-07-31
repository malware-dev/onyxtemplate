using System;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     Describes a field in a <see cref="TemplateTypeDescriptor" />.
    /// </summary>
    public class TemplateFieldDescriptor
    {
        readonly Identifier _complexTypeName;
        readonly ITypeResolver _typeResolver;
        TemplateTypeDescriptor _complexType;

        /// <summary>
        ///     Creates a new instance of <see cref="TemplateFieldDescriptor" />.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="elementType"></param>
        /// <param name="complexTypeName"></param>
        /// <param name="typeResolver"></param>
        public TemplateFieldDescriptor(Identifier name, TemplateFieldType type, TemplateFieldType elementType, Identifier complexTypeName, ITypeResolver typeResolver)
        {
            _complexTypeName = complexTypeName;
            _typeResolver = typeResolver;
            Name = name;
            Type = type;
            ElementType = elementType;
        }

        /// <summary>
        ///     The name of the field.
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        ///     The type of the field.
        /// </summary>
        public TemplateFieldType Type { get; }

        /// <summary>
        ///     The type of the elements in the field, if <see cref="Type" /> is <see cref="TemplateFieldType.Collection" />.
        /// </summary>
        public TemplateFieldType ElementType { get; }

        /// <summary>
        ///     If <see cref="Type" /> or <see cref="ElementType" /> is <see cref="TemplateFieldType.Complex" />, this property
        ///     will contain the complex type descriptor.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public TemplateTypeDescriptor ComplexType
        {
            get
            {
                if (Type != TemplateFieldType.Complex && Type != TemplateFieldType.Collection)
                    return null;
                if (_complexType != null)
                    return _complexType;
                if (!_complexTypeName.IsDefined())
                    return null;
                _complexType = _typeResolver.ResolveComplexType(_complexTypeName);
                if (_complexType == null)
                    throw new InvalidOperationException($"Complex type '{_complexTypeName}' not found.");
                return _complexType;
            }
        }

        /// <summary>
        ///     A builder for creating instances of <see cref="TemplateFieldDescriptor" />.
        /// </summary>
        public class Builder
        {
            /// <summary>
            ///     Creates a new instance of <see cref="Builder" />.
            /// </summary>
            public Builder()
            { }

            /// <summary>
            ///     Creates a new instance of <see cref="Builder" />.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="type"></param>
            public Builder(Identifier name, TemplateFieldType type)
                : this(name, type, TemplateFieldType.None)
            { }

            Builder(Identifier name, TemplateFieldType type, TemplateFieldType elementType)
            {
                Name = name;
                Type = type;
                ElementType = elementType;
            }

            /// <summary>
            ///     The currently defined name of the field.
            /// </summary>
            public Identifier Name { get; private set; }

            /// <summary>
            ///     The currently defined type of the field.
            /// </summary>
            public TemplateFieldType Type { get; private set; }

            /// <summary>
            ///     The currently defined type of the elements in the field, if <see cref="Type" /> is
            ///     <see cref="TemplateFieldType.Collection" />.
            /// </summary>
            public TemplateFieldType ElementType { get; private set; }

            /// <summary>
            ///     The currently defined identifier of the complex type, if <see cref="Type" /> or <see cref="ElementType" /> is
            ///     <see cref="TemplateFieldType.Complex" />.
            /// </summary>
            public Identifier ComplexTypeName { get; private set; }

            /// <summary>
            ///     Changes the name of the field.
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public Builder WithName(Identifier name)
            {
                Name = name;
                return this;
            }

            /// <summary>
            ///     Changes the type of the field.
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public Builder WithType(TemplateFieldType type)
            {
                if (Type == TemplateFieldType.Collection)
                    ElementType = type;
                else
                    Type = type;
                return this;
            }

            /// <summary>
            ///     Changes the type of the field to <see cref="TemplateFieldType.Complex" />, and sets the complex type name.
            /// </summary>
            /// <param name="complexTypeName"></param>
            /// <returns></returns>
            public Builder WithType(Identifier complexTypeName)
            {
                ComplexTypeName = complexTypeName;
                if (Type == TemplateFieldType.Collection)
                    ElementType = TemplateFieldType.Complex;
                else
                    Type = TemplateFieldType.Complex;
                return this;
            }

            /// <summary>
            ///     Changes the type of the field to <see cref="TemplateFieldType.Collection" />.
            /// </summary>
            /// <returns></returns>
            public Builder AsCollection()
            {
                ElementType = Type;
                Type = TemplateFieldType.Collection;
                return this;
            }

            /// <summary>
            ///     Builds the <see cref="TemplateFieldDescriptor" />.
            /// </summary>
            /// <param name="typeResolver"></param>
            /// <returns></returns>
            public TemplateFieldDescriptor Build(ITypeResolver typeResolver)
            {
                if (Type != TemplateFieldType.Complex && ElementType != TemplateFieldType.Complex)
                    return new TemplateFieldDescriptor(Name, Type, ElementType, default, null);
                return new TemplateFieldDescriptor(Name, Type, ElementType, ComplexTypeName, typeResolver);
            }
        }
    }
}