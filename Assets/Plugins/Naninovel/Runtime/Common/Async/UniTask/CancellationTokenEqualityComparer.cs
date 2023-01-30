// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Threading;

namespace Naninovel.Async
{
    public class CancellationTokenEqualityComparer : IEqualityComparer<CancellationToken>
    {
        public static readonly IEqualityComparer<CancellationToken> Default = new CancellationTokenEqualityComparer();

        public bool Equals (CancellationToken x, CancellationToken y)
        {
            return x.Equals(y);
        }

        public int GetHashCode (CancellationToken obj)
        {
            return obj.GetHashCode();
        }
    }
}
