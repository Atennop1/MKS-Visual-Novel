// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to manage <see cref="ICharacterActor"/> actors.
    /// </summary>
    public interface ICharacterManager : IActorManager<ICharacterActor, CharacterState, CharacterMetadata, CharactersConfiguration>
    {
        /// <summary>
        /// Invoked when avatar texture of a managed character is changed.
        /// </summary>
        event Action<CharacterAvatarChangedArgs> OnCharacterAvatarChanged;

        /// <summary>
        /// Checks whether avatar texture with the provided (local) path exists.
        /// </summary>
        bool AvatarTextureExists (string avatarTexturePath);
        /// <summary>
        /// Un-assigns avatar texture from a character with the provided ID.
        /// </summary>
        void RemoveAvatarTextureFor (string characterId);
        /// <summary>
        /// Attempts to retrieve currently assigned avatar texture for a character with the provided ID.
        /// Will return null when character is not found or doesn't have an avatar texture assigned.
        /// </summary>
        Texture2D GetAvatarTextureFor (string characterId);
        /// <summary>
        /// Attempts to retrieve a (local) path of the currently assigned avatar texture for a character with the provided ID.
        /// Will return null when character is not found or doesn't have an avatar texture assigned.
        /// </summary>
        string GetAvatarTexturePathFor (string characterId);
        /// <summary>
        /// Assigns avatar texture with the provided (local) path to a character with the provided ID.
        /// </summary>
        void SetAvatarTexturePathFor (string characterId, string avatarTexturePath);
        /// <summary>
        /// Attempts to find a display name for character with provided ID.
        /// Will return null when not found.
        /// </summary>
        string GetDisplayName (string characterId);
        /// <summary>
        /// Given character x position, returns a look direction to the scene origin.
        /// </summary>
        CharacterLookDirection LookAtOriginDirection (float xPos);
        /// <summary>
        /// Evenly distribute visible controlled characters positions over specified time.
        /// </summary>
        /// <param name="lookAtOrigin">Whether to also make the characters look at the scene origin.</param>
        /// <param name="duration">Duration of the arrangement animation, in seconds.</param>
        /// <param name="easingType">Easing type to use for the arrangement animation.</param>
        UniTask ArrangeCharactersAsync (bool lookAtOrigin = true, float duration = 0, EasingType easingType = default, AsyncToken asyncToken = default);
    }
}
