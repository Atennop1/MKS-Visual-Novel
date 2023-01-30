// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Provides extension methods for <see cref="IActor"/>.
    /// </summary>
    public static class ActorExtensions
    {
        /// <summary>
        /// Changes <see cref="IActor.Position"/> over X-axis over specified time using provided animation easing.
        /// </summary>
        public static async UniTask ChangePositionXAsync (this IActor actor, float posX, float duration, EasingType easingType = default, AsyncToken asyncToken = default) 
            => await actor.ChangePositionAsync(new Vector3(posX, actor.Position.y, actor.Position.z), duration, easingType, asyncToken);
        /// <summary>
        /// Changes <see cref="IActor.Position"/> over Y-axis over specified time using provided animation easing.
        /// </summary>
        public static async UniTask ChangePositionYAsync (this IActor actor, float posY, float duration, EasingType easingType = default, AsyncToken asyncToken = default) 
            => await actor.ChangePositionAsync(new Vector3(actor.Position.x, posY, actor.Position.z), duration, easingType, asyncToken);
        /// <summary>
        /// Changes <see cref="IActor.Position"/> over Z-axis over specified time using provided animation easing.
        /// </summary>
        public static async UniTask ChangePositionZAsync (this IActor actor, float posZ, float duration, EasingType easingType = default, AsyncToken asyncToken = default) 
            => await actor.ChangePositionAsync(new Vector3(actor.Position.x, actor.Position.y, posZ), duration, easingType, asyncToken);

        /// <summary>
        /// Changes <see cref="IActor.Rotation"/> over X-axis (Euler angle) over specified time using provided animation easing.
        /// </summary>
        public static async UniTask ChangeRotationXAsync (this IActor actor, float rotX, float duration, EasingType easingType = default, AsyncToken asyncToken = default) 
            => await actor.ChangeRotationAsync(Quaternion.Euler(rotX, actor.Rotation.eulerAngles.y, actor.Rotation.eulerAngles.z), duration, easingType, asyncToken);
        /// <summary>
        /// Changes <see cref="IActor.Rotation"/> over Y-axis (Euler angle) over specified time using provided animation easing.
        /// </summary>
        public static async UniTask ChangeRotationYAsync (this IActor actor, float rotY, float duration, EasingType easingType = default, AsyncToken asyncToken = default) 
            => await actor.ChangeRotationAsync(Quaternion.Euler(actor.Rotation.eulerAngles.x, rotY, actor.Rotation.eulerAngles.z), duration, easingType, asyncToken);
        /// <summary>
        /// Changes <see cref="IActor.Rotation"/> over Z-axis (Euler angle) over specified time using provided animation easing.
        /// </summary>
        public static async UniTask ChangeRotationZAsync (this IActor actor, float rotZ, float duration, EasingType easingType = default, AsyncToken asyncToken = default) 
            => await actor.ChangeRotationAsync(Quaternion.Euler(actor.Rotation.eulerAngles.x, actor.Rotation.eulerAngles.y, rotZ), duration, easingType, asyncToken);

        /// <summary>
        /// Changes <see cref="IActor.Scale"/> over X-axis over specified time using provided animation easing.
        /// </summary>
        public static async UniTask ChangeScaleXAsync (this IActor actor, float scaleX, float duration, EasingType easingType = default, AsyncToken asyncToken = default) 
            => await actor.ChangeScaleAsync(new Vector3(scaleX, actor.Scale.y, actor.Scale.z), duration, easingType, asyncToken);
        /// <summary>
        /// Changes <see cref="IActor.Scale"/> over Y-axis over specified time using provided animation easing.
        /// </summary>
        public static async UniTask ChangeScaleYAsync (this IActor actor, float scaleY, float duration, EasingType easingType = default, AsyncToken asyncToken = default) 
            => await actor.ChangeScaleAsync(new Vector3(actor.Scale.x, scaleY, actor.Scale.z), duration, easingType, asyncToken);
        /// <summary>
        /// Changes <see cref="IActor.Scale"/> over Z-axis over specified time using provided animation easing.
        /// </summary>
        public static async UniTask ChangeScaleZAsync (this IActor actor, float scaleZ, float duration, EasingType easingType = default, AsyncToken asyncToken = default) 
            => await actor.ChangeScaleAsync(new Vector3(actor.Scale.x, actor.Scale.y, scaleZ), duration, easingType, asyncToken);

        /// <summary>
        /// Changes <see cref="IActor.Position"/> over X-axis.
        /// </summary>
        public static void ChangePositionX (this IActor actor, float posX) => actor.Position = new Vector3(posX, actor.Position.y, actor.Position.z);
        /// <summary>
        /// Changes <see cref="IActor.Position"/> over Y-axis.
        /// </summary>
        public static void ChangePositionY (this IActor actor, float posY) => actor.Position = new Vector3(actor.Position.x, posY, actor.Position.z);
        /// <summary>
        /// Changes <see cref="IActor.Position"/> over Z-axis.
        /// </summary>
        public static void ChangePositionZ (this IActor actor, float posZ) => actor.Position = new Vector3(actor.Position.x, actor.Position.y, posZ);

        /// <summary>
        /// Changes <see cref="IActor.Rotation"/> over X-axis (Euler angle).
        /// </summary>
        public static void ChangeRotationX (this IActor actor, float rotX) => actor.Rotation = Quaternion.Euler(rotX, actor.Rotation.eulerAngles.y, actor.Rotation.eulerAngles.z);
        /// <summary>
        /// Changes <see cref="IActor.Rotation"/> over Y-axis (Euler angle).
        /// </summary>
        public static void ChangeRotationY (this IActor actor, float rotY) => actor.Rotation = Quaternion.Euler(actor.Rotation.eulerAngles.x, rotY, actor.Rotation.eulerAngles.z);
        /// <summary>
        /// Changes <see cref="IActor.Rotation"/> over Z-axis (Euler angle).
        /// </summary>
        public static void ChangeRotationZ (this IActor actor, float rotZ) => actor.Rotation = Quaternion.Euler(actor.Rotation.eulerAngles.x, actor.Rotation.eulerAngles.y, rotZ);

        /// <summary>
        /// Changes <see cref="IActor.Scale"/> over X-axis.
        /// </summary>
        public static void ChangeScaleX (this IActor actor, float scaleX) => actor.Scale = new Vector3(scaleX, actor.Scale.y, actor.Scale.z);
        /// <summary>
        /// Changes <see cref="IActor.Scale"/> over Y-axis.
        /// </summary>
        public static void ChangeScaleY (this IActor actor, float scaleY) => actor.Scale = new Vector3(actor.Scale.x, scaleY, actor.Scale.z);
        /// <summary>
        /// Changes <see cref="IActor.Scale"/> over Z-axis.
        /// </summary>
        public static void ChangeScaleZ (this IActor actor, float scaleZ) => actor.Scale = new Vector3(actor.Scale.x, actor.Scale.y, scaleZ);
    } 
}
