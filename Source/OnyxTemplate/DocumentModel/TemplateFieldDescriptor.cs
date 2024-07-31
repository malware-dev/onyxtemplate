using System;

namespace Mal.OnyxTemplate.DocumentModel
{
    public interface ITypeResolver
    {
        TemplateTypeDescriptor ResolveComplexType(Identifier name);
    }

    public class TemplateFieldDescriptor
    {
        readonly Identifier _complexTypeName;
        readonly ITypeResolver _typeResolver;
        TemplateTypeDescriptor _complexType;

        public TemplateFieldDescriptor(Identifier name, TemplateFieldType type, TemplateFieldType elementType, Identifier complexTypeName, ITypeResolver typeResolver)
        {
            _complexTypeName = complexTypeName;
            _typeResolver = typeResolver;
            Name = name;
            Type = type;
            ElementType = elementType;
        }

        public Identifier Name { get; }
        public TemplateFieldType Type { get; }
        public TemplateFieldType ElementType { get; }

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

        public class Builder
        {
            public Builder()
            { }

            public Builder(Identifier name, TemplateFieldType type)
                : this(name, type, TemplateFieldType.None)
            { }

            Builder(Identifier name, TemplateFieldType type, TemplateFieldType elementType)
            {
                Name = name;
                Type = type;
                ElementType = elementType;
            }

            public Identifier Name { get; private set; }

            public TemplateFieldType Type { get; private set; }

            public TemplateFieldType ElementType { get; private set; }

            public Identifier ComplexTypeName { get; private set; }

            // public Builder WithName(StringSegment name)
            // {
            //     Name = Document.CSharpify(name);
            //     return this;
            // }

            public Builder WithType(TemplateFieldType type)
            {
                if (Type == TemplateFieldType.Collection)
                    ElementType = type;
                else
                    Type = type;
                return this;
            }

            public Builder WithType(Identifier complexTypeName)
            {
                ComplexTypeName = complexTypeName;
                if (Type == TemplateFieldType.Collection)
                    ElementType = TemplateFieldType.Complex;
                else
                    Type = TemplateFieldType.Complex;
                return this;
            }

            public Builder AsCollection()
            {
                ElementType = Type;
                Type = TemplateFieldType.Collection;
                return this;
            }

            public TemplateFieldDescriptor Build(ITypeResolver typeResolver)
            {
                if (Type != TemplateFieldType.Complex && ElementType != TemplateFieldType.Complex)
                    return new TemplateFieldDescriptor(Name, Type, ElementType, default, null);
                return new TemplateFieldDescriptor(Name, Type, ElementType, ComplexTypeName, typeResolver);
            }
        }
    }
}