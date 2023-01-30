// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to represent an actor on scene.
    /// </summary>
    public interface IActor
    {
        /// <summary>
        /// Unique identifier of the actor. 
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Appearance of the actor. 
        /// </summary>
        string Appearance { get; set; }
        /// <summary>
        /// Whether the actor is currently visible on scene.
        /// </summary>
        bool Visible { get; set; }
        /// <summary>
        /// Position of the actor.
        /// </summary>
        Vector3 Position { get; set; }
        /// <summary>
        /// Rotation of the actor.
        /// </summary>
        Quaternion Rotation { get; set; }
        /// <summary>
        /// Scale of the actor.
        /// </summary>
        Vector3 Scale { get; set; }
        /// <summary>
        /// Tint color of the actor.
        /// </summary>
        Color TintColor { get; set; }

        /// <summary>
        /// Allows to perform an async initialization routine.
        /// Invoked once by <see cref="IActorManager"/> after actor is constructed.
        /// </summary>
        UniTask InitializeAsync ();

        /// <summary>
        /// Changes <see cref="Appearance"/> over specified time using provided animation easing and transition effect.
        /// </summary>
        UniTask ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default, 
            Transition? transition = default, AsyncToken asyncToken = default);
        /// <summary>
        /// Changes <see cref="Visible"/> over specified time using provided animation easing.
        /// </summary>
        UniTask ChangeVisibilityAsync (bool visible, float duration, EasingType easingType = default, AsyncToken asyncToken = default);
        /// <summary>
        /// Changes <see cref="Position"/> over specified time using provided animation easing.
        /// </summary>
        UniTask ChangePositionAsync (Vector3 position, float duration, EasingType easingType = default, AsyncToken asyncToken = default);
        /// <summary>
        /// Changes <see cref="Rotation"/> over specified time using provided animation easing.
        /// </summary>
        UniTask ChangeRotationAsync (Quaternion rotation, float duration, EasingType easingType = default, AsyncToken asyncToken = default);
        /// <summary>
        /// Changes <see cref="Scale"/> factor over specified time using provided animation easing.
        /// </summary>
        UniTask ChangeScaleAsync (Vector3 scale, float duration, EasingType easingType = default, AsyncToken asyncToken = default);
        /// <summary>
        /// Changes <see cref="TintColor"/> over specified time using provided animation easing.
        /// </summary>
        UniTask ChangeTintColorAsync (Color tintColor, float duration, EasingType easingType = default, AsyncToken asyncToken = default);

        /// <summary>
        /// Registers provided object as a holder of the resources associated with the specified actor appearance.
        /// The resources won't be unloaded by <see cref="ReleaseResources"/> while they're held by at least one object.
        /// </summary>
        UniTask HoldResourcesAsync (string appearance, object holder);
        /// <summary>
        /// Removes the provided object from the holders list of the resources associated with the specified actor appearance.
        /// Will unload the resources after the release in case no other objects are holding it.
        /// </summary>
        void ReleaseResources (string appearance, object holder);
    }
}
