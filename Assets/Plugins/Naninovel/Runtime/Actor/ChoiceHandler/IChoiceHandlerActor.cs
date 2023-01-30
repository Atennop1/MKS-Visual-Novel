// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to represent a choice handler actor on scene.
    /// </summary>
    public interface IChoiceHandlerActor : IActor
    {
        /// <summary>
        /// List of the currently available options to choose from,
        /// in the same order the options were added.
        /// </summary>
        List<ChoiceState> Choices { get; }

        /// <summary>
        /// Handler should add an option to choose from.
        /// </summary>
        void AddChoice (ChoiceState choice);
        /// <summary>
        /// Handler should remove a choice option with the provided ID.
        /// </summary>
        void RemoveChoice (string id);
        /// <summary>
        /// Handler should fetch a choice state with the provided ID.
        /// </summary>
        ChoiceState GetChoice (string id);
    } 
}
