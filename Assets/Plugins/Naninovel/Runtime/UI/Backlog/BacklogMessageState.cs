// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    [Serializable]
    public struct BacklogMessageState : IEquatable<BacklogMessageState>
    {
        public string MessageText => messageText;
        public string ActorNameText => actorNameText;
        public IReadOnlyCollection<string> VoiceClipNames => voiceClipNames;
        public PlaybackSpot RollbackSpot => rollbackSpot;

        [SerializeField] private string messageText;
        [SerializeField] private string actorNameText;
        [SerializeField] private string[] voiceClipNames;
        [SerializeField] private PlaybackSpot rollbackSpot;

        public BacklogMessageState (string messageText, string actorNameText, 
            IEnumerable<string> voiceClipNames, PlaybackSpot rollbackSpot)
        {
            this.messageText = messageText;
            this.actorNameText = actorNameText;
            this.voiceClipNames = voiceClipNames?.ToArray();
            this.rollbackSpot = rollbackSpot;
        }
        
        public bool Equals (BacklogMessageState other)
        {
            return messageText == other.messageText && actorNameText == other.actorNameText && Equals(voiceClipNames, other.voiceClipNames) && rollbackSpot.Equals(other.rollbackSpot);
        }

        public override bool Equals (object obj)
        {
            return obj is BacklogMessageState other && Equals(other);
        }

        public override int GetHashCode ()
        {
            unchecked
            {
                var hashCode = (MessageText != null ? MessageText.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ActorNameText != null ? ActorNameText.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (VoiceClipNames != null ? VoiceClipNames.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RollbackSpot.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator == (BacklogMessageState left, BacklogMessageState right)
        {
            return left.Equals(right);
        }

        public static bool operator != (BacklogMessageState left, BacklogMessageState right)
        {
            return !left.Equals(right);
        }
    }
}
