namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     A service that can resolve complex types.
    /// </summary>
    public interface ITypeResolver
    {
        /// <summary>
        ///     Resolves a complex type by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        TemplateTypeDescriptor ResolveComplexType(Identifier name);
    }
}