// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Arranges specified characters by X-axis.
    /// When no parameters provided, will execute an auto-arrange evenly distributing visible characters by X-axis.
    /// </summary>
    [CommandAlias("arrange")]
    public class ArrangeCharacters : Command
    {
        /// <summary>
        /// A collection of character ID to scene X-axis position (relative to the left scene border, in percents) named values.
        /// Position 0 relates to the left border and 100 to the right border of the scene; 50 is the center.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), ActorContext(CharactersConfiguration.DefaultPathPrefix, 0)]
        public NamedDecimalListParameter CharacterPositions;
        /// <summary>
        /// When performing auto-arrange, controls whether to also make the characters look at the scene origin (enabled by default).
        /// </summary>
        [ParameterAlias("look"), ParameterDefaultValue("true")]
        public BooleanParameter LookAtOrigin = true;
        /// <summary>
        /// Duration (in seconds) of the arrangement animation.
        /// </summary>
        [ParameterAlias("time"), ParameterDefaultValue("0.35")]
        public DecimalParameter Duration;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var manager = Engine.GetService<ICharacterManager>();
            var duration = Assigned(Duration) ? Duration.Value : manager.ActorManagerConfiguration.DefaultDuration;
            var easing = manager.ActorManagerConfiguration.DefaultEasing;

            // When positions are not specified execute auto arrange.
            if (!Assigned(CharacterPositions))
            {
                await manager.ArrangeCharactersAsync(LookAtOrigin, duration, easing, asyncToken);
                return;
            }

            var actors = manager.GetAllActors().ToList();
            var arrangeTasks = new List<UniTask>();

            foreach (var actorPos in CharacterPositions)
            {
                if (!actorPos.HasValue) continue;

                var actor = actors.Find(a => a.Id.EqualsFastIgnoreCase(actorPos.Name));
                var posX = actorPos.NamedValue / 100f; // Implementation is expecting local scene pos, not percents.
                if (actor is null)
                {
                    LogWarningWithPosition($"Actor '{actorPos.Name}' not found while executing arranging task.");
                    continue;
                }
                var newPosX = Engine.GetConfiguration<CameraConfiguration>().SceneToWorldSpace(new Vector2(posX, 0)).x;
                var newDir = manager.LookAtOriginDirection(newPosX);
                arrangeTasks.Add(actor.ChangeLookDirectionAsync(newDir, duration, easing, asyncToken));
                arrangeTasks.Add(actor.ChangePositionXAsync(newPosX, duration, easing, asyncToken));
            }

            // Sorting by z in order of declaration (first is bottom).
            var declaredActorIds = CharacterPositions
                .Where(a => !string.IsNullOrEmpty(a?.Value?.Name))
                .Select(a => a.Name).ToList();
            declaredActorIds.Reverse();
            for (int i = 0; i < declaredActorIds.Count - 1; i++)
            {
                var currentActor = actors.Find(a => a.Id.EqualsFastIgnoreCase(declaredActorIds[i]));
                var nextActor = actors.Find(a => a.Id.EqualsFastIgnoreCase(declaredActorIds[i + 1]));
                if (currentActor is null || nextActor is null) continue;

                if (currentActor.Position.z > nextActor.Position.z)
                {
                    var lowerZPos = nextActor.Position.z;
                    var higherZPos = currentActor.Position.z;

                    nextActor.ChangePositionZ(higherZPos);
                    currentActor.ChangePositionZ(lowerZPos);
                }
            }

            await UniTask.WhenAll(arrangeTasks);
        }
    }
}
