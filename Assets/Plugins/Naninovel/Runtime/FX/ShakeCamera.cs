// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.FX
{
    /// <summary>
    /// Shakes the main Naninovel render camera.
    /// </summary>
    public class ShakeCamera : ShakeTransform
    {
        protected override Transform GetShakenTransform ()
        {
            var cameraManager = Engine.GetService<ICameraManager>().Camera;
            if (cameraManager == null || !cameraManager.transform.parent) return null;
            return cameraManager.transform.parent;
        }
    }
}
