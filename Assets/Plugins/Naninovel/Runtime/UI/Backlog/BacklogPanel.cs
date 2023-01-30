// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class BacklogPanel : CustomUI, IBacklogUI
    {
        [Serializable]
        public new class GameState
        {
            public List<BacklogMessageState> Messages;
        }

        protected virtual BacklogMessage LastMessage => messages.Last?.Value;
        protected virtual RectTransform MessagesContainer => messagesContainer;
        protected virtual ScrollRect ScrollRect => scrollRect;
        protected virtual BacklogMessage MessagePrefab => messagePrefab;
        protected virtual bool ClearOnLoad => clearOnLoad;
        protected virtual bool ClearOnLocaleChange => clearOnLocaleChange;
        protected virtual int Capacity => capacity;
        protected virtual int SaveCapacity => saveCapacity;
        protected virtual bool StripTags => stripTags;
        protected virtual bool AddChoices => addChoices;
        protected virtual bool AllowReplayVoice => allowReplayVoice;
        protected virtual bool AllowRollback => allowRollback;

        [SerializeField] private RectTransform messagesContainer = default;
        [SerializeField] private ScrollRect scrollRect = default;
        [SerializeField] private BacklogMessage messagePrefab = default;
        [Tooltip("Whether to clear the backlog when loading another script or resetting state (exiting to title).")]
        [SerializeField] private bool clearOnLoad = true;
        [Tooltip("Whether to clear the backlog when changing game localization (language).")]
        [SerializeField] private bool clearOnLocaleChange = true;
        [Tooltip("How many messages should the backlog keep.")]
        [SerializeField] private int capacity = 100;
        [Tooltip("How many messages should the backlog keep when saving the game.")]
        [SerializeField] private int saveCapacity = 30;
        [Tooltip("Whether to strip formatting content (content inside `<` `>` and the angle brackets themselves) from the added messages.")]
        [SerializeField] private bool stripTags = true;
        [Tooltip("Whether to add choices summary to the log.")]
        [SerializeField] private bool addChoices = true;
        [Tooltip("Template to use for selected choice summary. " + choiceTemplateLiteral + " will be replaced with the actual choice summary.")]
        [SerializeField] private string selectedChoiceTemplate = $"    <b>{choiceTemplateLiteral}</b>";
        [Tooltip("Template to use for other (not selected) choice summary. " + choiceTemplateLiteral + " will be replaced with the actual choice summary.")]
        [SerializeField] private string otherChoiceTemplate = $"    <color=#ffffff88>{choiceTemplateLiteral}</color>";
        [Tooltip("String added between consequent choices.")]
        [SerializeField] private string choiceSeparator = "<br>";
        [Tooltip("Whether to allow replaying voices associated with the backlogged messages.")]
        [SerializeField] private bool allowReplayVoice = true;
        [Tooltip("Whether to allow rolling back to playback spots associated with the backlogged messages.")]
        [SerializeField] private bool allowRollback = true;

        private const int messageLengthLimit = 10000; // Due to Unity's mesh verts count limit.
        private const string choiceTemplateLiteral = "%SUMMARY%";

        /// <summary>
        /// Capture formatting content enclosed in angle brackets.
        /// </summary>
        private static readonly Regex formattingRegex = new Regex(@"<.*?>", RegexOptions.Compiled);

        private readonly LinkedList<BacklogMessage> messages = new LinkedList<BacklogMessage>();
        private readonly Stack<BacklogMessage> messagesPool = new Stack<BacklogMessage>();
        private IInputManager inputManager;
        private ICharacterManager charManager;
        private IStateManager stateManager;
        private ILocalizationManager localizationManager;
        private bool clearPending;

        public virtual void Clear ()
        {
            foreach (var message in messages)
            {
                message.gameObject.SetActive(false);
                messagesPool.Push(message);
            }
            messages.Clear();
        }

        public virtual void AddMessage (string messageText, string actorId = null, string voiceClipName = null, PlaybackSpot? rollbackSpot = null)
        {
            if (StripTags) messageText = StripFormatting(messageText);

            var actorNameText = charManager.GetDisplayName(actorId) ?? actorId;
            SpawnMessage(messageText, actorNameText,
                (AllowReplayVoice && !string.IsNullOrEmpty(voiceClipName)) ? new List<string> { voiceClipName } : null,
                (AllowRollback && rollbackSpot.HasValue) ? rollbackSpot.Value : PlaybackSpot.Invalid);
        }

        public virtual void AppendMessage (string message, string voiceClipName = null, PlaybackSpot? rollbackSpot = null)
        {
            if (!ObjectUtils.IsValid(LastMessage)) return;

            if (StripTags) message = StripFormatting(message);

            if ((LastMessage.Message.Length + message.Length) > messageLengthLimit)
            {
                SpawnMessage(message, LastMessage.Author,
                    (AllowReplayVoice && !string.IsNullOrEmpty(voiceClipName)) ? new List<string> { voiceClipName } : null,
                    (AllowRollback && rollbackSpot.HasValue) ? rollbackSpot.Value : PlaybackSpot.Invalid);
                return;
            }

            LastMessage.Append(message, voiceClipName);
        }

        public virtual void AddChoice (List<Tuple<string, bool>> summary)
        {
            if (!AddChoices || summary.All(t => string.IsNullOrWhiteSpace(t.Item1))) return;

            var messages = summary.Select(t => t.Item2 ? selectedChoiceTemplate.Replace(choiceTemplateLiteral, t.Item1) : otherChoiceTemplate.Replace(choiceTemplateLiteral, t.Item1));
            var separator = choiceSeparator.Replace("<br>", Environment.NewLine);
            var messageText = string.Join(separator, messages);

            SpawnMessage(messageText, null, null, PlaybackSpot.Invalid);
        }

        public override void SetVisibility (bool visible)
        {
            if (visible) ScrollToBottom();
            base.SetVisibility(visible);
        }

        public override UniTask ChangeVisibilityAsync (bool visible, float? duration = null, AsyncToken asyncToken = default)
        {
            if (visible) ScrollToBottom();
            return base.ChangeVisibilityAsync(visible, duration, asyncToken);
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(messagesContainer, scrollRect, messagePrefab);

            inputManager = Engine.GetService<IInputManager>();
            charManager = Engine.GetService<ICharacterManager>();
            stateManager = Engine.GetService<IStateManager>();
            localizationManager = Engine.GetService<ILocalizationManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            if (inputManager?.GetShowBacklog() != null)
                inputManager.GetShowBacklog().OnStart += Show;

            if (ClearOnLoad)
            {
                stateManager.OnGameLoadStarted += Clear;
                stateManager.OnResetStarted += Clear;
            }

            if (ClearOnLocaleChange)
            {
                localizationManager.OnLocaleChanged += SetClearPending;
                stateManager.OnGameLoadFinished += ClearIfPending;
            }
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            if (inputManager?.GetShowBacklog() != null)
                inputManager.GetShowBacklog().OnStart -= Show;

            if (stateManager != null)
            {
                stateManager.OnGameLoadStarted -= Clear;
                stateManager.OnGameLoadFinished -= ClearIfPending;
                stateManager.OnResetStarted -= Clear;
            }

            if (localizationManager != null)
                localizationManager.OnLocaleChanged -= SetClearPending;
        }

        protected virtual void SpawnMessage (string messageText, string actorNameText,
            IReadOnlyCollection<string> voiceClipNames, PlaybackSpot rollbackSpot)
        {
            var message = default(BacklogMessage);

            if (messages.Count > Capacity)
            {
                message = messages.First.Value;
                message.gameObject.SetActive(true);
                message.transform.SetSiblingIndex(MessagesContainer.childCount - 1);
                messages.RemoveFirst();
                messages.AddLast(message);
            }
            else
            {
                if (messagesPool.Count > 0)
                {
                    message = messagesPool.Pop();
                    message.gameObject.SetActive(true);
                    message.transform.SetSiblingIndex(MessagesContainer.childCount - 1);
                }
                else message = Instantiate(MessagePrefab, MessagesContainer, false);

                messages.AddLast(message);
            }

            message.Initialize(messageText, actorNameText, voiceClipNames, rollbackSpot);
        }

        protected override void HandleVisibilityChanged (bool visible)
        {
            base.HandleVisibilityChanged(visible);

            MessagesContainer.gameObject.SetActive(visible);
        }

        protected override void SerializeState (GameStateMap stateMap)
        {
            base.SerializeState(stateMap);
            var state = new GameState {
                Messages = messages.TakeLast(SaveCapacity).Select(m => m.GetState()).ToList()
            };
            stateMap.SetState(state);
        }

        protected override async UniTask DeserializeState (GameStateMap stateMap)
        {
            await base.DeserializeState(stateMap);

            Clear();

            var state = stateMap.GetState<GameState>();
            if (state is null) return;

            if (state.Messages?.Count > 0)
                foreach (var messageState in state.Messages)
                    SpawnMessage(messageState.MessageText, messageState.ActorNameText, messageState.VoiceClipNames, messageState.RollbackSpot);
        }

        protected virtual string StripFormatting (string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return content;
            return formattingRegex.Replace(content, string.Empty);
        }

        private async void ScrollToBottom ()
        {
            // Wait a frame and force rebuild layout before setting scroll position,
            // otherwise it's ignoring recently added messages.
            await AsyncUtils.WaitEndOfFrameAsync();
            LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.content);
            ScrollRect.verticalNormalizedPosition = 0;
        }

        private void SetClearPending (string locale) => clearPending = true;

        private void Clear (GameSaveLoadArgs args) => Clear();

        private void ClearIfPending (GameSaveLoadArgs args)
        {
            if (!clearPending) return;
            clearPending = false;
            Clear();
        }
    }
}
