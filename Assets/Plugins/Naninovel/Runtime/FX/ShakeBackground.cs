// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.FX
{
    /// <summary>
    /// Shakes a <see cref="IBackgroundActor"/> or the main one.
    /// </summary>
    public class ShakeBackground : ShakeTransform
    {
        protected override Transform GetShakenTransform ()
        {
            var id = string.IsNullOrEmpty(ObjectName) ? BackgroundsConfiguration.MainActorId : ObjectName;
            var go = GameObject.Find(id);
            return ObjectUtils.IsValid(go) ? go.transform : null;
        }
    }
}
