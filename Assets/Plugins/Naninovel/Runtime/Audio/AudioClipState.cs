// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    [Serializable]
    public struct AudioClipState : IEquatable<AudioClipState>
    {
        public string Path => path; 
        public float Volume => volume; 
        public bool Looped => looped;

        [SerializeField] private string path;
        [SerializeField] private float volume;
        [SerializeField] private bool looped;

        public AudioClipState (string path, float volume, bool looped)
        {
            this.path = path;
            this.volume = volume;
            this.looped = looped;
        }
        
        public bool Equals (AudioClipState other) => path == other.path;
        public override bool Equals (object obj) => obj is AudioClipState other && Equals(other);
        public override int GetHashCode () => Path != null ? Path.GetHashCode() : 0;
        public static bool operator == (AudioClipState left, AudioClipState right) => left.Equals(right);
        public static bool operator != (AudioClipState left, AudioClipState right) => !left.Equals(right);
    }
}
