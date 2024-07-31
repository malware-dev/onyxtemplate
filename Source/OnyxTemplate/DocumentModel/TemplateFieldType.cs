namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    /// Defines the type of a template field.
    /// </summary>
    public enum TemplateFieldType
    {
        /// <summary>
        /// Not defined.
        /// </summary>
        None,
        
        /// <summary>
        /// A boolean value.
        /// </summary>
        Boolean,
        
        /// <summary>
        /// A string value.
        /// </summary>
        String,
        
        /// <summary>
        /// A collection of values.
        /// </summary>
        Collection,
        
        /// <summary>
        /// A complex value (a value that has fields of its own).
        /// </summary>
        Complex
    }
}