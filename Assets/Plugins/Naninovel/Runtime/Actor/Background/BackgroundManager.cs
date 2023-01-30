// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel
{
    /// <inheritdoc cref="IBackgroundManager"/>
    [InitializeAtRuntime]
    public class BackgroundManager : OrthoActorManager<IBackgroundActor, BackgroundState, BackgroundMetadata, BackgroundsConfiguration>, IBackgroundManager
    {
        public BackgroundManager (BackgroundsConfiguration config, CameraConfiguration cameraConfig)
            : base(config, cameraConfig) { }
    }
}
