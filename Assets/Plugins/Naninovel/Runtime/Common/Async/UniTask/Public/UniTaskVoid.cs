// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

#pragma warning disable CS0436

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Naninovel.Async.CompilerServices;

namespace Naninovel
{
    [AsyncMethodBuilder(typeof(AsyncUniTaskVoidMethodBuilder))]
    public struct UniTaskVoid
    {
        public void Forget () { }

        [DebuggerHidden]
        public Awaiter GetAwaiter ()
        {
            return new Awaiter();
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            [DebuggerHidden]
            public bool IsCompleted => true;

            [DebuggerHidden]
            public void GetResult ()
            {
                UnityEngine.Debug.LogWarning("UniTaskVoid can't await, always fire-and-forget. use Forget instead of await.");
            }

            [DebuggerHidden]
            public void OnCompleted (Action continuation) { }

            [DebuggerHidden]
            public void UnsafeOnCompleted (Action continuation) { }
        }
    }
}
