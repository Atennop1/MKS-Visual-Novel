// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable state of a <see cref="IChoiceHandlerActor"/>.
    /// </summary>
    [System.Serializable]
    public class ChoiceHandlerState : ActorState<IChoiceHandlerActor>
    {
        /// <inheritdoc cref="IChoiceHandlerActor.Choices"/>
        public List<ChoiceState> Choices => new List<ChoiceState>(choices);

        [SerializeField] private List<ChoiceState> choices = new List<ChoiceState>();

        public override void OverwriteFromActor (IChoiceHandlerActor actor)
        {
            base.OverwriteFromActor(actor);

            choices.Clear();
            choices.AddRange(actor.Choices);
        }

        public override void ApplyToActor (IChoiceHandlerActor actor)
        {
            base.ApplyToActor(actor);

            foreach (var choice in actor.Choices.ToList())
                if (!choices.Contains(choice))
                    actor.RemoveChoice(choice.Id);

            foreach (var choice in choices)
                if (!actor.Choices.Contains(choice))
                    actor.AddChoice(choice);
        }
    }
}
