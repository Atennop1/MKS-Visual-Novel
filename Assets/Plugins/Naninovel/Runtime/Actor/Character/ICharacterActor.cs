// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel
{
    /// <summary>
    /// Implementation is able to represent a character actor on scene.
    /// </summary>
    public interface ICharacterActor : IActor
    {
        /// <summary>
        /// Look direction of the character.
        /// </summary>
        CharacterLookDirection LookDirection { get; set; }

        /// <summary>
        /// Changes character look direction over specified time using provided animation easing.
        /// </summary>
        UniTask ChangeLookDirectionAsync (CharacterLookDirection lookDirection, float duration, EasingType easingType = default, AsyncToken asyncToken = default);
    } 
}
