// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System.Collections.Generic;

namespace Mal.OnyxTemplate.DocumentModel
{
    public abstract class ConditionalMacroSection
    {
        public abstract IEnumerable<DocumentBlock> Descendants();
    }
}