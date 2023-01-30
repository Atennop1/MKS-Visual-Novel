// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections;

namespace Naninovel
{
    // UnityEngine Bridges.

    public readonly partial struct UniTask
    {
        public static IEnumerator ToCoroutine (Func<UniTask> taskFactory)
        {
            return taskFactory().ToCoroutine();
        }
    }
}
