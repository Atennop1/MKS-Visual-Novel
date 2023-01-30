// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

namespace Naninovel
{
    /// <summary>
    /// Represents data required to construct and initialize a <see cref="IChoiceHandlerActor"/>.
    /// </summary>
    [System.Serializable]
    public class ChoiceHandlerMetadata : ActorMetadata
    {
        [System.Serializable]
        public class Map : ActorMetadataMap<ChoiceHandlerMetadata> { }

        public ChoiceHandlerMetadata ()
        {
            Implementation = typeof(UIChoiceHandler).AssemblyQualifiedName;
            Loader = new ResourceLoaderConfiguration { PathPrefix = ChoiceHandlersConfiguration.DefaultPathPrefix };
        }

        public override ActorPose<TState> GetPoseOrNull<TState> (string poseName) => null;
    }
}
