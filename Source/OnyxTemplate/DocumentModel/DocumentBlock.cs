// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;
using System.Collections;
using System.Collections.Generic;

namespace Mal.OnyxTemplate.DocumentModel
{
    public abstract class DocumentBlock
    {
        public abstract IEnumerable<DocumentBlock> Descendants();
    }
}