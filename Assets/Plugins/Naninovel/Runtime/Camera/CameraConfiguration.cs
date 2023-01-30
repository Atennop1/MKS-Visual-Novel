// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    [EditInProjectSettings]
    public class CameraConfiguration : Configuration
    {
        [Tooltip("The reference resolution is used to evaluate proper rendering dimensions, so that actors are correctly positioned on scene. As a rule of thumb, set this equal to the resolution of the background textures you make for the game.")]
        public Vector2Int ReferenceResolution = new Vector2Int(1920, 1080);
        [Tooltip("How many pixels correspond to a scene unit. Reducing this will make all the actors appear smaller and vice-versa. Default value of 100 is recommended for most cases.")]
        public int ReferencePPU = 100;
        [Tooltip("Whether reference scene rectangle width should be matched against screen width. When enabled, relative (scene) position evaluation will use screen border as the origin; otherwise reference resolution is used.")]
        public bool MatchScreenWidth = false;
        [Tooltip("Initial world position of the managed cameras.")]
        public Vector3 InitialPosition = new Vector3(0, 0, -10);
        [Tooltip("A prefab with a camera component to use for rendering. Will use a default one when not specified. In case you wish to set some camera properties (background color, FOV, HDR, etc) or add post-processing scripts, create a prefab with the desired camera setup and assign the prefab to this field.")]
        public Camera CustomCameraPrefab = null;
        [Tooltip("Whether to render the UI in a separate camera. This will allow to use individual configuration for the main and UI cameras and prevent post-processing (image) effects from affecting the UI at the cost of a slight rendering overhead.")]
        public bool UseUICamera = true;
        [Tooltip("A prefab with a camera component to use for UI rendering. Will use a default one when not specified. Has no effect when `Use UI Camera` is disabled")]
        public Camera CustomUICameraPrefab = null;
        [Tooltip("Default duration (in seconds) for all the camera modifications (changing zoom, position, rotation, etc).")]
        public float DefaultDuration = .35f;
        [Tooltip("Easing function to use by default for all the camera modifications (changing zoom, position, rotation, etc).")]
        public EasingType DefaultEasing = EasingType.Linear;

        [Header("Thumbnails")]
        [Tooltip("The resolution in which thumbnails to preview game save slots will be captured.")]
        public Vector2Int ThumbnailResolution = new Vector2Int(240, 140);
        [Tooltip("Whether to ignore UI layer when capturing thumbnails.")]
        public bool HideUIInThumbnails = false;

        /// <summary>
        /// Rectangle representing reference dimensions of the scene.
        /// </summary>
        public Rect SceneRect => EvaluateSceneRect();

        /// <summary>
        /// Returns a rectangle that frames the scene content, in units.
        /// </summary>
        /// <param name="center">Position of the scene origin in world space.</param>
        /// <param name="resolution">Reference resolution.</param>
        /// <param name="ppu">Reference pixels per unit.</param>
        public static Rect EvaluateSceneRect (Vector2 center, Vector2 resolution, int ppu)
        {
            var size = resolution / ppu;
            var position = center - size / 2;
            return new Rect(position, size);
        }
        
        /// <summary>
        /// Converts provided relative scene position into world space based on the provided scene rect.
        /// </summary>
        /// <param name="scenePosition">x0y0 is at the bottom left and x1y1 is at the top right corner of the scene.</param>
        /// <param name="sceneRect">Rectangle, representing scene dimensions and position, in world-space units.</param>
        public static Vector3 SceneToWorldSpace (Vector3 scenePosition, Rect sceneRect)
        {
            var resultXY = sceneRect.min + Vector2.Scale(scenePosition, sceneRect.size);
            return new Vector3(resultXY.x, resultXY.y, scenePosition.z);
        }

        /// <summary>
        /// Inverse to <see cref="SceneToWorldSpace(Vector3,Rect)"/>.
        /// </summary>
        public static Vector3 WorldToSceneSpace (Vector3 worldPosition, Rect sceneRect)
        {
            var resultXY = new Vector2 {
                x = (worldPosition.x - sceneRect.min.x) / sceneRect.size.x,
                y = (worldPosition.y - sceneRect.min.y) / sceneRect.size.y
            };
            return new Vector3(resultXY.x, resultXY.y, worldPosition.z);
        }

        /// <inheritdoc cref="SceneToWorldSpace(UnityEngine.Vector3,UnityEngine.Rect)"/>
        public virtual Vector3 SceneToWorldSpace (Vector3 scenePosition) => SceneToWorldSpace(scenePosition, SceneRect);
        
        /// <inheritdoc cref="WorldToSceneSpace(UnityEngine.Vector3,UnityEngine.Rect)"/>
        public virtual Vector3 WorldToSceneSpace (Vector3 worldPosition) => WorldToSceneSpace(worldPosition, SceneRect);
        
        /// <inheritdoc cref="SceneRect"/>
        protected virtual Rect EvaluateSceneRect ()
        {
            var resolution = Vector2.zero; 
            if (MatchScreenWidth)
            {
                var modifier = ReferenceResolution.y / (float)Screen.height;
                var width = Screen.width * modifier;
                resolution = new Vector2(width, ReferenceResolution.y);
            }
            else resolution = ReferenceResolution;
            return EvaluateSceneRect(InitialPosition, resolution, ReferencePPU);
        }
    }
}
