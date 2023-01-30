// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to manage cameras and other systems required for scene rendering.
    /// </summary>
    public interface ICameraManager : IEngineService<CameraConfiguration>
    {
        /// <summary>
        /// Main render camera used by the engine.
        /// </summary>
        Camera Camera { get; }
        /// <summary>
        /// Optional camera used for UI rendering.
        /// </summary>
        Camera UICamera { get; }
        /// <summary>
        /// Whether to render the UI.
        /// </summary>
        bool RenderUI { get; set; }
        /// <summary>
        /// Local camera position offset in units by X and Y axis relative to the initial position set in the configuration.
        /// </summary>
        Vector3 Offset { get; set; }
        /// <summary>
        /// Local camera rotation.
        /// </summary>
        Quaternion Rotation { get; set; }
        /// <summary>
        /// Relative camera zoom (orthographic size or FOV depending on <see cref="Orthographic"/>), in 0.0 to 1.0 range.
        /// </summary>
        float Zoom { get; set; }
        /// <summary>
        /// Whether the camera should render in orthographic (true) or perspective (false) mode.
        /// </summary>
        bool Orthographic { get; set; }
        /// <summary>
        /// Current rendering quality level (<see cref="QualitySettings"/>) index.
        /// </summary>
        int QualityLevel { get; set; }

        /// <summary>
        /// Activates/disables camera look mode, when player can offset the main camera with input devices 
        /// (eg, by moving a mouse or using gamepad analog stick).
        /// </summary>
        void SetLookMode (bool enabled, Vector2 lookZone, Vector2 lookSpeed, bool gravity);
        /// <summary>
        /// Save current content of the screen to be used as a thumbnail (eg, for save slots).
        /// </summary>
        Texture2D CaptureThumbnail ();
        /// <summary>
        /// Modifies <see cref="Offset"/> over time.
        /// </summary>
        UniTask ChangeOffsetAsync (Vector3 offset, float duration, EasingType easingType = default, AsyncToken asyncToken = default);
        /// <summary>
        /// Modifies <see cref="Rotation"/> over time.
        /// </summary>
        UniTask ChangeRotationAsync (Quaternion rotation, float duration, EasingType easingType = default, AsyncToken asyncToken = default);
        /// <summary>
        /// Modifies <see cref="Zoom"/> over time.
        /// </summary>
        UniTask ChangeZoomAsync (float zoom, float duration, EasingType easingType = default, AsyncToken asyncToken = default);
    } 
}
