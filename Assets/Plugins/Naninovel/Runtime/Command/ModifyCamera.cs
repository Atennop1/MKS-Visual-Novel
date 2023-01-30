// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Modifies the main camera, changing offset, zoom level and rotation over time.
    /// Check [this video](https://youtu.be/zy28jaMss8w) for a quick demonstration of the command effect.
    /// </summary>
    [CommandAlias("camera")]
    public class ModifyCamera : Command
    {
        /// <summary>
        /// Local camera position offset in units by X,Y,Z axes.
        /// </summary>
        public DecimalListParameter Offset;
        /// <summary>
        /// Local camera rotation by Z-axis in angle degrees (0.0 to 360.0 or -180.0 to 180.0).
        /// The same as third component of `rotation` parameter; ignored when `rotation` is specified.
        /// </summary>
        public DecimalParameter Roll;
        /// <summary>
        /// Local camera rotation over X,Y,Z-axes in angle degrees (0.0 to 360.0 or -180.0 to 180.0).
        /// </summary>
        public DecimalListParameter Rotation;
        /// <summary>
        /// Relative camera zoom (orthographic size or field of view, depending on the render mode), in 0.0 (no zoom) to 1.0 (full zoom) range.
        /// </summary>
        public DecimalParameter Zoom;
        /// <summary>
        /// Whether the camera should render in orthographic (true) or perspective (false) mode.
        /// </summary>
        [ParameterAlias("ortho")]
        public BooleanParameter Orthographic;
        /// <summary>
        /// Names of the components to toggle (enable if disabled and vice-versa). The components should be attached to the same game object as the camera.
        /// This can be used to toggle [custom post-processing effects](/guide/special-effects.md#camera-effects).
        /// Use `*` to affect all the components attached to the camera object.
        /// </summary>
        [ParameterAlias("toggle")]
        public StringListParameter ToggleTypeNames;
        /// <summary>
        /// Names of the components to enable or disable. The components should be attached to the same game object as the camera.
        /// This can be used to explicitly enable or disable [custom post-processing effects](/guide/special-effects.md#camera-effects).
        /// Specified components enabled state will override effect of `toggle` parameter.
        /// Use `*` to affect all the components attached to the camera object.
        /// </summary>
        [ParameterAlias("set")]
        public NamedBooleanListParameter SetTypeNames;
        /// <summary>
        /// Name of the easing function to use for the modification.
        /// <br/><br/>
        /// Available options: Linear, SmoothStep, Spring, EaseInQuad, EaseOutQuad, EaseInOutQuad, EaseInCubic, EaseOutCubic, EaseInOutCubic, EaseInQuart, EaseOutQuart, EaseInOutQuart, EaseInQuint, EaseOutQuint, EaseInOutQuint, EaseInSine, EaseOutSine, EaseInOutSine, EaseInExpo, EaseOutExpo, EaseInOutExpo, EaseInCirc, EaseOutCirc, EaseInOutCirc, EaseInBounce, EaseOutBounce, EaseInOutBounce, EaseInBack, EaseOutBack, EaseInOutBack, EaseInElastic, EaseOutElastic, EaseInOutElastic.
        /// <br/><br/>
        /// When not specified, will use a default easing function set in the camera configuration settings.
        /// </summary>
        [ParameterAlias("easing"), ConstantContext(typeof(EasingType))]
        public StringParameter EasingTypeName;
        /// <summary>
        /// Duration (in seconds) of the modification.
        /// </summary>
        [ParameterAlias("time"), ParameterDefaultValue("0.35")]
        public DecimalParameter Duration;

        private const string selectAllSymbol = "*";
        
        private readonly List<MonoBehaviour> componentsCache = new List<MonoBehaviour>();

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var cameraManager = Engine.GetService<ICameraManager>();

            var duration = Assigned(Duration) ? Duration.Value : cameraManager.Configuration.DefaultDuration;
            var easingType = cameraManager.Configuration.DefaultEasing;
            if (Assigned(EasingTypeName) && !Enum.TryParse(EasingTypeName, true, out easingType))
                LogWarningWithPosition($"Failed to parse `{EasingTypeName}` easing.");

            if (Assigned(Orthographic))
                cameraManager.Camera.orthographic = Orthographic;

            if (Assigned(ToggleTypeNames))
                foreach (var name in ToggleTypeNames)
                    DoForComponent(name, c => c.enabled = !c.enabled);

            if (Assigned(SetTypeNames))
                foreach (var kv in SetTypeNames)
                    if (kv.HasValue && !string.IsNullOrWhiteSpace(kv.Name) && kv.NamedValue.HasValue) 
                        DoForComponent(kv.Name, c => c.enabled = kv.Value?.Value ?? false);

            var tasks = new List<UniTask>();

            if (Assigned(Offset)) tasks.Add(cameraManager.ChangeOffsetAsync(ArrayUtils.ToVector3(Offset, Vector3.zero), duration, easingType, asyncToken));
            if (Assigned(Rotation)) tasks.Add(cameraManager.ChangeRotationAsync(Quaternion.Euler(
                Rotation.ElementAtOrDefault(0) ?? cameraManager.Rotation.eulerAngles.x,
                Rotation.ElementAtOrDefault(1) ?? cameraManager.Rotation.eulerAngles.y,
                Rotation.ElementAtOrDefault(2) ?? cameraManager.Rotation.eulerAngles.z), duration, easingType, asyncToken));
            else if (Assigned(Roll)) tasks.Add(cameraManager.ChangeRotationAsync(Quaternion.Euler(
                cameraManager.Rotation.eulerAngles.x,
                cameraManager.Rotation.eulerAngles.y, 
                Roll), duration, easingType, asyncToken));
            if (Assigned(Zoom)) tasks.Add(cameraManager.ChangeZoomAsync(Zoom, duration, easingType, asyncToken));

            await UniTask.WhenAll(tasks);

            void DoForComponent (string componentName, Action<MonoBehaviour> action)
            {
                if (componentName == selectAllSymbol)
                {
                    cameraManager.Camera.gameObject.GetComponents(componentsCache);
                    componentsCache.ForEach(action);
                    return;
                }
                
                var cmp = cameraManager.Camera.gameObject.GetComponent(componentName) as MonoBehaviour;
                if (!cmp)
                {
                    LogWithPosition($"Failed to toggle `{componentName}` camera component; the component is not found on the camera's game object.");
                    return;
                }
                action.Invoke(cmp);
            }
        }
    }
}
