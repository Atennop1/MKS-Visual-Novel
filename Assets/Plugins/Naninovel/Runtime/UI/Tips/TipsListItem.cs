// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Naninovel
{
    public class TipsListItem : MonoBehaviour
    {
        [Serializable]
        private class OnLabelChangedEvent : UnityEvent<string> { }
        [Serializable]
        private class OnLabelStyleChangedEvent : UnityEvent<FontStyle> { }

        public virtual string UnlockableId { get; private set; }
        public virtual int Number => transform.GetSiblingIndex() + 1;

        protected virtual Button Button => button;
        protected virtual GameObject SelectedIndicator => selectedIndicator;

        [Tooltip("Tip label template. `{N}` will be replaced with the record number, `{T}` — with the title.")]
        [SerializeField] private string template = "{N}. {T}";
        [Tooltip("Record title to set when the tip item is locked.")]
        [SerializeField] private string lockedTitle = "???";
        [Tooltip("The tip button.")]
        [SerializeField] private Button button = default;
        [Tooltip("When assigned, the game object will be activated when the tip is selected.")]
        [SerializeField] private GameObject selectedIndicator = default;
        [SerializeField] private OnLabelChangedEvent onLabelChanged = default;
        [SerializeField] private OnLabelStyleChangedEvent onLabelStyleChanged = default;

        private Action<TipsListItem> onClick;
        private string title;
        private bool selectedOnce;

        public static TipsListItem Instantiate (TipsListItem prototype, string unlockableId,
            string title, bool selectedOnce, Action<TipsListItem> onClick)
        {
            var item = Instantiate(prototype);

            item.onClick = onClick;
            item.UnlockableId = unlockableId;
            item.title = title;
            item.selectedOnce = selectedOnce;

            return item;
        }

        public virtual void SetSelected (bool selected)
        {
            if (selected)
            {
                selectedOnce = true;
                SetLabelStyle(FontStyle.Normal);
            }
            if (SelectedIndicator)
                SelectedIndicator.SetActive(selected);
        }

        public virtual void SetUnlocked (bool unlocked)
        {
            SetLabel(template.Replace("{N}", Number.ToString()).Replace("{T}", unlocked ? title : lockedTitle));
            SetLabelStyle(!unlocked || selectedOnce ? FontStyle.Normal : FontStyle.Bold);
            Button.interactable = unlocked;
        }

        protected virtual void Awake ()
        {
            this.AssertRequiredObjects(Button);
            if (SelectedIndicator)
                SelectedIndicator.SetActive(false);
        }

        protected virtual void OnEnable ()
        {
            Button.onClick.AddListener(HandleButtonClicked);
        }

        protected virtual void OnDisable ()
        {
            Button.onClick.RemoveListener(HandleButtonClicked);
        }

        protected virtual void SetLabel (string value)
        {
            onLabelChanged?.Invoke(value);
        }

        protected virtual void SetLabelStyle (FontStyle value)
        {
            onLabelStyleChanged?.Invoke(value);
        }

        protected virtual void HandleButtonClicked ()
        {
            onClick?.Invoke(this);
        }
    }
}
