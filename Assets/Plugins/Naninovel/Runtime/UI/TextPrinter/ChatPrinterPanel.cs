// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    /// <summary>
    /// A <see cref="UITextPrinterPanel"/> implementation for a chat-style printer.
    /// </summary>
    public class ChatPrinterPanel : UITextPrinterPanel
    {
        [System.Serializable]
        public new class GameState
        {
            public List<ChatMessageState> Messages;
            public string LastMessageText;
        }

        public override string PrintedText { get => printedText; set => SetPrintedText(value); }
        public override string AuthorNameText { get; set; }
        public override float RevealProgress
        {
            get => revealProgress;
            set
            {
                if (value == 0) DestroyAllMessages();
                else if (messageStack?.Count > 0 && messageStack.Peek() is ChatMessage message && message)
                    message.MessageText = lastMessageText;
            }
        }
        public override string Appearance { get; set; }

        protected virtual ScrollRect ScrollRect => scrollRect;
        protected virtual RectTransform MessagesContainer => messagesContainer;
        protected virtual ChatMessage MessagePrototype => messagePrototype;
        protected virtual ScriptableUIBehaviour InputIndicator => inputIndicator;
        protected virtual float RevealDelayModifier => revealDelayModifier;

        [SerializeField] private ScrollRect scrollRect = default;
        [SerializeField] private RectTransform messagesContainer = default;
        [SerializeField] private ChatMessage messagePrototype = default;
        [SerializeField] private ScriptableUIBehaviour inputIndicator = default;
        [SerializeField] private float revealDelayModifier = 3f;

        private Stack<ChatMessage> messageStack = new Stack<ChatMessage>();
        private ICharacterManager characterManager;
        private string lastAuthorId;
        private string printedText;
        private string lastMessageText;
        private float revealProgress = .1f;

        public override async UniTask RevealPrintedTextOverTimeAsync (float revealDelay, AsyncToken asyncToken)
        {
            var message = AddMessage(lastMessageText, lastAuthorId);
            message.SetIsTyping(true);
            revealProgress = .1f;

            if (revealDelay > 0 && lastMessageText != null)
            {
                await AsyncUtils.WaitEndOfFrameAsync(asyncToken);
                ScrollToBottom(); // Wait before scrolling, otherwise it's not scrolled.
                var revealDuration = lastMessageText.Count(char.IsLetterOrDigit) * revealDelay * revealDelayModifier;
                var revealStartTime = Time.time;
                var revealFinishTime = revealStartTime + revealDuration;
                while (revealFinishTime > Time.time && messageStack.Count > 0 && messageStack.Peek() == message)
                {
                    revealProgress = (Time.time - revealStartTime) / revealDuration;
                    await AsyncUtils.WaitEndOfFrameAsync(asyncToken);
                    if (asyncToken.Completed) break;
                }
            }

            ScrollToBottom();
            revealProgress = 1f;
            message.SetIsTyping(false);
        }

        public override void SetWaitForInputIndicatorVisible (bool visible)
        {
            if (visible) inputIndicator.Show();
            else inputIndicator.Hide();
        }

        public override void OnAuthorChanged (string authorId, CharacterMetadata authorMeta)
        {
            lastAuthorId = authorId;
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(scrollRect, messagesContainer, messagePrototype, inputIndicator);

            characterManager = Engine.GetService<ICharacterManager>();
        }

        protected virtual void SetPrintedText (string value)
        {
            printedText = value;

            if (messageStack.Count == 0 || string.IsNullOrEmpty(lastMessageText))
                lastMessageText = value;
            else
            {
                var previousText = string.Join(string.Empty, messageStack.Select(m => m.MessageText).Reverse());
                lastMessageText = value.GetAfterFirst(previousText);
            }
        }

        protected virtual ChatMessage AddMessage (string messageText, string authorId = null, bool instant = false)
        {
            var message = Instantiate(messagePrototype, messagesContainer, false);
            message.MessageText = messageText;
            message.AuthorId = authorId;

            if (!string.IsNullOrEmpty(authorId))
            {
                message.ActorNameText = characterManager.GetDisplayName(authorId) ?? authorId;
                message.AvatarTexture = CharacterManager.GetAvatarTextureFor(authorId);

                var meta = characterManager.Configuration.GetMetadataOrDefault(authorId);
                if (meta.UseCharacterColor)
                {
                    message.MessageColor = meta.MessageColor;
                    message.ActorNameTextColor = meta.NameColor;
                }
            }
            else
            {
                message.ActorNameText = string.Empty;
                message.AvatarTexture = null;
            }

            if (instant) message.Visible = true;
            else message.Show();

            messageStack.Push(message);
            return message;
        }

        protected virtual void DestroyAllMessages ()
        {
            while (messageStack.Count > 0)
            {
                var message = messageStack.Pop();
                ObjectUtils.DestroyOrImmediate(message.gameObject);
            }
        }

        protected override void SerializeState (GameStateMap stateMap)
        {
            base.SerializeState(stateMap);

            var state = new GameState {
                Messages = messageStack.Select(m => m.GetState()).Reverse().ToList(),
                LastMessageText = lastMessageText
            };
            stateMap.SetState(state);
        }

        protected override async UniTask DeserializeState (GameStateMap stateMap)
        {
            await base.DeserializeState(stateMap);

            DestroyAllMessages();
            lastMessageText = null;

            var state = stateMap.GetState<GameState>();
            if (state is null) return;

            if (state.Messages?.Count > 0)
                foreach (var message in state.Messages)
                    AddMessage(message.PrintedText, message.AuthorId, true);

            lastMessageText = state.LastMessageText;

            ScrollToBottom();
        }

        private async void ScrollToBottom ()
        {
            // Wait a frame and force rebuild layout before setting scroll position,
            // otherwise it's ignoring recently added messages.
            await AsyncUtils.WaitEndOfFrameAsync();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.verticalNormalizedPosition = 0;
        }
    }
}
