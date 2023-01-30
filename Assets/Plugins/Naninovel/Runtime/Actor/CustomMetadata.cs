// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// When inherited by a custom type, serializable fields will be added to <see cref="ActorMetadata"/> for the actors of the specified actor implementation type.
    /// The data will be exposed in the Naninovel's actor editor menus and can be accessed at runtime with <see cref="ActorMetadata.GetCustomData{TData}"/>.
    /// </summary>
    [Serializable]
    public abstract class CustomMetadata<TActor> : CustomMetadata
        where TActor : IActor { }
    
    /// <see cref="CustomMetadata{TActor}"/>
    [Serializable]
    public abstract class CustomMetadata
    {
        internal CustomMetadata () { }
    }
}
