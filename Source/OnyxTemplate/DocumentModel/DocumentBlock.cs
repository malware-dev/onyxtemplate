// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System.Collections.Generic;

namespace Mal.OnyxTemplate.DocumentModel
{
    public abstract class DocumentBlock
    {
        public abstract IEnumerable<DocumentBlock> Descendants();
        public abstract bool NeedsMacroState();
    }
}