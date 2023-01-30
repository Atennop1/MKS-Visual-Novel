// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

namespace Naninovel
{
    public class MockAddressableBuilder : IAddressableBuilder
    {
        public void RemoveEntries () { }
        public bool TryAddEntry (string assetGuid, string resourcePath) => false;
        public void BuildContent () { }
    }
}
