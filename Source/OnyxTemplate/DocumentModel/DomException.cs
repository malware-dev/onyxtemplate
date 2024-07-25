// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;

namespace Mal.OnyxTemplate.DocumentModel
{
    public class DomException : Exception
    {
        internal DomException(TextPtr ptr, string message) : base(message) { }
    }
}