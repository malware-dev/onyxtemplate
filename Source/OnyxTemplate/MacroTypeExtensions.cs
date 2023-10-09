// OnyxTemplate
// 
// Copyright 2023 Morten A. Lyrstad

using System;

namespace Mal.OnyxTemplate
{
    /// <summary>
    ///     Provides utility functions for <see cref="MacroType" />.
    /// </summary>
    static class MacroTypeExtensions
    {
        /// <summary>
        /// Whether this is the start of a scope.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsStartOfScope(this MacroType type)
        {
            switch (type)
            {
                case MacroType.ForEach:
                case MacroType.If:
                case MacroType.ElseIf:
                case MacroType.Else:
                    return true;
                
                case MacroType.Root:
                case MacroType.Header:
                case MacroType.Text:
                case MacroType.Ref:
                case MacroType.Next:
                case MacroType.End:
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Whether this is the end of a previously started scope.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEndOfScope(this MacroType type)
        {
            switch (type)
            {
                case MacroType.Next:
                case MacroType.ElseIf:
                case MacroType.Else:
                case MacroType.End:
                    return true;

                case MacroType.Root:
                case MacroType.Header:
                case MacroType.Text:
                case MacroType.Ref:
                case MacroType.ForEach:
                case MacroType.If:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether this is the end of a block (the $end after an $if, for example)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBlockStart(this MacroType type)
        {
            switch (type)
            {
                case MacroType.If:
                case MacroType.ElseIf:
                case MacroType.Else:
                    return true;

                case MacroType.Root:
                case MacroType.Header:
                case MacroType.Text:
                case MacroType.Ref:
                case MacroType.ForEach:
                case MacroType.Next:
                case MacroType.End:
                default:
                    return false;
            }
        }
    }
}