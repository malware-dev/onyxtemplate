// OnyxTemplate
// 
// Copyright 2024 Morten Aune Lyrstad

using System;

namespace Mal.OnyxTemplate
{
    public class DisposableHandle : IDisposable
    {
        Action _dispose;

        public DisposableHandle(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            _dispose?.Invoke();
            _dispose = null;
        }
    }
}