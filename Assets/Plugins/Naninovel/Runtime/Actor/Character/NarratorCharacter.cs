// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="ICharacterActor"/> implementation, which doesn't have any presence on scene
    /// and can be used to represent a narrator (author of the printed text messages).
    /// </summary>
    [ActorResources(null, false)]
    public class NarratorCharacter : ICharacterActor
    {
        public string Id { get; }
        public string Appearance { get; set; }
        public bool Visible { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Color TintColor { get; set; }
        public CharacterLookDirection LookDirection { get; set; }

        public NarratorCharacter (string id, CharacterMetadata metadata)
        {
            Id = id;
        }
        
        public UniTask InitializeAsync () => UniTask.CompletedTask;

        public UniTask ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default, 
            Transition? transition = default, AsyncToken asyncToken = default) => UniTask.CompletedTask;

        public UniTask ChangeVisibilityAsync (bool visible, float duration, EasingType easingType = default, 
            AsyncToken asyncToken = default) => UniTask.CompletedTask;

        public UniTask ChangePositionAsync (Vector3 position, float duration, EasingType easingType = default, 
            AsyncToken asyncToken = default) => UniTask.CompletedTask;

        public UniTask ChangeRotationAsync (Quaternion rotation, float duration, EasingType easingType = default, 
            AsyncToken asyncToken = default) => UniTask.CompletedTask;

        public UniTask ChangeScaleAsync (Vector3 scale, float duration, EasingType easingType = default, 
            AsyncToken asyncToken = default) => UniTask.CompletedTask;

        public UniTask ChangeTintColorAsync (Color tintColor, float duration, EasingType easingType = default, 
            AsyncToken asyncToken = default) => UniTask.CompletedTask;
        
        public UniTask ChangeLookDirectionAsync (CharacterLookDirection lookDirection, float duration, EasingType easingType = default, 
            AsyncToken asyncToken = default) => UniTask.CompletedTask;

        public UniTask HoldResourcesAsync (string appearance, object holder) => UniTask.CompletedTask;

        public void ReleaseResources (string appearance, object holder) { }
    }
}
