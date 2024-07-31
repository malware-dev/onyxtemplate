// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;

namespace Mal.OnyxTemplate.DocumentModel
{
    /// <summary>
    ///     An exception that occurs during the parsing of a document.
    /// </summary>
    public class DomException : Exception
    {
        /// <summary>
        ///     Creates a new instance of <see cref="DomException" />.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="length"></param>
        /// <param name="message"></param>
        public DomException(TextPtr ptr, int length, string message) : base(message)
        {
            Ptr = ptr;
            Length = length;
        }

        /// <summary>
        ///     The pointer to the text where the exception occurred.
        /// </summary>
        public TextPtr Ptr { get; }

        /// <summary>
        ///     The length of the text where the exception occurred.
        /// </summary>
        public int Length { get; }
    }
}