using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Mal.OnyxTemplate.DocumentModel
{
    public class TemplateTypeDescriptor
    {
        TemplateTypeDescriptor(string name, ImmutableArray<TemplateFieldDescriptor> fields, ImmutableArray<TemplateTypeDescriptor> complexTypes)
        {
            Name = name;
            Fields = fields;
            ComplexTypes = complexTypes;
        }

        public string Name { get; }

        public ImmutableArray<TemplateFieldDescriptor> Fields { get; }
        public ImmutableArray<TemplateTypeDescriptor> ComplexTypes { get; }

        public class Builder
        {
            readonly ImmutableArray<Builder>.Builder _complexTypes = ImmutableArray.CreateBuilder<Builder>();
            readonly ImmutableArray<TemplateFieldDescriptor.Builder>.Builder _properties = ImmutableArray.CreateBuilder<TemplateFieldDescriptor.Builder>();

            public Builder(Builder parent = null)
            {
                Parent = parent;
            }

            public Builder Parent { get; }

            public string Name { get; private set; }

            public Builder Up(int levels = 1)
            {
                var parent = this;
                for (var i = 0; i < levels; i++)
                    parent = parent.Parent;
                return parent;
            }

            public Builder WithName(string name)
            {
                name = Document.CSharpify(name);
                Name = name;
                return this;
            }

            public Builder WithField(string name, TemplateFieldType type, Action<TemplateFieldDescriptor.Builder> configure = null)
            {
                name = Document.CSharpify(name);

                var field = _properties.FirstOrDefault(p => p.Name == name);
                if (field != null)
                {
                    if (type > field.Type)
                        field.WithType(type);
                }
                else
                {
                    field = new TemplateFieldDescriptor.Builder(name, type);
                    _properties.Add(field);
                }

                configure?.Invoke(field);
                return this;
            }

            public TemplateTypeDescriptor Build(ITypeResolver typeResolver = null)
            {
                var isMyTypeResolver = typeResolver == null;
                if (isMyTypeResolver)
                    typeResolver = new TypeResolver();
                ImmutableArray<TemplateTypeDescriptor> complexTypes;
                if (Parent != null)
                    complexTypes = ImmutableArray<TemplateTypeDescriptor>.Empty;
                else
                    complexTypes = _complexTypes.Select(c => c.Build(typeResolver)).OrderBy(c => c.Name).ToImmutableArray();
                var fields = _properties.Select(p => p.Build(typeResolver)).OrderBy(p => p.Name).ToImmutableArray();

                if (isMyTypeResolver)
                {
                    foreach (var complexType in _complexTypes)
                        ((TypeResolver)typeResolver).Types[complexType.Name] = complexType.Build(typeResolver);
                }

                return new TemplateTypeDescriptor(Name, fields, complexTypes);
            }

            public Builder WithComplexType(Builder complexType)
            {
                if (Parent != null)
                    return Parent.WithComplexType(complexType);
                _complexTypes.Add(complexType);
                return this;
            }

            class TypeResolver : ITypeResolver
            {
                public Dictionary<string, TemplateTypeDescriptor> Types { get; } = new Dictionary<string, TemplateTypeDescriptor>(StringComparer.OrdinalIgnoreCase);
                public TemplateTypeDescriptor ResolveComplexType(string name) => Types.TryGetValue(name, out var type) ? type : null;
            }
        }
    }
}