// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Adds a selection box to the decorated <see cref="Vector3"/> field, allowing to select between
    /// world-space and Naninovel scene-space (relative to <see cref="CameraConfiguration.ReferenceResolution"/>) position modes.
    /// </summary>
    public class ScenePositionAttribute : PropertyAttribute { }
}
