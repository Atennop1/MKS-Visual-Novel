// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class GameStateSlot : ScriptableGridSlot
    {
        [Serializable]
        private class OnTitleTextChangedEvent : UnityEvent<string> { }

        public override string Id => SlotNumber.ToString();
        public int SlotNumber { get; private set; }
        public GameStateMap State { get; private set; }

        [ManagedText("DefaultUI")]
        protected static string EmptySlotLabel = "Empty";

        protected virtual Button DeleteButton => deleteButton;
        protected virtual RawImage ThumbnailImage => thumbnailImage;
        protected virtual Texture2D EmptySlotThumbnail => emptySlotThumbnail;

        [Tooltip("Format of the date set in the title. For available options see C# docs for date and time format strings.")]
        [SerializeField] private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        [Tooltip("Title template. `{N}` is replaced with the slot number, `{D}` — with the date (or empty label when the slot is empty).")]
        [SerializeField] private string titleTemplate = "{N}. {D}";
        [SerializeField] private Button deleteButton = default;
        [SerializeField] private RawImage thumbnailImage = default;
        [SerializeField] private Texture2D emptySlotThumbnail = default;
        [SerializeField] private OnTitleTextChangedEvent onTitleTextChanged = default;

        private Action<int> onClicked, onDeleteClicked;
        private ScriptableUIBehaviour deleteButtonBehaviour;
        private ISaveLoadUI saveLoadUI;

        public virtual void Initialize (Action<int> onClicked, Action<int> onDeleteClicked)
        {
            this.onClicked = onClicked;
            this.onDeleteClicked = onDeleteClicked;
            saveLoadUI = Engine.GetService<IUIManager>().GetUI<ISaveLoadUI>();
            if (Engine.GetService<IInputManager>().GetDelete() is IInputSampler deleteInput)
                deleteInput.OnStart += HandleDeleteInputActivated;
        }

        public virtual void Bind (int slotNumber, GameStateMap state)
        {
            State = state;
            SlotNumber = slotNumber;

            if (state is null)
            {
                DeleteButton.gameObject.SetActive(false);
                SetTitleText(titleTemplate.Replace("{N}", SlotNumber.ToString()).Replace("{D}", EmptySlotLabel));
                ThumbnailImage.texture = EmptySlotThumbnail;
            }
            else
            {
                DeleteButton.gameObject.SetActive(true);
                var date = state.SaveDateTime.ToString(dateFormat);
                SetTitleText(titleTemplate.Replace("{N}", SlotNumber.ToString()).Replace("{D}", date));
                ThumbnailImage.texture = state.Thumbnail;
            }
        }

        public override void OnPointerEnter (PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (deleteButtonBehaviour)
                deleteButtonBehaviour.Show();
        }

        public override void OnPointerExit (PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (deleteButtonBehaviour)
                deleteButtonBehaviour.Hide();
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(DeleteButton, ThumbnailImage);

            if (!EmptySlotThumbnail)
                emptySlotThumbnail = Texture2D.whiteTexture;
            DeleteButton.TryGetComponent<ScriptableUIBehaviour>(out deleteButtonBehaviour);
            DeleteButton.onClick.AddListener(HandleDeleteButtonClicked);
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy();

            DeleteButton.onClick.RemoveListener(HandleDeleteButtonClicked);
        }

        protected virtual void SetTitleText (string value)
        {
            onTitleTextChanged?.Invoke(value);
        }

        protected override void OnButtonClick ()
        {
            base.OnButtonClick();
            onClicked?.Invoke(SlotNumber);
        }

        protected virtual void HandleDeleteButtonClicked ()
        {
            onDeleteClicked?.Invoke(SlotNumber);
        }

        protected virtual void HandleDeleteInputActivated ()
        {
            if (saveLoadUI.Visible && gameObject.activeInHierarchy && Selected)
                onDeleteClicked?.Invoke(SlotNumber);
        }
    }
}
