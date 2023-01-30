// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class NavigatorSyncButton : ScriptableButton
    {
        private Image syncImage;
        private IScriptManager scriptManager;
        private bool loadingScripts;
        private ScriptNavigatorPanel panel;

        protected override void Awake ()
        {
            base.Awake();

            syncImage = GetComponentInChildren<Image>();
            panel = GetComponentInParent<ScriptNavigatorPanel>();
            this.AssertRequiredObjects(syncImage, panel);

            scriptManager = Engine.GetService<IScriptManager>();
            UIComponent.interactable = false;
        }
        
        protected override void OnEnable ()
        {
            base.OnEnable();

            scriptManager.OnScriptLoadStarted += HandleScriptLoadStarted;
            scriptManager.OnScriptLoadCompleted += HandleScriptLoadFinished;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            scriptManager.OnScriptLoadStarted -= HandleScriptLoadStarted;
            scriptManager.OnScriptLoadCompleted -= HandleScriptLoadFinished;
        }

        protected override void Update ()
        {
            base.Update();

            if (scriptManager is null || !scriptManager.ScriptNavigator || !scriptManager.ScriptNavigator.Visible) return;
            if (loadingScripts) syncImage.rectTransform.Rotate(new Vector3(0, 0, -99) * Time.unscaledDeltaTime);
            else syncImage.rectTransform.rotation = Quaternion.identity;
        }

        protected override void OnButtonClick ()
        {
            panel.LocateScriptsAsync().Forget();
        }

        private void HandleScriptLoadStarted ()
        {
            loadingScripts = true;
            UIComponent.interactable = false;
        }

        private void HandleScriptLoadFinished ()
        {
            loadingScripts = false;
            UIComponent.interactable = true;
        }
    }
}
