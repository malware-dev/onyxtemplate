namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    /// Defines the kind of meta macro.
    /// </summary>
    public enum MetaMacroKind
    {
        /// <summary>
        /// Not a meta-macro.
        /// </summary>
        None,
        
        /// <summary>
        /// A meta-macro that identifies the current item as the first item in a collection.
        /// </summary>
        First,
        
        /// <summary>
        /// A meta-macro that identifies the current item as the last item in a collection.
        /// </summary>
        Last,
        
        /// <summary>
        /// A meta-macro that identifies the current item as a middle item in a collection.
        /// </summary>
        Middle,
        
        /// <summary>
        /// A meta-macro that identifies the current item as an odd item in a collection (its index is an odd number).
        /// </summary>
        Odd,
        
        /// <summary>
        /// A meta-macro that identifies the current item as an even item in a collection (its index is an even number).
        /// </summary>
        Even
    }
}