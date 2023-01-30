// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    [Serializable]
    public struct ChatMessageState : IEquatable<ChatMessageState>
    {
        public string PrintedText => printedText;
        public string AuthorId => authorId;

        [SerializeField] private string printedText;
        [SerializeField] private string authorId;

        public ChatMessageState (string printedText, string authorId)
        {
            this.printedText = printedText;
            this.authorId = authorId;
        }
        
        public bool Equals (ChatMessageState other)
        {
            return printedText == other.printedText && authorId == other.authorId;
        }

        public override bool Equals (object obj)
        {
            return obj is ChatMessageState other && Equals(other);
        }

        public override int GetHashCode ()
        {
            unchecked
            {
                return ((PrintedText != null ? PrintedText.GetHashCode() : 0) * 397) ^ (AuthorId != null ? AuthorId.GetHashCode() : 0);
            }
        }

        public static bool operator == (ChatMessageState left, ChatMessageState right)
        {
            return left.Equals(right);
        }

        public static bool operator != (ChatMessageState left, ChatMessageState right)
        {
            return !left.Equals(right);
        }
    }
}
