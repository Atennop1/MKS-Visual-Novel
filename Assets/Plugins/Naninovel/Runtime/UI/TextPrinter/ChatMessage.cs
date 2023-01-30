// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Naninovel.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ChatMessage : ScriptableUIBehaviour
    {
        [System.Serializable]
        private class MessageTextChangedEvent : UnityEvent<string> { }

        public virtual string MessageText { get => messageText; set { messageText = value; onMessageTextChanged?.Invoke(value); } }
        public virtual string AuthorId { get; set; }
        public virtual Color MessageColor { get => messageFrameImage.color; set => messageFrameImage.color = value; }
        public virtual string ActorNameText { get => actorNamePanel.Text; set => actorNamePanel.Text = value; }
        public virtual Color ActorNameTextColor { get => actorNamePanel.TextColor; set => actorNamePanel.TextColor = value; }
        public virtual Texture AvatarTexture { get => avatarImage.texture; set { avatarImage.texture = value; avatarImage.gameObject.SetActive(value); } }

        protected virtual AuthorNamePanel ActorNamePanel => actorNamePanel;
        protected virtual Image MessageFrameImage => messageFrameImage;
        protected virtual RawImage AvatarImage => avatarImage;
        protected virtual bool Typing { get; private set; }

        [SerializeField] private AuthorNamePanel actorNamePanel = default;
        [SerializeField] private Image messageFrameImage = default;
        [SerializeField] private RawImage avatarImage = default;
        [Tooltip("Invoked when the message text is changed.")]
        [SerializeField] private MessageTextChangedEvent onMessageTextChanged = default;
        [SerializeField] private UnityEvent onStartTyping = default;
        [SerializeField] private UnityEvent onStopTyping = default;

        private string messageText;

        public virtual ChatMessageState GetState () => new ChatMessageState(MessageText, AuthorId);

        public virtual void SetIsTyping (bool typing)
        {
            if (typing == Typing) return;
            Typing = typing;
            if (Typing) onStartTyping?.Invoke();
            else onStopTyping?.Invoke();
        }
        
        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(actorNamePanel, messageFrameImage, avatarImage);
        }
    }
}
