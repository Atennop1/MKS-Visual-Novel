// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine.EventSystems;

namespace Naninovel.UI
{
    /// <summary>
    /// Represents a full-screen invisible UI panel, which blocks UI interaction and all (or a subset of) the input samplers while visible,
    /// but can hide itself and execute (if registered) `onClick` callback when user clicks the panel.
    /// </summary>
    public class ClickThroughPanel : CustomUI, IPointerClickHandler
    {
        private IInputManager inputManager;
        private Action onClick;
        private bool hideOnClick;

        public virtual void Show (bool hideOnClick, Action onClick, params string[] allowedSamplers)
        {
            this.hideOnClick = hideOnClick;
            this.onClick = onClick;
            Show();
            inputManager.AddBlockingUI(this, allowedSamplers);
        }

        public override void Hide ()
        {
            onClick = null;
            inputManager.RemoveBlockingUI(this);
            base.Hide();
        }

        public virtual void OnPointerClick (PointerEventData eventData)
        {
            onClick?.Invoke();
            if (hideOnClick) Hide();
        }

        protected override void Awake ()
        {
            base.Awake();

            inputManager = Engine.GetService<IInputManager>();
        }
    }
}
