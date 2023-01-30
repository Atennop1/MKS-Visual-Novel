// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents data required to construct and initialize a <see cref="IBackgroundActor"/>.
    /// </summary>
    [System.Serializable]
    public class BackgroundMetadata : OrthoActorMetadata
    {
        [System.Serializable]
        public class Map : ActorMetadataMap<BackgroundMetadata> { }
        [System.Serializable]
        public class Pose : ActorPose<BackgroundState> { }

        [Tooltip("Controls the mode in which background actor is matched to the screen when aspect ratios are different:" +
                 "\n • Crop — Crop the background to ensure no `black bars` are visible." +
                 "\n • Fit — Fit the background to the screen. The whole background will always be visible, but `black bars` will appear." +
                 "\n • Custom — Match either width or height with a custom ratio. The ratio is controlled with `Custom Match Ratio` property." +
                 "\n • Disable — Don't perform any matching.")]
        public CameraMatchMode MatchMode = CameraMatchMode.Crop;
        [Tooltip("When `Match Mode` is set to `Custom`, controls the match ratio. Minimum (0) value will match width and ignore height, maximum (1) — vice-versa."), Range(0, 1)]
        public float CustomMatchRatio = 0;
        [Tooltip("Named states (poses) of the background; pose name can be used as appearance in `@back` commands to set enabled properties of the associated state.")]
        public List<Pose> Poses = new List<Pose>();

        public BackgroundMetadata ()
        {
            Implementation = typeof(SpriteBackground).AssemblyQualifiedName;
            Loader = new ResourceLoaderConfiguration { PathPrefix = BackgroundsConfiguration.DefaultPathPrefix };
            Pivot = new Vector2(.5f, .5f);
        }

        public override ActorPose<TState> GetPoseOrNull<TState> (string poseName) => Poses.FirstOrDefault(p => p.Name == poseName) as ActorPose<TState>;
    }
}
