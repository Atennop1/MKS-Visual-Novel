// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Linq;

namespace Naninovel.Commands
{
    /// <summary>
    /// Removes all the choice options in the choice handler with the provided ID (or in default one, when ID is not specified; 
    /// or in all the existing handlers, when `*` is specified as ID) and (optionally) hides it (them).
    /// </summary>
    [CommandAlias("clearChoice")]
    public class ClearChoiceHandler : Command
    {
        /// <summary>
        /// ID of the choice handler to clear. Will use a default handler if not provided.
        /// Specify `*` to clear all the existing handlers.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), ActorContext(ChoiceHandlersConfiguration.DefaultPathPrefix)]
        public StringParameter HandlerId;
        /// <summary>
        /// Whether to also hide the affected choice handlers.
        /// </summary>
        [ParameterDefaultValue("true")]
        public BooleanParameter Hide = true;

        public override UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var choiceManager = Engine.GetService<IChoiceHandlerManager>();

            if (Assigned(HandlerId) && HandlerId == "*")
            {
                foreach (var handler in choiceManager.GetAllActors())
                {
                    RemoveAllChoices(handler);
                    if (Hide) handler.Visible = false;
                }
                return UniTask.CompletedTask;
            }

            var handlerId = Assigned(HandlerId) ? HandlerId.Value : choiceManager.Configuration.DefaultHandlerId;
            if (!choiceManager.ActorExists(handlerId))
            {
                LogWarningWithPosition($"Failed to clear `{handlerId}` choice handler: handler actor with the provided ID doesn't exist.");
                return UniTask.CompletedTask;
            }

            var choiceHandler = choiceManager.GetActor(handlerId);
            RemoveAllChoices(choiceHandler);
            if (Hide) choiceHandler.Visible = false;
            return UniTask.CompletedTask;
        }

        private static void RemoveAllChoices (IChoiceHandlerActor choiceHandler)
        {
            foreach (var choiceState in choiceHandler.Choices.ToList())
                choiceHandler.RemoveChoice(choiceState.Id);
        }
    }
}
