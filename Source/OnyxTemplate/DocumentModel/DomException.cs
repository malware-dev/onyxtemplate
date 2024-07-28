// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;

namespace Mal.OnyxTemplate.DocumentModel
{
    public class DomException : Exception
    {
        public TextPtr Ptr { get; }
        public int Length { get; }

        public DomException(TextPtr ptr, int length, string message) : base(message)
        {
            Ptr = ptr;
            Length = length;
        }
    }
}