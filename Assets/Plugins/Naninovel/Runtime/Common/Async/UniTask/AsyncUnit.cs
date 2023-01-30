// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel.Async
{
    public struct AsyncUnit : IEquatable<AsyncUnit>
    {
        public static readonly AsyncUnit Default = new AsyncUnit();

        public override int GetHashCode ()
        {
            return 0;
        }

        public bool Equals (AsyncUnit other)
        {
            return true;
        }

        public override string ToString ()
        {
            return "()";
        }
    }
}
