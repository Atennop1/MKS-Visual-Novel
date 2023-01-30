// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel
{
    /// <summary>
    /// Implementation is able to manage <see cref="IBackgroundActor"/> actors.
    /// </summary>
    public interface IBackgroundManager : IActorManager<IBackgroundActor, BackgroundState, BackgroundMetadata, BackgroundsConfiguration>
    {

    }
}
