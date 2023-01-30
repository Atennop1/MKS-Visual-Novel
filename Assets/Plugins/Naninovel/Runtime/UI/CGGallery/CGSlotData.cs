// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel.Runtime.UI
{
    public readonly struct CGSlotData : IEquatable<CGSlotData>
    {
        public readonly string Id;
        public readonly IReadOnlyList<string> TexturePaths;
        public readonly IResourceLoader<Texture2D> TextureLoader;

        public CGSlotData (string id, IEnumerable<string> texturePaths, IResourceLoader<Texture2D> textureLoader)
        {
            Id = id;
            TexturePaths = texturePaths.ToArray();
            TextureLoader = textureLoader;
        }

        public bool Equals (CGSlotData other) => Id == other.Id;
        public override bool Equals (object obj) => obj is CGSlotData other && Equals(other);
        public override int GetHashCode () => Id.GetHashCode();
    }
}
