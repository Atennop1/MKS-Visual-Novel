// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

namespace Naninovel
{
    public interface IAddressableBuilder
    {
        void RemoveEntries ();
        bool TryAddEntry (string assetGuid, string resourcePath);
        void BuildContent ();
    }
}
