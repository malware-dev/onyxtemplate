using System;

namespace Mal.OnyxTemplate.DocumentModel
{
    public interface ITypeResolver
    {
        TemplateTypeDescriptor ResolveComplexType(string name);
    }

    public class TemplateFieldDescriptor
    {
        readonly string _complexTypeName;
        readonly ITypeResolver _typeResolver;
        TemplateTypeDescriptor _complexType;

        public TemplateFieldDescriptor(string name, TemplateFieldType type, TemplateFieldType elementType, string complexTypeName, ITypeResolver typeResolver)
        {
            _complexTypeName = complexTypeName;
            _typeResolver = typeResolver;
            name = Document.CSharpify(name);
            Name = name;
            Type = type;
            ElementType = elementType;
        }

        public string Name { get; }
        public TemplateFieldType Type { get; }
        public TemplateFieldType ElementType { get; }

        public TemplateTypeDescriptor ComplexType
        {
            get
            {
                if (_complexType != null)
                    return _complexType;
                if (_complexTypeName == null)
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

            public Builder(string name, TemplateFieldType type)
                : this(name, type, TemplateFieldType.None)
            { }

            Builder(string name, TemplateFieldType type, TemplateFieldType elementType)
            {
                name = Document.CSharpify(name);
                Name = name;
                Type = type;
                ElementType = elementType;
            }

            public string Name { get; private set; }

            public TemplateFieldType Type { get; private set; }

            public TemplateFieldType ElementType { get; private set; }

            public string ComplexTypeName { get; private set; }

            public Builder WithName(string name)
            {
                name = Document.CSharpify(name);
                Name = name;
                return this;
            }

            public Builder WithType(TemplateFieldType type)
            {
                if (Type == TemplateFieldType.Collection)
                    ElementType = type;
                else
                    Type = type;
                return this;
            }

            public Builder WithType(string complexTypeName)
            {
                complexTypeName = Document.CSharpify(complexTypeName);
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
                    return new TemplateFieldDescriptor(Name, Type, ElementType, null, null);
                return new TemplateFieldDescriptor(Name, Type, ElementType, ComplexTypeName, typeResolver);
            }
        }
    }
}