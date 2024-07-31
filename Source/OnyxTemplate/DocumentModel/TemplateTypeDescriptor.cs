using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     Defines a type that will be used to render a template.
    /// </summary>
    public class TemplateTypeDescriptor
    {
        TemplateTypeDescriptor(Identifier name, ImmutableArray<TemplateFieldDescriptor> fields, ImmutableArray<TemplateTypeDescriptor> complexTypes)
        {
            Name = name;
            Fields = fields;
            ComplexTypes = complexTypes;
        }

        /// <summary>
        ///     The name of the type.
        /// </summary>
        public Identifier Name { get; }

        /// <summary>
        ///     All fields of the type.
        /// </summary>
        public ImmutableArray<TemplateFieldDescriptor> Fields { get; }

        /// <summary>
        ///     All nested complex types, if any.
        /// </summary>
        public ImmutableArray<TemplateTypeDescriptor> ComplexTypes { get; }

        /// <summary>
        ///     A builder for <see cref="TemplateTypeDescriptor" />.
        /// </summary>
        public class Builder
        {
            readonly ImmutableArray<Builder>.Builder _complexTypes = ImmutableArray.CreateBuilder<Builder>();
            readonly ImmutableArray<TemplateFieldDescriptor.Builder>.Builder _properties = ImmutableArray.CreateBuilder<TemplateFieldDescriptor.Builder>();

            /// <summary>
            ///     Creates a new instance of <see cref="Builder" />.
            /// </summary>
            /// <param name="parent"></param>
            public Builder(Builder parent = null)
            {
                Parent = parent;
            }

            /// <summary>
            ///     A reference to the parent builder, if any.
            /// </summary>
            public Builder Parent { get; }

            /// <summary>
            ///     The currently defined name of the type.
            /// </summary>
            public Identifier Name { get; private set; }

            /// <summary>
            ///     Evaluates the parent builder up to the specified number of levels.
            /// </summary>
            /// <param name="levels"></param>
            /// <returns></returns>
            public Builder Up(int levels = 1)
            {
                var parent = this;
                for (var i = 0; i < levels; i++)
                    parent = parent.Parent;
                return parent;
            }

            /// <summary>
            ///     Changes the name of the type.
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public Builder WithName(Identifier name)
            {
                Name = name;
                return this;
            }

            /// <summary>
            ///     Adds or alters a field in the type.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="type"></param>
            /// <param name="configure"></param>
            /// <returns></returns>
            public Builder WithField(StringSegment name, TemplateFieldType type, Action<TemplateFieldDescriptor.Builder> configure = null)
            {
                var identifier = Identifier.MakeSafe(name);

                var field = _properties.FirstOrDefault(p => p.Name == identifier);
                if (field != null)
                {
                    if (type > field.Type)
                        field.WithType(type);
                }
                else
                {
                    field = new TemplateFieldDescriptor.Builder(identifier, type);
                    _properties.Add(field);
                }

                configure?.Invoke(field);
                return this;
            }

            /// <summary>
            ///     Builds the <see cref="TemplateTypeDescriptor" />.
            /// </summary>
            /// <param name="typeResolver"></param>
            /// <returns></returns>
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

            /// <summary>
            ///     Adds a nested complex type to the type.
            /// </summary>
            /// <param name="complexType"></param>
            /// <returns></returns>
            public Builder WithComplexType(Builder complexType)
            {
                if (Parent != null)
                    return Parent.WithComplexType(complexType);
                _complexTypes.Add(complexType);
                return this;
            }

            class TypeResolver : ITypeResolver
            {
                public Dictionary<Identifier, TemplateTypeDescriptor> Types { get; } = new Dictionary<Identifier, TemplateTypeDescriptor>(Identifier.IgnoreCaseComparer);
                public TemplateTypeDescriptor ResolveComplexType(Identifier name) => Types.TryGetValue(name, out var type) ? type : null;
            }
        }
    }
}