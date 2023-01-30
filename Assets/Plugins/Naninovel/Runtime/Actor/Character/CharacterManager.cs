// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <inheritdoc cref="ICharacterManager"/>
    [InitializeAtRuntime]
    public class CharacterManager : OrthoActorManager<ICharacterActor, CharacterState, CharacterMetadata, CharactersConfiguration>, ICharacterManager
    {
        [Serializable]
        public new class GameState
        {
            public SerializableLiteralStringMap CharIdToAvatarPathMap = new SerializableLiteralStringMap();
        }

        public event Action<CharacterAvatarChangedArgs> OnCharacterAvatarChanged;

        private readonly ITextManager textManager;
        private readonly ILocalizationManager localizationManager;
        private readonly ITextPrinterManager textPrinterManager;
        private readonly SerializableLiteralStringMap charIdToAvatarPathMap = new SerializableLiteralStringMap();
        
        private ResourceLoader<Texture2D> avatarTextureLoader;

        public CharacterManager (CharactersConfiguration config, CameraConfiguration cameraConfig, ITextManager textManager, 
            ILocalizationManager localizationManager, ITextPrinterManager textPrinterManager)
            : base(config, cameraConfig)
        {
            this.textManager = textManager;
            this.localizationManager = localizationManager;
            this.textPrinterManager = textPrinterManager;
        }

        public override void ResetService ()
        {
            base.ResetService();

            charIdToAvatarPathMap.Clear();
        }

        public override async UniTask InitializeServiceAsync ()
        {
            await base.InitializeServiceAsync();

            var providerManager = Engine.GetService<IResourceProviderManager>();
            avatarTextureLoader = Configuration.AvatarLoader.CreateFor<Texture2D>(providerManager);

            textPrinterManager.OnPrintTextStarted += HandleAuthorHighlighting;

            // Loading only the required avatar resources is not possible, as we can't use async to provide them later.
            // In case of heavy usage of the avatar resources, consider using `render character to texture` feature instead.
            await avatarTextureLoader.LoadAndHoldAllAsync(this);
        }

        public override void DestroyService ()
        {
            base.DestroyService();

            if (textPrinterManager != null)
                textPrinterManager.OnPrintTextStarted -= HandleAuthorHighlighting;

            avatarTextureLoader?.ReleaseAll(this);
        }

        public override void SaveServiceState (GameStateMap stateMap)
        {
            base.SaveServiceState(stateMap);

            var gameState = new GameState {
                CharIdToAvatarPathMap = new SerializableLiteralStringMap(charIdToAvatarPathMap)
            };
            stateMap.SetState(gameState);
        }

        public override async UniTask LoadServiceStateAsync (GameStateMap stateMap)
        {
            await base.LoadServiceStateAsync(stateMap);

            var state = stateMap.GetState<GameState>();
            if (state is null)
            {
                if (charIdToAvatarPathMap.Count > 0)
                    foreach (var charId in charIdToAvatarPathMap.Keys.ToArray())
                        RemoveAvatarTextureFor(charId);
                return;
            }

            // Remove non-existing avatar mappings.
            if (charIdToAvatarPathMap.Count > 0)
                foreach (var charId in charIdToAvatarPathMap.Keys.ToArray())
                    if (!state.CharIdToAvatarPathMap.ContainsKey(charId))
                        RemoveAvatarTextureFor(charId);
            // Add new or changed avatar mappings.
            foreach (var kv in state.CharIdToAvatarPathMap)
                SetAvatarTexturePathFor(kv.Key, kv.Value);
        }

        public virtual bool AvatarTextureExists (string avatarTexturePath) => avatarTextureLoader.IsLoaded(avatarTexturePath);

        public virtual void RemoveAvatarTextureFor (string characterId)
        {
            if (!charIdToAvatarPathMap.ContainsKey(characterId)) return;

            charIdToAvatarPathMap.Remove(characterId);
            OnCharacterAvatarChanged?.Invoke(new CharacterAvatarChangedArgs(characterId, null));
        }

        public virtual Texture2D GetAvatarTextureFor (string characterId)
        {
            var avatarTexturePath = GetAvatarTexturePathFor(characterId);
            if (avatarTexturePath is null) return null;
            return avatarTextureLoader.GetLoadedOrNull(avatarTexturePath);
        }

        public virtual string GetAvatarTexturePathFor (string characterId)
        {
            if (!charIdToAvatarPathMap.TryGetValue(characterId ?? string.Empty, out var avatarTexturePath))
            {
                var defaultPath = $"{characterId}/Default"; // Attempt default path.
                return AvatarTextureExists(defaultPath) ? defaultPath : null;
            }
            if (!AvatarTextureExists(avatarTexturePath)) return null;
            return avatarTexturePath;
        }

        public virtual void SetAvatarTexturePathFor (string characterId, string avatarTexturePath)
        {
            if (!ActorExists(characterId))
            {
                Debug.LogWarning($"Failed to assign `{avatarTexturePath}` avatar texture to `{characterId}` character: character with the provided ID not found.");
                return;
            }

            if (!AvatarTextureExists(avatarTexturePath))
            {
                Debug.LogWarning($"Failed to assign `{avatarTexturePath}` avatar texture to `{characterId}` character: avatar texture with the provided path not found.");
                return;
            }

            charIdToAvatarPathMap.TryGetValue(characterId ?? string.Empty, out var initialPath);
            charIdToAvatarPathMap[characterId] = avatarTexturePath;

            if (initialPath != avatarTexturePath)
            {
                var avatarTexture = GetAvatarTextureFor(characterId);
                OnCharacterAvatarChanged?.Invoke(new CharacterAvatarChangedArgs(characterId, avatarTexture));
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// When using a non-source locale, will first attempt to find a corresponding record 
        /// in the managed text documents, and, if not found, check the character metadata.
        /// In case the display name is found and is wrapped in curly braces, will attempt to evaluate the value from the expression.
        /// </remarks>
        public virtual string GetDisplayName (string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId)) return null;

            var displayName = default(string);

            if (!localizationManager.IsSourceLocaleSelected())
                displayName = textManager.GetRecordValue(characterId, CharactersConfiguration.DisplayNamesCategory);

            if (string.IsNullOrEmpty(displayName))
                displayName = Configuration.GetMetadataOrDefault(characterId).DisplayName;

            if (!string.IsNullOrEmpty(displayName) && displayName.StartsWithFast("{") && displayName.EndsWithFast("}"))
            {
                var expression = displayName.GetAfterFirst("{").GetBeforeLast("}");
                displayName = ExpressionEvaluator.Evaluate<string>(expression, desc => Debug.LogError($"Failed to evaluate `{characterId}` character display name: {desc}"));
            }

            return string.IsNullOrEmpty(displayName) ? null : displayName;
        }

        public virtual CharacterLookDirection LookAtOriginDirection (float xPos)
        {
            if (Mathf.Approximately(xPos, GlobalSceneOrigin.x)) return CharacterLookDirection.Center;
            return xPos < GlobalSceneOrigin.x ? CharacterLookDirection.Right : CharacterLookDirection.Left;
        }

        public virtual async UniTask ArrangeCharactersAsync (bool lookAtOrigin = true, float duration = 0, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            var actors = ManagedActors?.Values
                .Where(c => c.Visible && !Configuration.GetMetadataOrDefault(c.Id).RenderTexture)
                .OrderBy(c => c.Id).ToList();
            if (actors is null || actors.Count == 0) return;

            var sceneWidth = CameraConfiguration.SceneRect.width;
            var arrangeRange = Configuration.ArrangeRange;
            var arrangeWidth = sceneWidth * (arrangeRange.y - arrangeRange.x);
            var stepSize = arrangeWidth / actors.Count;
            var xOffset = (sceneWidth * arrangeRange.x - sceneWidth * (1 - arrangeRange.y)) / 2;

            var tasks = new List<UniTask>();
            var evenCount = 1;
            var unevenCount = 1;
            for (int i = 0; i < actors.Count; i++)
            {
                var isEven = i.IsEven();
                var posX = xOffset;
                if (isEven)
                {
                    var step = (evenCount * stepSize) / 2f;
                    posX += -(arrangeWidth / 2f) + step;
                    evenCount++;
                }
                else
                {
                    var step = (unevenCount * stepSize) / 2f;
                    posX += arrangeWidth / 2f - step;
                    unevenCount++;
                }
                tasks.Add(actors[i].ChangePositionXAsync(posX, duration, easingType, asyncToken));

                if (lookAtOrigin)
                {
                    var lookDir = LookAtOriginDirection(posX);
                    tasks.Add(actors[i].ChangeLookDirectionAsync(lookDir, duration, easingType, asyncToken));
                }
            }
            await UniTask.WhenAll(tasks);
        }

        protected override async UniTask<ICharacterActor> ConstructActorAsync (string actorId)
        {
            var actor = await base.ConstructActorAsync(actorId);

            // When adding new character place it at the scene origin by default.
            actor.Position = new Vector3(GlobalSceneOrigin.x, GlobalSceneOrigin.y, actor.Position.z);

            var meta = Configuration.GetMetadataOrDefault(actorId);
            if (meta.HighlightWhenSpeaking)
                ApplyPose(actor, meta.NotSpeakingPose);

            return actor;
        }

        protected virtual void HandleAuthorHighlighting (PrintTextArgs args)
        {
            if (ManagedActors.Count == 0) return;

            var visibleActors = ManagedActors.Count(a => a.Value.Visible);

            foreach (var actor in ManagedActors.Values)
            {
                var actorMeta = Configuration.GetMetadataOrDefault(actor.Id);
                if (!actorMeta.HighlightWhenSpeaking) continue;
                var poseName = (actorMeta.HighlightCharacterCount > visibleActors || actor.Id == args.AuthorId) ? actorMeta.SpeakingPose : actorMeta.NotSpeakingPose;
                ApplyPose(actor, poseName, actorMeta.HighlightDuration, actorMeta.HighlightEasing);
            }

            if (string.IsNullOrEmpty(args.AuthorId) || !ActorExists(args.AuthorId)) return;
            var authorMeta = Configuration.GetMetadataOrDefault(args.AuthorId);
            if (authorMeta.HighlightWhenSpeaking && authorMeta.HighlightCharacterCount <= visibleActors && authorMeta.PlaceOnTop)
            {
                var topmostChar = ManagedActors.Values.OrderBy(c => c.Position.z).FirstOrDefault();
                if (topmostChar != null && !topmostChar.Id.EqualsFast(args.AuthorId))
                {
                    var authorChar = GetActor(args.AuthorId);
                    var authorZPos = authorChar.Position.z;
                    var topmostZPos = topmostChar.Position.z < authorZPos ? topmostChar.Position.z : topmostChar.Position.z - .1f;
                    authorChar.ChangePositionZAsync(topmostZPos, authorMeta.HighlightDuration, authorMeta.HighlightEasing).Forget();
                    topmostChar.ChangePositionZAsync(authorZPos, authorMeta.HighlightDuration, authorMeta.HighlightEasing).Forget();
                }
            }
        }

        protected virtual void ApplyPose (ICharacterActor actor, string poseName, float duration = 0, EasingType easingType = default)
        {
            if (string.IsNullOrEmpty(poseName)) return;
            var pose = Configuration.GetMetadataOrDefault(actor.Id).GetPoseOrNull<CharacterState>(poseName);
            if (pose is null) return;

            if (pose.IsPropertyOverridden(nameof(CharacterState.Appearance)))
                actor.ChangeAppearanceAsync(pose.ActorState.Appearance, duration, easingType).Forget();
            if (pose.IsPropertyOverridden(nameof(CharacterState.Position)))
                actor.ChangePositionAsync(pose.ActorState.Position, duration, easingType).Forget();
            if (pose.IsPropertyOverridden(nameof(CharacterState.Rotation)))
                actor.ChangeRotationAsync(pose.ActorState.Rotation, duration, easingType).Forget();
            if (pose.IsPropertyOverridden(nameof(CharacterState.Scale)))
                actor.ChangeScaleAsync(pose.ActorState.Scale, duration, easingType).Forget();
            if (pose.IsPropertyOverridden(nameof(CharacterState.Visible)))
                actor.ChangeVisibilityAsync(pose.ActorState.Visible, duration, easingType).Forget();
            if (pose.IsPropertyOverridden(nameof(CharacterState.LookDirection)))
                actor.ChangeLookDirectionAsync(pose.ActorState.LookDirection, duration, easingType).Forget();
            if (pose.IsPropertyOverridden(nameof(CharacterState.TintColor)))
                actor.ChangeTintColorAsync(pose.ActorState.TintColor, duration, easingType).Forget();
        }
    }
}
