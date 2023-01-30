// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Thrown upon cancellation of an async operation due to Unity object being destroyed.
    /// </summary>
    public class AsyncOperationDestroyedException : AsyncOperationCanceledException
    {
        public AsyncOperationDestroyedException (UnityEngine.Object obj)
        {
            if (ObjectUtils.IsValid(obj)) throw new ArgumentException("Provided object is not destroyed.", nameof(obj));
        }
    }
}
