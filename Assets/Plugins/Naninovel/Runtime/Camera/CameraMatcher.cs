// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Threading;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Allows scaling an arbitrary value to match current camera aspect ratio.
    /// </summary>
    public abstract class CameraMatcher : IDisposable
    {
        /// <summary>
        /// Current match mode.
        /// </summary>
        public CameraMatchMode MatchMode { get; set; }
        /// <summary>
        /// Current match ratio to use in case <see cref="CameraMatchMode.Custom"/> mode is active.
        /// </summary>
        public float CustomMatchRatio { get; set; }
        /// <summary>
        /// Whether the matcher is currently working.
        /// </summary>
        public bool Matching => cts != null;

        private readonly UnityEngine.Object target;
        private readonly ICameraManager cameraManager;

        private CancellationTokenSource cts;
        private Vector2 previousCameraSize;
        private Vector2 previousReferenceSize;

        protected CameraMatcher (ICameraManager cameraManager, UnityEngine.Object target)
        {
            this.cameraManager = cameraManager;
            this.target = target;
        }

        /// <summary>
        /// Begins the matching process.
        /// </summary>
        public virtual void Start ()
        {
            if (Matching) return;
            if (!target) throw new InvalidOperationException("Match target is not valid.");
            cts = new CancellationTokenSource();
            MatchInLoopAsync(cts.Token).Forget();
        }

        /// <summary>
        /// Stops the matching process.
        /// </summary>
        public virtual void Stop ()
        {
            if (!Matching) return;
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }

        public void Dispose ()
        {
            cts?.Dispose();
        }

        /// <summary>
        /// Whether matching should be performed at this time.
        /// </summary>
        protected virtual bool IsMatchRequired (Vector2 cameraSize, Vector2 referenceSize)
        {
            return cameraSize != previousCameraSize || referenceSize != previousReferenceSize;
        }

        /// <summary>
        /// Attempts to evaluate current reference sizes for matching.
        /// Returns false in case it's not possible to evaluate the size.
        /// </summary>
        protected abstract bool TryGetReferenceSize (out Vector2 referenceSize);

        /// <summary>
        /// Attempts to evaluate current camera size for matching.
        /// Returns false in case it's not possible to evaluate the size.
        /// </summary>
        protected virtual bool TryGetCameraSize (out Vector2 cameraSize)
        {
            cameraSize = default;
            if (!cameraManager?.Camera || !cameraManager.Camera.orthographic) return false;
            var cameraHeight = cameraManager.Configuration.SceneRect.height;
            var cameraWidth = cameraHeight * cameraManager.Camera.aspect;
            cameraSize = new Vector2(cameraWidth, cameraHeight);
            return true;
        }

        /// <summary>
        /// Invoked when reference size should be scaled to match current camera size.
        /// </summary>
        /// <param name="scaleFactor">Modifier to apply for the reference size.</param>
        protected abstract void ApplyScale (float scaleFactor);

        /// <summary>
        /// Performs the matching for the specified sizes.
        /// </summary>
        protected virtual void Match (Vector2 cameraSize, Vector2 referenceSize)
        {
            var scaleFactor = EvaluateScaleFactor(cameraSize, referenceSize);
            ApplyScale(scaleFactor);
            previousCameraSize = cameraSize;
            previousReferenceSize = referenceSize;
        }

        /// <summary>
        /// Calculates scale factor for the specified sizes.
        /// </summary>
        protected virtual float EvaluateScaleFactor (Vector2 cameraSize, Vector2 referenceSize)
        {
            switch (MatchMode)
            {
                case CameraMatchMode.Crop:
                    return Mathf.Max(cameraSize.x / referenceSize.x, cameraSize.y / referenceSize.y);
                case CameraMatchMode.Fit:
                    return Mathf.Min(cameraSize.x / referenceSize.x, cameraSize.y / referenceSize.y);
                case CameraMatchMode.Custom:
                    const float logBase = 2f;
                    var logWidth = Mathf.Log(cameraSize.x / referenceSize.x, logBase);
                    var logHeight = Mathf.Log(cameraSize.y / referenceSize.y, logBase);
                    var logWeightedAverage = Mathf.Lerp(logWidth, logHeight, CustomMatchRatio);
                    return Mathf.Pow(logBase, logWeightedAverage);
                case CameraMatchMode.Disable:
                    throw new InvalidOperationException("Shouldn't run matchers when matching is disabled.");
                default:
                    throw new FormatException($"Unsupported match mode: `{MatchMode}`.");
            }
        }

        private async UniTaskVoid MatchInLoopAsync (CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && target)
            {
                if (TryGetCameraSize(out var cameraSize) &&
                    TryGetReferenceSize(out var referenceSize) &&
                    IsMatchRequired(cameraSize, referenceSize))
                    Match(cameraSize, referenceSize);
                await AsyncUtils.WaitEndOfFrameAsync();
            }
        }
    }
}
