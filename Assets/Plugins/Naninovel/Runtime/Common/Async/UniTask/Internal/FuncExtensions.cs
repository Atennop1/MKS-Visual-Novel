// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel.Async.Internal
{
    internal static class FuncExtensions
    {
        // avoid lambda capture

        internal static Action<T> AsFuncOfT<T> (this Action action)
        {
            return action.Invoke;
        }

        private static void Invoke<T> (this Action action, T unused)
        {
            action();
        }
    }
}
