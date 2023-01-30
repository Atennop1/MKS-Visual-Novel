// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Naninovel.UI
{
    public class ScriptNavigatorPanel : CustomUI
    {
        protected Transform ButtonsContainer => buttonsContainer;
        protected GameObject PlayButtonPrototype => playButtonPrototype;

        [SerializeField] private Transform buttonsContainer = null;
        [SerializeField] private GameObject playButtonPrototype = null;

        protected IScriptPlayer Player { get; private set; }
        protected IScriptManager ScriptManager { get; private set; }

        public override async UniTask ChangeVisibilityAsync (bool visible, float? duration = null, AsyncToken asyncToken = default)
        {
            await base.ChangeVisibilityAsync(visible, duration, asyncToken);
            if (visible) await LocateScriptsAsync(asyncToken);
        }

        public virtual void DestroyScriptButtons () => ObjectUtils.DestroyAllChildren(buttonsContainer);

        public virtual async UniTask LocateScriptsAsync (AsyncToken asyncToken = default)
        {
            var scripts = await ScriptManager.LocateScriptsAsync();
            asyncToken.ThrowIfCanceled();
            GenerateScriptButtons(scripts);
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(buttonsContainer, playButtonPrototype);
            Player = Engine.GetService<IScriptPlayer>();
            ScriptManager = Engine.GetService<IScriptManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            Player.OnPlay += HandlePlay;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            Player.OnPlay -= HandlePlay;
        }

        protected virtual void GenerateScriptButtons (IEnumerable<string> scriptNames)
        {
            DestroyScriptButtons();

            foreach (var name in scriptNames)
            {
                var scriptButton = Instantiate(playButtonPrototype, buttonsContainer, false);
                scriptButton.GetComponent<NavigatorPlayButton>().Initialize(this, name, Player);
            }
        }

        private void HandlePlay (Script script)
        {
            if (ScriptManager.Configuration.TitleScript == script.Name) return;
            Hide();
        }
    }
}
