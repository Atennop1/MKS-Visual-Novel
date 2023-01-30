// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Naninovel
{
    [Serializable]
    public struct EditorResource : IEquatable<EditorResource>
    {
        public string Name => name;
        public string PathPrefix => pathPrefix;
        public string Guid => guid;
        public string Path => $"{PathPrefix ?? string.Empty}/{Name ?? string.Empty}";

        [FormerlySerializedAs("Name")]
        [SerializeField] private string name;
        [FormerlySerializedAs("PathPrefix")]
        [SerializeField] private string pathPrefix;
        [FormerlySerializedAs("Guid")]
        [SerializeField] private string guid;

        public EditorResource (string name, string pathPrefix, string guid)
        {
            this.name = name;
            this.pathPrefix = pathPrefix;
            this.guid = guid;
        }
        
        public bool Equals (EditorResource other)
        {
            return name == other.name && pathPrefix == other.pathPrefix && guid == other.guid;
        }

        public override bool Equals (object obj)
        {
            return obj is EditorResource other && Equals(other);
        }

        public override int GetHashCode ()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PathPrefix != null ? PathPrefix.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Guid != null ? Guid.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator == (EditorResource left, EditorResource right)
        {
            return left.Equals(right);
        }

        public static bool operator != (EditorResource left, EditorResource right)
        {
            return !left.Equals(right);
        }
    }
}
