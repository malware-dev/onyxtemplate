// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

namespace Mal.OnyxTemplate.DocumentModel
{
    enum TokenKind
    {
        None,
        Template,
        If,
        ElseIf,
        Else,
        End,
        ForEach,
        Next,
        Word,
        Dot,
        Colon,
        First,
        Last,
        Middle,
        Odd,
        Even
    }
}