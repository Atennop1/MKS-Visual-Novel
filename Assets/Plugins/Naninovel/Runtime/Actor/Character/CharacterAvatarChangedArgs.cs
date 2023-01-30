// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Arguments associated with the <see cref="ICharacterManager.OnCharacterAvatarChanged"/> event.
    /// </summary>
    public class CharacterAvatarChangedArgs : EventArgs
    {
        /// <summary>
        /// ID of the character for which the avatar texture has changed.
        /// </summary>
        public readonly string CharacterId;
        /// <summary>
        /// The new avatar texture of the character.
        /// </summary>
        public readonly Texture2D AvatarTexture;

        public CharacterAvatarChangedArgs (string characterId, Texture2D avatarTexture)
        {
            CharacterId = characterId;
            AvatarTexture = avatarTexture;
        }
    }
}